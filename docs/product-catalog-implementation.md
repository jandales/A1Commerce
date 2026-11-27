# Product Catalog Implementation Specification

## 1. Scope

Implement the product catalog using these four entities:

1. `Product`
2. `ProductAttribute`
3. `ProductAttributeValue`
4. `ProductImage`

This specification focuses on product information, product attributes and values, and product images. Product variants and inventory management are intentionally not implemented in this phase.

## 2. Core concepts

- A **Product** is the item displayed in the store, such as `Classic T-Shirt`.
- A **ProductAttribute** describes a characteristic of a product, such as `Color` or `Size`.
- A **ProductAttributeValue** is one option for an attribute, such as `Red`, `Blue`, `Small`, or `Large`.
- A **ProductImage** stores images that belong to the product gallery.
- Each attribute value can have its own optional image. For example, `Red` can show a red shirt image and `Blue` can show a blue shirt image.

Important: an attribute value is not automatically a sellable SKU. A combination such as `Red + Small` is a product variant. Variant-level SKU, barcode, price, and stock should be designed separately when variant support is implemented.

## 3. Entity definitions

Use the project's existing `BaseEntity` and naming conventions. The examples below show the intended fields; adjust types and property casing to match the existing codebase.

### 3.1 Product

Suggested fields:

| Field | Type | Notes |
|---|---|---|
| `Name` | string | Required product name |
| `Slug` | string | Required URL-friendly unique identifier |
| `Description` | string? | Full product description |
| `ShortDescription` | string? | Optional short summary |
| `CategoryId` | int | Required category relationship, if categories are in scope |
| `RegularPrice` | decimal | Base product price, if pricing is product-level |
| `SalePrice` | decimal? | Optional sale price |
| `IsActive` | bool | Controls storefront visibility |
| `IsFeatured` | bool | Optional featured flag |

Navigation collections:

- `Images`
- `Attributes`

Do not add variant or inventory navigation in this phase.

### 3.2 ProductAttribute

Suggested fields:

| Field | Type | Notes |
|---|---|---|
| `ProductId` | int | Required parent product |
| `Name` | string | Attribute key, e.g. `Color` |
| `DisplayName` | string? | Optional storefront label |
| `IsVariation` | bool | Indicates whether the attribute is used to define variants later |
| `DisplayOrder` | int | Controls display order |

Navigation properties:

- `Product`
- `Values`

For this implementation, attributes are product-specific. For example, the `Color` attribute on one product is a separate record from the `Color` attribute on another product. A reusable global attribute catalog can be introduced later if needed.

### 3.3 ProductAttributeValue

Suggested fields:

| Field | Type | Notes |
|---|---|---|
| `ProductAttributeId` | int | Required parent attribute |
| `Value` | string | Required option, e.g. `Red` |
| `ImagePath` | string? | Optional image associated with this specific value |
| `DisplayOrder` | int | Controls option display order |

Navigation property:

- `ProductAttribute`

Example C# model:

```csharp
public class ProductAttributeValue : BaseEntity
{
    public int ProductAttributeId { get; set; }

    public string Value { get; set; } = string.Empty;

    public string? ImagePath { get; set; }

    public int DisplayOrder { get; set; }

    public ProductAttribute ProductAttribute { get; set; } = null!;
}
```

**SKU and barcode decision:** Do not put SKU or barcode on `ProductAttributeValue` by default. A value such as `Red` can be used in multiple combinations (`Red + Small`, `Red + Large`), and those combinations may need different SKUs and barcodes. Add SKU and barcode to a future `ProductVariant` entity if each combination is independently purchasable. If the business later confirms that an individual attribute value itself is a sellable item, revisit this rule before adding those fields.

**Quantity decision:** Do not store quantity on `ProductAttributeValue`. Quantity is inventory data and should be stored in an inventory/stock entity, potentially per location or warehouse. Do not use a product option's image or value record as the stock ledger.

### 3.4 ProductImage

Suggested fields:

| Field | Type | Notes |
|---|---|---|
| `ProductId` | int | Required parent product |
| `Path` | string | Required stored image path or URL |
| `AltText` | string? | Accessible image description |
| `IsPrimary` | bool | Marks the main gallery image |
| `SortOrder` | int | Gallery display order |

Navigation property:

- `Product`

Use `ProductImage` for the product's general gallery. Use `ProductAttributeValue.ImagePath` for an image that represents a particular option. These are separate purposes; do not duplicate an option image in the gallery unless it should also appear there.

## 4. Relationships

```text
Product
  |
  | 1-to-many
  v
ProductAttribute
  |
  | 1-to-many
  v
ProductAttributeValue

Product
  |
  | 1-to-many
  v
ProductImage
```

Expected relationships:

- One `Product` has many `ProductAttribute` records.
- One `ProductAttribute` has many `ProductAttributeValue` records.
- One `Product` has many `ProductImage` records.
- One `ProductAttributeValue` has zero or one option image path.

## 5. Example data

Product: `Classic T-Shirt`

### ProductAttribute

| Id | ProductId | Name | IsVariation | DisplayOrder |
|---:|---:|---|---|---:|
| 1 | 100 | Color | true | 1 |
| 2 | 100 | Size | true | 2 |

### ProductAttributeValue

| Id | ProductAttributeId | Value | ImagePath | DisplayOrder |
|---:|---:|---|---|---:|
| 1 | 1 | Red | `/uploads/products/100/red.jpg` | 1 |
| 2 | 1 | Blue | `/uploads/products/100/blue.jpg` | 2 |
| 3 | 1 | Black | `/uploads/products/100/black.jpg` | 3 |
| 4 | 2 | Small | null | 1 |
| 5 | 2 | Medium | null | 2 |
| 6 | 2 | Large | null | 3 |

The storefront can use the selected color value's `ImagePath` to update the displayed product image. Size values do not need images unless the business wants them.

## 6. EF Core configuration and database constraints

Configure the following:

- `Product` to `ProductAttribute`: one-to-many.
- `ProductAttribute` to `ProductAttributeValue`: one-to-many.
- `Product` to `ProductImage`: one-to-many.
- Required foreign keys for each child entity.
- A maximum length for names, values, slugs, and image paths according to project conventions.
- A unique index for `Product.Slug`.
- A unique constraint for `(ProductAttributeId, Value)` if duplicate option values should not be allowed within one attribute.
- Index foreign-key columns such as `ProductId` and `ProductAttributeId`.
- Configure delete behavior deliberately. Cascade deletion may be suitable for product-owned attributes, values, and images, provided it matches the project's existing data-retention rules.

Use decimal types for monetary fields. Do not use floating-point types for prices.

## 7. Validation rules

### Product

- Name is required.
- Slug is required and unique.
- Price must not be negative.
- Sale price, when supplied, must not be negative.
- If sale price is required to be lower than regular price, enforce that business rule in validation.

### ProductAttribute

- Product must exist.
- Name is required.
- Display order must be zero or greater, if negative ordering is not supported.

### ProductAttributeValue

- Parent attribute must exist.
- Value is required.
- Image path is optional.
- Display order must be zero or greater, if negative ordering is not supported.
- Prevent duplicate values under the same attribute if that is the intended business rule.

### ProductImage

- Parent product must exist.
- Path is required.
- Only one product image should be primary. Enforce this in application logic and, where supported by the database design, with an appropriate filtered unique index.

## 8. API implementation checklist

Use the project's existing controller, business-layer, DTO, response, and error-handling conventions.

Suggested endpoints:

| Method | Endpoint | Purpose |
|---|---|---|
| `GET` | `/api/products` | List products |
| `GET` | `/api/products/{id}` | Get product with images, attributes, and values |
| `POST` | `/api/products` | Create product and nested catalog data |
| `PUT` | `/api/products/{id}` | Update product and nested catalog data |
| `DELETE` | `/api/products/{id}` | Delete or deactivate product according to project rules |

Optional child endpoints, if the project prefers separate resource management:

- `/api/products/{productId}/attributes`
- `/api/product-attributes/{attributeId}/values`
- `/api/products/{productId}/images`

Avoid exposing EF Core entities directly as request/response contracts if the project already uses DTOs. Prevent circular JSON references by mapping entities to DTOs.

## 9. Storefront behavior

- Show the product's primary gallery image first.
- Display other product gallery images in the product image gallery.
- Render each attribute and its values in display order.
- If a selected attribute value has an `ImagePath`, update the product's displayed image to that image.
- If the selected value has no image, keep the current product image.
- Do not calculate stock from attribute values.
- Do not treat selecting an attribute value as selecting a unique SKU until product variants are implemented.

For products with multiple image-bearing attributes, define a clear storefront rule. For example, a selected color can control the main image while other attribute selections do not change it.

## 10. Implementation order

1. Review the existing `BaseEntity`, `Product`, category relationship, and EF Core conventions.
2. Create or update the four entity classes.
3. Configure entity relationships, indexes, lengths, and delete behavior.
4. Add and apply an EF Core migration.
5. Create DTOs and validation for create/update operations.
6. Implement product create, update, list, and detail business logic.
7. Implement image upload/storage using the project's existing file-storage approach. Store the resulting path in the database; do not store raw image bytes in these entity fields.
8. Test product creation with multiple attributes and values.
9. Test an attribute value with an image and another without one.
10. Verify storefront image switching when an image-bearing value is selected.

## 11. Out of scope for this phase

Do not implement these as part of this four-entity catalog task:

- `ProductVariant` / sellable attribute combinations
- Variant-level SKU and barcode
- Inventory quantity, stock movement, or warehouse balances
- Cart and order processing
- Global reusable attribute definitions

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

The four entities are the existing product model, reshaped. Field names follow the codebase where it already had them.

| Spec | Code | Notes |
|---|---|---|
| `Product.Description` | `Product.LongDescription` | `ShortDescription` as in the spec |
| `Product.IsActive` | `Product.Status` (`Draft` / `Published`) | The shop shows `Published` products only |
| `Product.IsFeatured` | `Product.Featured` | |
| `Product.Images` | `Product.Images` (`ProductImage`) | Was `Photos` / `Photo` |
| `ProductAttribute` | `ProductAttribute` | Was a link to the global `AttributeDefinition`; now product-specific |
| `ProductAttributeValue` | `ProductAttributeValue` | Replaces `ProductVariant` (which only held an option name) |

- **Entities and maps:** `ecormce.Core/Entities/Catalog/{Product,ProductAttribute,ProductAttributeValue,ProductImage}.cs` and `Configuration/*Map.cs`. Unique indexes: `(ProductId, Name)` on attributes, `(ProductAttributeId, Value)` on values, and a filtered unique index allowing one `IsPrimary` picture per product. Attributes, values and images cascade with their product.
- **Migration** `ProductCatalogAttributesValuesImages` keeps the existing data. Each attribute row takes the name of the global attribute it pointed at, the old option rows become values, and `Photos` is renamed to `ProductImages` with the first picture of each product marked primary. `Down` restores the old shape.
- **Business logic** (`ProductBusinessLayer`): `ReplaceAttributesAsync` validates (names and values required, at most 191 characters, no duplicates ignoring case, display orders zero or more) and updates attributes and values in place, matched by name, so their ids survive. `AddAsync` accepts attributes nested in a new product under the same rules. `SetAttributeValueImageAsync` sets or clears an option's picture. `GetDetailAsync` and the shop's product page return attributes and values in display order.
- **Gallery** (`ProductImageBusinessLayer`): the first picture by `SortOrder` is the primary one. Its `IsPrimary` flag and `Product.ImagePath` are kept in step on upload, delete, reorder and "set main".
- **Endpoints:**
  - `POST /api/Product`: create, optionally with `attributes[].values[]`.
  - `PUT /api/Product/{id}/attributes`: `[{ name, displayName, isVariation, displayOrder, values: [{ value, imagePath, displayOrder }] }]`.
  - `POST` / `DELETE /api/Product/{id}/attributes/values/{valueId}/image`: upload or clear an option's picture.
  - `/api/ProductPhoto/{productId}`: upload, delete, reorder and set the main gallery picture.
  - `/api/ProductImage`: generic staff CRUD on picture rows.
- **CSV import:** the `Sizes` column writes the product's own `Size` attribute, so no global attribute has to be created first.
- **Out of scope here, but already in the codebase:** sellable combinations with their own SKU, barcode, price and stock are `ProductSku`, and stock is `Stock` / `StockMovement`. The cart and orders depend on them, so they were left unchanged. `AttributeDefinition` (the global attribute list, `/api/Attribute`) still exists but products no longer reference it.
