using A1Commerce.BusinessLayer.Implementation.Base;
using A1Commerce.BusinessLayer.Implementation.Interface;
using A1Commerce.Core.Entities;

namespace A1Commerce.BusinessLayer.Implementation
{
    public class CategoryBusinessLayer : BaseBusinessLayer<Category>, ICategoryBusinessLayer
    {
        public CategoryBusinessLayer(IRepository db) : base(db)
        {
        }
    }
}
