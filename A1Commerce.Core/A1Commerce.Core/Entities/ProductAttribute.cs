using System.Text.Json.Serialization;

namespace A1Commerce.Core.Entities
{
    public class ProductAttribute : BaseEntity
    {
        public int ProductId { get; set; }

        public int AttributeId { get; set; }

        public int DisplayOrder { get; set; }

        [JsonIgnore]
        public Product? Product { get; set; }

        public Attribute? Attribute { get; set; }

        public List<ProductAttributeValue> Values { get; set; } = new();
    }
}
