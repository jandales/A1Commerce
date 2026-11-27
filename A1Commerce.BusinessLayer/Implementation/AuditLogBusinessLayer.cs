using A1Commerce.BusinessLayer.Implementation.Base;
using A1Commerce.BusinessLayer.Implementation.Interface;
using A1Commerce.Infrastructure;
using A1Commerce.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace A1Commerce.BusinessLayer.Implementation
{
    public class AuditLogBusinessLayerLayer : BaseBusinessLayer<AuditLog>, IAuditLogBusinessLayer
    {
        public AuditLogBusinessLayerLayer(IRepository context) : base(context)
        {
        }

        public async Task<List<AuditLog>> GetAuditLogByEntity(string entityType, int entityId)
        {
            return await _dbSet
                .AsNoTracking()
                .Include(x => x.AuditLogChanges)
                .Where(x =>
                    x.EntityType == entityType &&
                    x.EntityId == entityId)
                .OrderByDescending(x => x.ChangedAt)
                .ToListAsync();
        }
    }
}
