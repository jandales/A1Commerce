using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Attribute = A1Commerce.Core.Entities.Attribute;

namespace A1Commerce.Core.Configuration
{
    public class AttributeMap : IEntityTypeConfiguration<Attribute>
    {
        public void Configure(EntityTypeBuilder<Attribute> builder)
        {
            builder.ToTable("Attributes");

            // Lengths must match AttributeValidator.
            builder.Property(x => x.Name).IsRequired().HasMaxLength(100);
            builder.Property(x => x.Slug).IsRequired().HasMaxLength(120);
            builder.Property(x => x.Description).HasMaxLength(500);
            builder.Property(x => x.DisplayOrder).HasDefaultValue(0);

            // Sentinel true (matching the C# initializer) so EF still sends an explicit false;
            // with the usual sentinel (false) an inactive attribute would be inserted as active.
            builder.Property(x => x.IsActive).HasDefaultValue(true).HasSentinel(true);

            builder.HasIndex(x => x.Name).IsUnique();
            builder.HasIndex(x => x.Slug).IsUnique();
        }
    }
}
