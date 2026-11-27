using System.ComponentModel.DataAnnotations;

namespace A1Commerce.Core.Entities
{
    /// <summary>A product category in the store, e.g. "Men's Shirts" or "Shoes".</summary>
    public class Category : BaseEntity
    {
        [MaxLength(128)]
        public string Name { get; set; } = string.Empty;

        /// <summary>Unique, URL-friendly name: lowercase letters, digits and hyphens ("mens-shirts").</summary>
        [MaxLength(160)]
        public string Slug { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string? Description { get; set; }

        /// <summary>Image path or URL.</summary>
        [MaxLength(512)]
        public string? Image { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
