using A1Commerce.BusinessLayer.Implementation.Base;
using A1Commerce.BusinessLayer.Implementation.Interface;
using A1Commerce.Core.Entities;
using A1Commerce.Core.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace A1Commerce.BusinessLayer.Implementation
{
    /// <summary>
    /// Products with their attributes (links to store-wide attributes), attribute values and gallery images,
    /// handled as one unit: create and update take the whole tree, detail returns it in display order.
    /// Field rules (lengths, duplicates, one primary image) are in ProductValidator; this class checks what
    /// needs the database (category and attributes exist) and keeps the primary image consistent.
    /// </summary>
    public class ProductBusinessLayer : BaseBusinessLayer<Product>, IProductBusinessLayer
    {
        public ProductBusinessLayer(IRepository db) : base(db)
        {
        }

        /// <summary>The product with its images, attributes (with the store-wide attribute) and values, each in display order.</summary>
        public override async Task<Product?> GetByIdAsync(int id)
        {
            return await _dbSet
                .AsNoTracking()
                .AsSplitQuery()
                .Include(p => p.Images.OrderBy(i => i.SortOrder).ThenBy(i => i.id))
                .Include(p => p.Attributes.OrderBy(a => a.DisplayOrder).ThenBy(a => a.id))
                    .ThenInclude(a => a.Attribute)
                .Include(p => p.Attributes)
                    .ThenInclude(a => a.Values.OrderBy(v => v.DisplayOrder).ThenBy(v => v.id))
                .FirstOrDefaultAsync(p => p.id == id);
        }

        /// <summary>Creates the product together with any attributes, values and images sent with it.</summary>
        public override async Task AddAsync(Product entity)
        {
            await EnsureReferencesExistAsync(entity);

            // Everything in the tree is new: ignore ids a client may have sent, and never create
            // store-wide attributes from a product (only AttributeId links to them).
            foreach (var attribute in entity.Attributes)
            {
                attribute.id = 0;
                attribute.Attribute = null;
                foreach (var value in attribute.Values)
                    value.id = 0;
            }

            foreach (var image in entity.Images)
                image.id = 0;

            EnsureOnePrimary(entity.Images);

            await base.AddAsync(entity);
        }

        /// <summary>
        /// Replaces the product and its whole tree with what was sent (PUT semantics: a child that is not sent is deleted).
        /// Children are matched by id, or when no id is sent by AttributeId / value text / image path,
        /// so unchanged rows keep their ids.
        /// </summary>
        public override async Task UpdateSync(Product entity)
        {
            var existing = await _dbSet
                .AsSplitQuery()
                .Include(p => p.Images)
                .Include(p => p.Attributes).ThenInclude(a => a.Values)
                .FirstOrDefaultAsync(p => p.id == entity.id)
                ?? throw NotFoundException.ForEntity(nameof(Product));

            await EnsureReferencesExistAsync(entity);
            EnsureOnePrimary(entity.Images);

            await using var transaction = _context.Database.CurrentTransaction == null
                ? await _context.Database.BeginTransactionAsync()
                : null;

            // The filtered unique index allows one primary image per product at any moment, so a change of
            // primary image is saved in two steps: first no image is primary, then the new one is.
            var currentPrimary = existing.Images.FirstOrDefault(i => i.IsPrimary);
            var newPrimary = entity.Images.FirstOrDefault(i => i.IsPrimary);
            if (currentPrimary != null && (newPrimary == null || Match(existing.Images, newPrimary) != currentPrimary))
            {
                currentPrimary.IsPrimary = false;
                await _context.SaveChangesAsync();
            }

            SyncProduct(entity, existing);
            SyncAttributes(entity.Attributes, existing);
            SyncImages(entity.Images, existing);

            await _context.SaveChangesAsync();

            if (transaction != null)
                await transaction.CommitAsync();

            // Return the saved state (ids of new rows included) to the caller.
            entity.Attributes = existing.Attributes;
            entity.Images = existing.Images;
            entity.intConcurrencyId = existing.intConcurrencyId;
            entity.CreateAt = existing.CreateAt;
            entity.ModifiedAt = existing.ModifiedAt;
        }

        public async Task<ProductImage> AddImageAsync(int productId, string path, string? altText)
        {
            if (!await _dbSet.AnyAsync(p => p.id == productId))
                throw NotFoundException.ForEntity(nameof(Product));

            var images = _context.ProductImages.Where(i => i.ProductId == productId);
            var isFirst = !await images.AnyAsync();

            var image = new ProductImage
            {
                ProductId = productId,
                Path = path,
                AltText = altText,
                IsPrimary = isFirst,
                SortOrder = isFirst ? 0 : await images.MaxAsync(i => i.SortOrder) + 1
            };

            _context.ProductImages.Add(image);
            await _context.SaveChangesAsync();

            return image;
        }

        public async Task<ProductAttributeValue> SetAttributeValueImageAsync(int productId, int valueId, string? imagePath)
        {
            var value = await _context.ProductAttributeValues
                .FirstOrDefaultAsync(v => v.id == valueId && v.ProductAttribute!.ProductId == productId)
                ?? throw NotFoundException.ForEntity(nameof(ProductAttributeValue));

            value.ImagePath = imagePath;
            await _context.SaveChangesAsync();

            return value;
        }

        /// <summary>The category and every store-wide attribute the product points at must exist (422 otherwise).</summary>
        private async Task EnsureReferencesExistAsync(Product product)
        {
            var errors = new Dictionary<string, string[]>();

            if (!await _context.Categories.AnyAsync(c => c.id == product.CategoryId))
                errors[nameof(Product.CategoryId)] = new[] { "The category does not exist." };

            var attributeIds = product.Attributes.Select(a => a.AttributeId).Distinct().ToList();
            var found = await _context.Attributes
                .Where(a => attributeIds.Contains(a.id))
                .Select(a => a.id)
                .ToListAsync();
            var missing = attributeIds.Except(found).ToList();

            if (missing.Count > 0)
                errors[nameof(Product.Attributes)] = new[] { $"Attribute(s) not found: {string.Join(", ", missing)}." };

            if (errors.Count > 0)
                throw new BusinessRuleException("The product refers to records that do not exist.", errors);
        }

        /// <summary>A gallery with images has exactly one primary image: the one marked, otherwise the first by sort order.</summary>
        private static void EnsureOnePrimary(List<ProductImage> images)
        {
            if (images.Count == 0 || images.Count(i => i.IsPrimary) == 1)
                return;

            // More than one marked is rejected by ProductValidator; here none is marked.
            foreach (var image in images)
                image.IsPrimary = false;

            images.OrderBy(i => i.SortOrder).First().IsPrimary = true;
        }

        private static void SyncProduct(Product from, Product to)
        {
            to.Name = from.Name;
            to.Slug = from.Slug;
            to.Description = from.Description;
            to.ShortDescription = from.ShortDescription;
            to.CategoryId = from.CategoryId;
            to.RegularPrice = from.RegularPrice;
            to.SalePrice = from.SalePrice;
            to.IsActive = from.IsActive;
            to.IsFeatured = from.IsFeatured;
        }

        private void SyncAttributes(List<ProductAttribute> incoming, Product product)
        {
            var kept = new List<ProductAttribute>();

            foreach (var source in incoming)
            {
                var target = (source.id > 0 ? product.Attributes.FirstOrDefault(a => a.id == source.id) : null)
                    ?? product.Attributes.FirstOrDefault(a => !kept.Contains(a) && a.AttributeId == source.AttributeId);

                if (target == null)
                {
                    target = new ProductAttribute();
                    product.Attributes.Add(target);
                }

                target.AttributeId = source.AttributeId;
                target.DisplayOrder = source.DisplayOrder;
                SyncValues(source.Values, target);

                kept.Add(target);
            }

            foreach (var removed in product.Attributes.Except(kept).ToList())
            {
                product.Attributes.Remove(removed);
                _context.Remove(removed);
            }
        }

        private void SyncValues(List<ProductAttributeValue> incoming, ProductAttribute attribute)
        {
            var kept = new List<ProductAttributeValue>();

            foreach (var source in incoming)
            {
                var target = (source.id > 0 ? attribute.Values.FirstOrDefault(v => v.id == source.id) : null)
                    ?? attribute.Values.FirstOrDefault(v => !kept.Contains(v) && SameText(v.Value, source.Value));

                if (target == null)
                {
                    target = new ProductAttributeValue();
                    attribute.Values.Add(target);
                }

                target.Value = source.Value;
                target.ImagePath = source.ImagePath;
                target.DisplayOrder = source.DisplayOrder;

                kept.Add(target);
            }

            foreach (var removed in attribute.Values.Except(kept).ToList())
            {
                attribute.Values.Remove(removed);
                _context.Remove(removed);
            }
        }

        private void SyncImages(List<ProductImage> incoming, Product product)
        {
            var kept = new List<ProductImage>();

            foreach (var source in incoming)
            {
                var target = Match(product.Images, source, kept);

                if (target == null)
                {
                    target = new ProductImage();
                    product.Images.Add(target);
                }

                target.Path = source.Path;
                target.AltText = source.AltText;
                target.IsPrimary = source.IsPrimary;
                target.SortOrder = source.SortOrder;

                kept.Add(target);
            }

            foreach (var removed in product.Images.Except(kept).ToList())
            {
                product.Images.Remove(removed);
                _context.Remove(removed);
            }
        }

        private static ProductImage? Match(List<ProductImage> existing, ProductImage source, List<ProductImage>? taken = null)
        {
            return (source.id > 0 ? existing.FirstOrDefault(i => i.id == source.id) : null)
                ?? existing.FirstOrDefault(i => (taken == null || !taken.Contains(i)) && SameText(i.Path, source.Path));
        }

        private static bool SameText(string? a, string? b) =>
            string.Equals(a?.Trim(), b?.Trim(), StringComparison.OrdinalIgnoreCase);
    }
}
