using A1Commerce.BusinessLayer.Implementation;
using A1Commerce.Core.Auth;
using Microsoft.AspNetCore.Authorization;
using Attribute = A1Commerce.Core.Entities.Attribute;

namespace A1Commerce.Web.Controllers
{
    /// <summary>Store-wide attributes ("Size", "Color"). Administrators only.</summary>
    [Authorize(Roles = Roles.Admin)]
    public class AttributeController : BaseController<Attribute>
    {
        public AttributeController(IAttributeBusinessLayer service) : base(service)
        {
        }
    }
}
