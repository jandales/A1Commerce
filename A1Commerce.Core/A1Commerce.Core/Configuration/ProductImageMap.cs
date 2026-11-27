using A1Commerce.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace A1Commerce.Core.Configuration
{
    public class ProductImageMap : IEntityTypeConfiguration<ProductImage>
    {
        public void Configure(EntityTypeBuilder<ProductImage> builder)
        {
            builder.ToTable("ProductImages");

            // Lengths must match ProductImageValidator.
            builder.Property(x => x.Path).IsRequired().HasMaxLength(512);
            builder.Property(x => x.AltText).HasMaxLength(300);

            builder.HasIndex(x => x.ProductId);

            // At most one primary image per product. Named, so it is a second index and not a change to the one above.
            builder.HasIndex(x => x.ProductId, "IX_ProductImages_ProductId_Primary")
                .IsUnique()
                .HasFilter("[IsPrimary] = 1");
        }
    }
}
