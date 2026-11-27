# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Overview

A1Commerce is an ASP.NET Core 8 Web API (SQL Server, EF Core 9) built on the **i25Core** foundation. Most cross-cutting features (auth, auditing, events, background jobs, mail, notifications, caching, storage, localization, versioning, rate limiting, health checks) are already wired; new work is mostly adding domain entities on top of the generic CRUD stack. There is no test project yet.

Feature plans live in `docs/` (e.g. `docs/category_implementation.md`). Read the relevant plan before implementing a feature.

## Commands

The solution file is inside the Web project folder: `A1Commerce.Web/A1Commerce.Web.sln`.

```bash
dotnet build A1Commerce.Web/A1Commerce.Web.sln
dotnet run --project A1Commerce.Web                 # http://localhost:5288, Swagger at /swagger (Development only)
dotnet run --project A1Commerce.Web --launch-profile https   # https://localhost:7078
```

EF Core migrations (DbContext is `I25DbContext` in Infrastructure). The global `dotnet-ef` 11.x tool cannot load this net8.0 project; use a local 8.x tool:

```bash
dotnet new tool-manifest && dotnet tool install dotnet-ef --version 8.*   # once
dotnet ef migrations add <Name> --project A1Commerce.Infrastructure/A1Commerce.Infrastructure --startup-project A1Commerce.Web
dotnet ef database update --project A1Commerce.Infrastructure/A1Commerce.Infrastructure --startup-project A1Commerce.Web
```

The connection string in `appsettings.json` points at a local SQL Server (`Trusted_Connection`). `Auth:Jwt:Key` is empty: in Development a temporary key is generated (everyone is signed out on restart); outside Development startup fails unless it is set (use user secrets).

## Project layout and dependencies

Note the doubled folder nesting for two projects:

- `A1Commerce.Core/A1Commerce.Core/`: entities, EF mappings (`Configuration/*Map.cs`, `IEntityTypeConfiguration`), contracts/interfaces (`ICacheService`, `IMailer`, `IFileStorage`, `ITranslator`, events, `Roles`), `ApiResponse` DTO, exceptions.
- `A1Commerce.Infrastructure/A1Commerce.Infrastructure/`: `I25DbContext`, migrations, `DbSeed`, implementations of Core contracts. `Extensions/ServiceCollectionExtension.cs` exposes `AddI25Infrastructure`.
- `A1Commerce.BusinessLayer/`: `BaseBusinessLayer<T>`, `IRepository`/`BaseRepository` (note the file is misspelled `BaseRepositoty.cs`), dynamic filter/sort/paging (`Extensions/`, `Parameters/`).
- `A1Commerce.Web/`: controllers, validators, middleware, and one `*ServiceExtensions.cs` per feature (`AddI25X` / `UseI25X`) called from `Program.cs`. Configuration for each feature is a named section in `appsettings.json`.

References: Web → BusinessLayer, Core, Infrastructure; BusinessLayer → Core, Infrastructure; Infrastructure → Core.

## Generic CRUD stack

- **Entities** inherit `BaseEntity` (`id` — lowercase —, `intConcurrencyId`, `CreateAt`, `ModifiedAt`). `I25DbContext` marks `intConcurrencyId` as a concurrency token for every `BaseEntity`, increments it on update, and sets the timestamps on save.
- **Business layer**: `IBusinessLayer<T>` / `BaseBusinessLayer<T>` (registered open-generic in `Program.cs`) provide `AddAsync`, `UpdateSync` (sic), `DeleteAsync`, `GetByIdAsync`, `GetAllAsync`, and `GetAsync(GetRequest)` for filters/sorts/paging. Entity-specific layers are concrete classes: `XBusinessLayer : BaseBusinessLayer<X>, IXBusinessLayer`, registered with `AddScoped` in `Program.cs`.
- **Controllers** inherit `BaseController<T>`, which exposes POST/PUT/DELETE `/api/X`, GET `/api/X`, GET `/api/X/{id}`, GET `/api/X/get` (filter/sort/page via `GetRequestModelBinder`). Every controller is routed at both `/api/[controller]` and `/api/v{version}/[controller]`. Writes evict the controller's `[CacheResponse]` output-cache entries.
- **Validation**: FluentValidation validators (`BaseValidator<T>`) are auto-registered from the Web assembly; `BaseController` resolves `IValidator<T>` on create/update. Errors and responses all use the `ApiResponse<T>` envelope; unhandled exceptions are mapped by `Middleware/GlobalExceptionHandler`.
- The `/create-i21core-entity` skill scaffolds entity → map → DbContext registration → business layer → `Program.cs` registration → controller. Its sample business layer is generic (`XBusinessLayer<T>`) and does not compile as intended; use the concrete form above. It does not cover validators, authorization, or migrations.

## Behaviour that happens implicitly on SaveChanges

`I25DbContext.SaveChanges[Async]`:
1. Writes audit rows (`AuditLogs`/`AuditLogChange`) in the same transaction as the data change. Exclude secrets or noisy fields with `[AuditIgnore]` (property or class).
2. After commit, dispatches automatic Created/Updated/Deleted model events to `IEventListener<T>` implementations (auto-registered from the Web assembly). `[NoModelEvents]` on an entity disables them; `[Queued]` on a listener runs it via Hangfire.

## Auth

JWT access tokens + refresh tokens. `Auth:RequireAuthenticatedByDefault = true` installs a fallback policy, so **every endpoint requires a signed-in user unless marked `[AllowAnonymous]`**. Roles are `Roles.Admin` and `Roles.User`. `BaseController` actions carry no role attributes, so per-entity access rules must be added explicitly (e.g. class-level `[Authorize(Roles = Roles.Admin)]`).

## Localization

User-facing messages are translation keys resolved from `A1Commerce.Web/lang/{en,es}/*.json` (`Translate("messages.created")` in controllers, `ITranslator` elsewhere). Add new keys to both languages.
