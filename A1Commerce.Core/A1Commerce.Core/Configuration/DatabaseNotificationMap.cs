using A1Commerce.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace A1Commerce.Core.Configuration
{
    public class DatabaseNotificationMap : IEntityTypeConfiguration<DatabaseNotification>
    {
        public void Configure(EntityTypeBuilder<DatabaseNotification> builder)
        {
            builder.ToTable("Notifications");

            // The inbox query: "this receiver's notifications, unread first".
            builder.HasIndex(x => new { x.NotifiableType, x.NotifiableId, x.ReadAt });
        }
    }
}
