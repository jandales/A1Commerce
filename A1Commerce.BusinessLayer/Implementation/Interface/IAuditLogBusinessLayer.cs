
using A1Commerce.Core.Entities;


namespace A1Commerce.BusinessLayer.Implementation
{
    public interface IAuditLogBusinessLayer : IBusinessLayer<AuditLog>
    {   
        Task<List<AuditLog>> GetAuditLogByEntity(string entityType, int entityId);
    }
}
