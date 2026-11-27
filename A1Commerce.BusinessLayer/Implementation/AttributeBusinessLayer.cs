using A1Commerce.BusinessLayer.Implementation.Base;
using A1Commerce.BusinessLayer.Implementation.Interface;
using Attribute = A1Commerce.Core.Entities.Attribute;

namespace A1Commerce.BusinessLayer.Implementation
{
    public class AttributeBusinessLayer : BaseBusinessLayer<Attribute>, IAttributeBusinessLayer
    {
        public AttributeBusinessLayer(IRepository db) : base(db)
        {
        }
    }
}
