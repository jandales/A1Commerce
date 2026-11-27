using A1Commerce.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace A1Commerce.Core.Configuration
{
    public class CategoryMap : IEntityTypeConfiguration<Category>
    {
        public void Configure(EntityTypeBuilder<Category> builder)
        {
            builder.ToTable("Categories");

            builder.Property(x => x.Name).IsRequired();
            builder.Property(x => x.Slug).IsRequired();
            // Sentinel true (matching the C# initializer) so EF still sends an explicit false;
            // with the usual sentinel (false) an inactive category would be inserted as active.
            builder.Property(x => x.IsActive).HasDefaultValue(true).HasSentinel(true);

            // One category per slug: a duplicate is rejected by the database.
            builder.HasIndex(x => x.Slug).IsUnique();
        }
    }
}
