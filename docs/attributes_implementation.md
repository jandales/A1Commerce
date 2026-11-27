# Attributes Implementation

A store-wide catalog of reusable attributes, e.g. "Size" or "Color", managed by administrators.
Built on the i25Core foundation: `BaseEntity`, `BaseBusinessLayer<T>`, `BaseController<T>`.

This is separate from `ProductAttribute` (product catalog), which belongs to a single product.
Attribute values (e.g. "Red", "Small") are **not** part of this item.

## How to implement

Implement with the **`/create-i21core-entity`** skill (argument: `Attribute`), following its steps in order:

| Skill step | For Attributes |
|---|---|
| 1. Create Entity Model | `Core/Entities/Attribute.cs`, `Attribute : BaseEntity` |
| 2. Create Entity Mapping | `Core/Configuration/AttributeMap.cs` |
| 3. Register in DbContext | `DbSet<Attribute> Attributes` + `ApplyConfiguration(new AttributeMap())` in `I25DbContext` |
| 4. Business layer interface | `IAttributeBusinessLayer : IBusinessLayer<Attribute>` |
| 5. Business layer | `AttributeBusinessLayer : BaseBusinessLayer<Attribute>, IAttributeBusinessLayer` |
| 6. Register in Program.cs | `AddScoped<IAttributeBusinessLayer, AttributeBusinessLayer>()` |
| 7. API controller | `AttributeController : BaseController<Attribute>` |

Where this plan differs from the skill's sample code:

- **Class name `Attribute`** shares its name with `System.Attribute`. In a file that uses both `System` (implicit usings)
  and `A1Commerce.Core.Entities` and mentions `Attribute`, write `System.Attribute` or `Entities.Attribute` in full to
  avoid an ambiguity error. Table `Attributes`, controller `AttributeController`, URL `/api/Attribute`.
- **No `INotifiable`** (step 1): an attribute never receives notifications.
- **Non-generic business layer** (step 5): use the concrete form above, as `CategoryBusinessLayer` does.
- **Steps the skill does not cover**, done after it: the validator, the access rule and the migration (sections below).

## Database structure

Table: `Attributes`

| Column | Type | C# property | Rules |
|---|---|---|---|
| Id | int | `id` (from `BaseEntity`) | Primary key |
| Name | nvarchar(100) | `string Name` | Required, unique (e.g. "Size") |
| Slug | nvarchar(120) | `string Slug` | Required, unique, lowercase letters, digits and hyphens |
| Description | nvarchar(500) | `string? Description` | Optional |
| DisplayOrder | int | `int DisplayOrder` | Default 0 |
| IsActive | bit | `bool IsActive` | Default true |

Added automatically by `BaseEntity`: `intConcurrencyId`, `CreateAt`, `ModifiedAt`.

## Mapping (`AttributeMap`)

- `ToTable("Attributes")`
- `Name` and `Slug` required; unique index on each.
- `DisplayOrder` default `0`, `IsActive` default `true`.

## Validation (`Web/Validator/AttributeValidator.cs`)

- `Name`: required, max 100.
- `Slug`: required, max 120, `^[a-z0-9]+(-[a-z0-9]+)*$`.
- `Description`: max 500.
- `DisplayOrder`: 0 or more.
- Uniqueness is enforced by the unique indexes.

## Access

Administrators only: `[Authorize(Roles = Roles.Admin)]` on `AttributeController`, the same as `CategoryController`.

## API endpoints

From `BaseController<T>`, at `/api/Attribute` and `/api/v1/Attribute`:

| Method | Route | Action |
|---|---|---|
| GET | `/api/Attribute` | All attributes |
| GET | `/api/Attribute/{id}` | One attribute |
| GET | `/api/Attribute/get` | Filtering, sorting, paging |
| POST | `/api/Attribute` | Create |
| PUT | `/api/Attribute` | Update |
| DELETE | `/api/Attribute` | Delete |

## Migration

`dotnet ef migrations add AddAttributes --project A1Commerce.Infrastructure/A1Commerce.Infrastructure --startup-project A1Commerce.Web`
then `dotnet ef database update` (local EF Core 8 tool from `dotnet-tools.json`).

Before generating it, commit or merge the in-progress product catalog work: both change `I25DbContext`, `Program.cs`
and the model snapshot.

## Validation checklist

- [ ] `dotnet build` with 0 errors
- [ ] `Attributes` table created with unique indexes on `Name` and `Slug`
- [ ] CRUD works through Swagger as Admin
- [ ] Duplicate name/slug and invalid slug are rejected
- [ ] Non-admin gets 403, anonymous gets 401

## Commit

One commit on branch `feature/attributes`: model, mapping, business layer, controller, validator, migration.
