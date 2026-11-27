using A1Commerce.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace A1Commerce.Core.Configuration
{
    public class ProductMap : IEntityTypeConfiguration<Product>
    {
        public void Configure(EntityTypeBuilder<Product> builder)
        {
            builder.ToTable("Products");

            // Lengths must match ProductValidator.
            builder.Property(x => x.Name).IsRequired().HasMaxLength(200);
            builder.Property(x => x.Slug).IsRequired().HasMaxLength(220);
            builder.Property(x => x.Description).HasMaxLength(4000);
            builder.Property(x => x.ShortDescription).HasMaxLength(500);
            builder.Property(x => x.RegularPrice).HasPrecision(18, 2);
            builder.Property(x => x.SalePrice).HasPrecision(18, 2);

            // Sentinel true (matching the C# initializer) so EF still sends an explicit false.
            builder.Property(x => x.IsActive).HasDefaultValue(true).HasSentinel(true);

            builder.HasIndex(x => x.Slug).IsUnique();

            // A category that still has products cannot be deleted.
            builder.HasOne(x => x.Category)
                .WithMany()
                .HasForeignKey(x => x.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            // Attributes (and through them their values) and images belong to the product.
            builder.HasMany(x => x.Attributes)
                .WithOne(x => x.Product)
                .HasForeignKey(x => x.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(x => x.Images)
                .WithOne(x => x.Product)
                .HasForeignKey(x => x.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
