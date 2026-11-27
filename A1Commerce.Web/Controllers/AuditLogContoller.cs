using A1Commerce.Core.Entities;
using Microsoft.AspNetCore.Mvc;
using A1Commerce.BusinessLayer.Implementation;

namespace A1Commerce.Web.Controllers
{
      [Microsoft.AspNetCore.Authorization.Authorize(Roles = A1Commerce.Core.Auth.Roles.Admin)]   // the audit trail is for administrators
      public class AuditLogContoller : BaseController<AuditLog>
    {
        private readonly IAuditLogBusinessLayer _bl;
        public AuditLogContoller(IAuditLogBusinessLayer bl) : base(bl)
        {
            _bl = bl;
        }

        [HttpGet("{entityType}/{entityId:int}")]
        public async Task<IActionResult> GetAuditLogByEntity(string entityType, int entityId)
        {
            var entity = await _bl.GetAuditLogByEntity(entityType, entityId);
            return OkResponse(entity);
        }



    }
}
