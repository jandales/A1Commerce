using A1Commerce.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace A1Commerce.Core.Configuration
{
    public class AuditLogMap : IEntityTypeConfiguration<AuditLog>
    {
        public void Configure(EntityTypeBuilder<AuditLog> builder)
        {
            builder.ToTable("AuditLog");

            builder.HasMany(x => x.AuditLogChanges)
                .WithOne(x => x.AuditLog)
                .HasForeignKey(x => x.AuditLogId)
                .OnDelete(DeleteBehavior.Cascade);

        }
    }
}
