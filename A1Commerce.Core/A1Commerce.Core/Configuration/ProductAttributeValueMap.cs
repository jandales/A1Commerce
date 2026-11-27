using A1Commerce.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace A1Commerce.Core.Configuration
{
    public class ProductAttributeValueMap : IEntityTypeConfiguration<ProductAttributeValue>
    {
        public void Configure(EntityTypeBuilder<ProductAttributeValue> builder)
        {
            builder.ToTable("ProductAttributeValues");

            // Lengths must match ProductAttributeValueValidator.
            builder.Property(x => x.Value).IsRequired().HasMaxLength(100);
            builder.Property(x => x.ImagePath).HasMaxLength(512);

            // One "Red" per product attribute. Also serves as the ProductAttributeId index.
            builder.HasIndex(x => new { x.ProductAttributeId, x.Value }).IsUnique();
        }
    }
}
