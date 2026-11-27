using A1Commerce.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace A1Commerce.Core.Configuration
{
    public class AuditLogChangeMap : IEntityTypeConfiguration<AuditLogChange>
    {
        public void Configure(EntityTypeBuilder<AuditLogChange> builder)
        {
            builder.ToTable("AuditLogChange");

        }
    }
}
