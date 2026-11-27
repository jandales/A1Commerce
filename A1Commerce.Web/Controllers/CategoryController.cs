using A1Commerce.BusinessLayer.Implementation;
using A1Commerce.Core.Auth;
using A1Commerce.Core.Entities;
using Microsoft.AspNetCore.Authorization;

namespace A1Commerce.Web.Controllers
{
    /// <summary>Product categories. Administrators only.</summary>
    [Authorize(Roles = Roles.Admin)]
    public class CategoryController : BaseController<Category>
    {
        public CategoryController(ICategoryBusinessLayer service) : base(service)
        {
        }
    }
}
