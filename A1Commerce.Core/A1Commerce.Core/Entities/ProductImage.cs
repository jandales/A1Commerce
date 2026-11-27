using System.Text.Json.Serialization;

namespace A1Commerce.Core.Entities
{
    public class ProductImage : BaseEntity
    {
        public int ProductId { get; set; }

        public string Path { get; set; } = string.Empty;

        public string? AltText { get; set; }

        public bool IsPrimary { get; set; }

        public int SortOrder { get; set; }

        [JsonIgnore]
        public Product? Product { get; set; }
    }
}
