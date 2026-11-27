# Product Catalog Implementation Specification

## 1. Scope

Implement the product catalog using these four entities:

1. `Product`
2. `ProductAttribute`
3. `ProductAttributeValue`
4. `ProductImage`

This specification focuses on product information, product attributes and values, and product images. Product variants and inventory management are intentionally not implemented in this phase.

It builds on two existing features: `Category` (`/api/Category`) and the store-wide `Attribute` catalog (`/api/Attribute`, see `attributes_implementation.md`).

## 2. Core concepts

- A **Product** is the item displayed in the store, such as `Classic T-Shirt`.
- A **ProductAttribute** links a product to a store-wide `Attribute`, such as `Color` or `Size`, and holds that product's values for it.
- A **ProductAttributeValue** is one option for a product attribute, such as `Red`, `Blue`, `Small`, or `Large`.
- A **ProductImage** stores images that belong to the product gallery.
- Each attribute value can have its own optional image. For example, `Red` can show a red shirt image and `Blue` can show a blue shirt image.

Important: an attribute value is not automatically a sellable SKU. A combination such as `Red + Small` is a product variant. Variant-level SKU, barcode, price, and stock should be designed separately when variant support is implemented.

## 3. Entity definitions

All four entities inherit `BaseEntity` (`id`, `intConcurrencyId`, `CreateAt`, `ModifiedAt`). The models carry no data annotations: column sizes are set in the `*Map` classes and input rules in the `*Validator` classes. Back-references to a parent (`Product`, `ProductAttribute`, `Category`) are `[JsonIgnore]` and nullable, so the API returns a tree without circular references.

### 3.1 Product

| Field | Type | Notes |
|---|---|---|
| `Name` | string | Required, max 200 |
| `Slug` | string | Required, max 220, unique, lowercase letters, digits and hyphens |
| `Description` | string? | Max 4000 |
| `ShortDescription` | string? | Max 500 |
| `CategoryId` | int | Required, links to `Category` |
| `RegularPrice` | decimal | `decimal(18,2)`, not negative |
| `SalePrice` | decimal? | `decimal(18,2)`, not negative, lower than `RegularPrice` |
| `IsActive` | bool | Default `true`; controls storefront visibility |
| `IsFeatured` | bool | Default `false` |

Navigation:

- `Category` (`[JsonIgnore]`)
- `Images`
- `Attributes`

Do not add variant or inventory navigation in this phase.

### 3.2 ProductAttribute

| Field | Type | Notes |
|---|---|---|
| `ProductId` | int | Required parent product |
| `AttributeId` | int | Required. Links to the store-wide `Attribute` (e.g. `Color`), managed at `/api/Attribute` |
| `DisplayOrder` | int | Controls display order, 0 or more |

Navigation properties:

- `Product` (`[JsonIgnore]`)
- `Attribute`: returned by product detail so the storefront has the attribute's name; ignored when sent in a request
- `Values`

The attribute itself (`Color`) is shared through the store-wide `Attribute` catalog; the `ProductAttribute` row links one product to it, and its values belong to that product. For example, two products can both use `Color`, each with its own values. A product can use each attribute only once.

### 3.3 ProductAttributeValue

| Field | Type | Notes |
|---|---|---|
| `ProductAttributeId` | int | Required parent product attribute |
| `Value` | string | Required, max 100, e.g. `Red`; unique within its product attribute (ignoring case) |
| `ImagePath` | string? | Max 512. Optional image for this specific value |
| `DisplayOrder` | int | Controls option display order, 0 or more |

Navigation property:

- `ProductAttribute` (`[JsonIgnore]`)

C# model:

```csharp
public class ProductAttributeValue : BaseEntity
{
    public int ProductAttributeId { get; set; }

    public string Value { get; set; } = string.Empty;

    public string? ImagePath { get; set; }

    public int DisplayOrder { get; set; }

    [JsonIgnore]
    public ProductAttribute? ProductAttribute { get; set; }
}
```

The back-reference is nullable on purpose: a non-nullable reference property is treated as required by ASP.NET Core model validation, so a request without it would be rejected.

**SKU and barcode decision:** Do not put SKU or barcode on `ProductAttributeValue` by default. A value such as `Red` can be used in multiple combinations (`Red + Small`, `Red + Large`), and those combinations may need different SKUs and barcodes. Add SKU and barcode to a future `ProductVariant` entity if each combination is independently purchasable. If the business later confirms that an individual attribute value itself is a sellable item, revisit this rule before adding those fields.

**Quantity decision:** Do not store quantity on `ProductAttributeValue`. Quantity is inventory data and should be stored in an inventory/stock entity, potentially per location or warehouse. Do not use a product option's image or value record as the stock ledger.

### 3.4 ProductImage

| Field | Type | Notes |
|---|---|---|
| `ProductId` | int | Required parent product |
| `Path` | string | Required, max 512. Stored image URL or path |
| `AltText` | string? | Max 300. Accessible image description |
| `IsPrimary` | bool | Marks the main gallery image; exactly one per product when it has images |
| `SortOrder` | int | Gallery display order, 0 or more |

Navigation property:

- `Product` (`[JsonIgnore]`)

Use `ProductImage` for the product's general gallery. Use `ProductAttributeValue.ImagePath` for an image that represents a particular option. These are separate purposes; do not duplicate an option image in the gallery unless it should also appear there.

## 4. Relationships

```text
Category                Attribute (store-wide)
  |                       |
  | 1-to-many             | 1-to-many
  v                       v
Product --1-to-many--> ProductAttribute
  |                       |
  | 1-to-many             | 1-to-many
  v                       v
ProductImage            ProductAttributeValue
```

Expected relationships:

- One `Category` has many `Product` records.
- One `Product` has many `ProductAttribute` records.
- One store-wide `Attribute` is used by many `ProductAttribute` records (at most one per product).
- One `ProductAttribute` has many `ProductAttributeValue` records.
- One `Product` has many `ProductImage` records.
- One `ProductAttributeValue` has zero or one option image path.

## 5. Example data

Product: `Classic T-Shirt` (id 100)

### ProductAttribute

| Id | ProductId | AttributeId | DisplayOrder |
|---:|---:|---:|---:|
| 1 | 100 | 1 | 1 |
| 2 | 100 | 2 | 2 |

`AttributeId` 1 is the store-wide `Color` attribute and 2 is `Size`.

### ProductAttributeValue

| Id | ProductAttributeId | Value | ImagePath | DisplayOrder |
|---:|---:|---|---|---:|
| 1 | 1 | Red | `/storage/products/100/options/red.jpg` | 1 |
| 2 | 1 | Blue | `/storage/products/100/options/blue.jpg` | 2 |
| 3 | 1 | Black | `/storage/products/100/options/black.jpg` | 3 |
| 4 | 2 | Small | null | 1 |
| 5 | 2 | Medium | null | 2 |
| 6 | 2 | Large | null | 3 |

The storefront can use the selected color value's `ImagePath` to update the displayed product image. Size values do not need images unless the business wants them.

## 6. EF Core configuration and database constraints

Configured in `ProductMap`, `ProductAttributeMap`, `ProductAttributeValueMap` and `ProductImageMap` (migration `AddProductCatalog`):

| Table | Constraint |
|---|---|
| `Products` | Unique index on `Slug`; FK `CategoryId` → `Categories` with **Restrict** (a category that has products cannot be deleted) |
| `ProductAttributes` | FK `ProductId` → `Products` with **Cascade**; FK `AttributeId` → `Attributes` with **Restrict** (an attribute used by a product cannot be deleted); unique index on `(ProductId, AttributeId)` |
| `ProductAttributeValues` | FK `ProductAttributeId` → `ProductAttributes` with **Cascade**; unique index on `(ProductAttributeId, Value)` |
| `ProductImages` | FK `ProductId` → `Products` with **Cascade**; index on `ProductId`; filtered unique index `IX_ProductImages_ProductId_Primary` on `ProductId` where `[IsPrimary] = 1` |

- Deleting a product deletes its attributes, their values, and its images.
- Prices are `decimal(18,2)`. Do not use floating-point types for prices.
- `Product.IsActive` has a database default of `true` with sentinel `true`, so an explicit `false` is still inserted.
- The database collation ignores case, so the unique indexes treat `Red` and `red` as the same value.

## 7. Validation rules

FluentValidation classes in `Web/Validator/`: `ProductValidator`, which applies `ProductAttributeValidator` (and through it `ProductAttributeValueValidator`) and `ProductImageValidator` to each nested item. Failures return 400 with errors per field.

### Product

- Name is required, max 200.
- Slug is required, max 220, `^[a-z0-9]+(-[a-z0-9]+)*$`; uniqueness is enforced by the unique index (409).
- Description max 4000; short description max 500.
- A category is required, and it must exist (checked by the business layer, 422).
- Regular price must not be negative, at most 2 decimals.
- Sale price, when supplied, must not be negative and must be lower than the regular price.
- The same attribute cannot be added twice.
- At most one image can be marked primary.

### ProductAttribute

- An attribute (`AttributeId`) is required, and it must exist in the store-wide catalog (checked by the business layer, 422).
- Display order must be zero or greater.
- Values must be unique within the attribute (ignoring case).

### ProductAttributeValue

- Value is required, max 100.
- Image path is optional, max 512.
- Display order must be zero or greater.

### ProductImage

- Path is required, max 512.
- Alt text max 300.
- Sort order must be zero or greater.
- Only one product image is primary. If none is marked, the first by sort order becomes primary. The filtered unique index enforces it in the database.

## 8. API

`ProductController : BaseController<Product>`, at `/api/Product` and `/api/v1/Product`. **Administrators only** (`[Authorize(Roles = Roles.Admin)]`), like `CategoryController` and `AttributeController`. All responses use the standard `ApiResponse` envelope.

| Method | Endpoint | Purpose |
|---|---|---|
| `GET` | `/api/Product` | List products (without nested data) |
| `GET` | `/api/Product/get` | Filtering, sorting, paging |
| `GET` | `/api/Product/{id}` | Product with images, attributes (including the store-wide `attribute`) and values, each in display order |
| `POST` | `/api/Product` | Create a product with nested `attributes[].values[]` and `images[]` |
| `PUT` | `/api/Product` | Replace the product and its nested data (`id` in the body) |
| `DELETE` | `/api/Product` | Delete the product and its nested data (`{ "id": … }` in the body) |
| `POST` | `/api/Product/{id}/images` | Upload a gallery image (multipart: `file`, optional `altText`); added at the end of the gallery |
| `POST` | `/api/Product/{id}/attributes/values/{valueId}/image` | Upload the image of one attribute value (multipart: `file`) |
| `DELETE` | `/api/Product/{id}/attributes/values/{valueId}/image` | Clear the image of one attribute value (the stored file is kept) |

Create request example:

```json
{
  "name": "Classic T-Shirt",
  "slug": "classic-t-shirt",
  "categoryId": 4,
  "regularPrice": 19.99,
  "salePrice": 14.50,
  "attributes": [
    { "attributeId": 1, "displayOrder": 1, "values": [
      { "value": "Red", "imagePath": "/storage/products/100/options/red.jpg", "displayOrder": 1 },
      { "value": "Blue", "displayOrder": 2 } ] },
    { "attributeId": 2, "displayOrder": 2, "values": [
      { "value": "Small", "displayOrder": 1 }, { "value": "Large", "displayOrder": 2 } ] }
  ],
  "images": [ { "path": "/storage/products/100/front.jpg", "altText": "Front", "sortOrder": 1 } ]
}
```

PUT semantics: the request is the full new state. A nested item that is not sent is deleted. Items are matched by `id` when sent, otherwise by `attributeId` (attributes), value text (values) or path (images), so unchanged rows keep their ids.

Entities are used directly as request/response contracts, as elsewhere in the codebase (`BaseController<T>`); circular references are avoided with `[JsonIgnore]` on back-references.

## 9. Storefront behavior

Not part of this repository (API only). The API supports it; a storefront should:

- Show the product's primary gallery image first.
- Display other product gallery images in the product image gallery.
- Render each attribute and its values in display order.
- If a selected attribute value has an `ImagePath`, update the product's displayed image to that image.
- If the selected value has no image, keep the current product image.
- Not calculate stock from attribute values.
- Not treat selecting an attribute value as selecting a unique SKU until product variants are implemented.

For products with multiple image-bearing attributes, define a clear storefront rule. For example, a selected color can control the main image while other attribute selections do not change it.

Note: the product endpoints are admin-only for now; public storefront reads need their own access rule.

## 10. Implementation order

1. Review `BaseEntity`, `Category`, the store-wide `Attribute` and the EF Core conventions.
2. Create the four entity classes.
3. Configure entity relationships, indexes, lengths, and delete behavior in the maps.
4. Add and apply the `AddProductCatalog` EF Core migration.
5. Create validation for create/update operations.
6. Implement product create, update, list, and detail business logic.
7. Implement image upload/storage using the existing `IFileUploader` (public disk). Store the resulting URL in the database; do not store raw image bytes in these entity fields.
8. Test product creation with multiple attributes and values.
9. Test an attribute value with an image and another without one.
10. Verify storefront image switching when an image-bearing value is selected (once a storefront exists).

## 11. Out of scope for this phase

Do not implement these as part of this four-entity catalog task:

- `ProductVariant` / sellable attribute combinations
- Variant-level SKU and barcode
- Inventory quantity, stock movement, or warehouse balances
- Cart and order processing
- Storefront (public) product reads and UI

These can be designed as separate features later. In particular, implement `ProductVariant` before assigning unique SKU/barcode to combinations such as `Red + Small`.

## 12. Acceptance criteria

- A product can be created with multiple attributes.
- Each attribute can contain multiple values.
- Each attribute value can optionally have its own image path.
- A product can have multiple gallery images.
- Product detail returns the product, gallery images, attributes, and values in display order.
- The storefront can change the displayed image when an image-bearing attribute value is selected.
- SKU, barcode, and quantity are not incorrectly stored on attribute values.
- Foreign keys and validation prevent orphaned or invalid catalog data.

## 13. How it is implemented

| Layer | Files |
|---|---|
| Core | `Entities/{Product,ProductAttribute,ProductAttributeValue,ProductImage}.cs`, `Configuration/{Product,ProductAttribute,ProductAttributeValue,ProductImage}Map.cs` |
| Infrastructure | `I25DbContext` (`Products`, `ProductAttributes`, `ProductAttributeValues`, `ProductImages` + the four maps), migration `AddProductCatalog` |
| BusinessLayer | `Implementation/Interface/IProductBusinessLayer.cs`, `Implementation/ProductBusinessLayer.cs` |
| Web | `Controllers/ProductController.cs`, `Validator/{Product,ProductAttribute,ProductAttributeValue,ProductImage}Validator.cs`, registration in `Program.cs` |

**`ProductBusinessLayer`** (registered as `IProductBusinessLayer`):

- `GetByIdAsync`: loads the product with images (by `SortOrder`), attributes (by `DisplayOrder`, with the store-wide `Attribute`) and values (by `DisplayOrder`), as a no-tracking split query.
- `AddAsync`: checks that the category and every `AttributeId` exist (422 otherwise), ignores client-sent ids and any nested `attribute` object (a product never creates store-wide attributes), makes sure exactly one image is primary, then saves the whole tree.
- `UpdateSync`: loads the existing tree, runs the same checks, then calls `SyncProduct` (scalar fields), `SyncAttributes` / `SyncValues` and `SyncImages` (add, update in place, delete what was not sent). A change of primary image is saved in two steps inside one transaction, so the filtered unique index is never violated.
- `AddImageAsync`: adds an uploaded gallery image at the end; the product's first image becomes primary.
- `SetAttributeValueImageAsync`: sets or clears one value's image, only for a value that belongs to the given product (404 otherwise).

**Uploads** (`ProductController`): only `.jpg`, `.jpeg`, `.png`, `.gif` and `.webp` are accepted (400 otherwise), then `IFileUploader` applies the general size rules and stores the file under a random name: gallery images in `products/{id}`, option images in `products/{id}/options`, on the public disk (served under `/storage`). The returned public URL is what is saved.

**Errors:** validation failures 400; unknown category or attribute 422; duplicate slug, duplicate attribute/value, or deleting a category/attribute still in use 409 (unique index / foreign key, mapped by `GlobalExceptionHandler`); unknown product or value 404.
