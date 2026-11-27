using System.Text.Json.Serialization;

namespace A1Commerce.Core.Entities
{
    public class Product : BaseEntity
    {
        public string Name { get; set; } = string.Empty;

        public string Slug { get; set; } = string.Empty;

        public string? Description { get; set; }

        public string? ShortDescription { get; set; }

        public int CategoryId { get; set; }

        public decimal RegularPrice { get; set; }

        public decimal? SalePrice { get; set; }

        public bool IsActive { get; set; } = true;

        public bool IsFeatured { get; set; }

        [JsonIgnore]
        public Category? Category { get; set; }

        public List<ProductImage> Images { get; set; } = new();

        public List<ProductAttribute> Attributes { get; set; } = new();
    }
}
