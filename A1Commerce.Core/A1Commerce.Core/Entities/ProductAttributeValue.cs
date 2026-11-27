using System.Text.Json.Serialization;

namespace A1Commerce.Core.Entities
{
    public class ProductAttributeValue : BaseEntity
    {
        public int ProductAttributeId { get; set; }

        public string Value { get; set; } = string.Empty;

        public string? ImagePath { get; set; }

        public int DisplayOrder { get; set; }

        [JsonIgnore]
        public ProductAttribute? ProductAttribute { get; set; }
    }
}
