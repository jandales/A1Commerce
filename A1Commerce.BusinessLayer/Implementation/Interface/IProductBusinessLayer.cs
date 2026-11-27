using A1Commerce.Core.Entities;

namespace A1Commerce.BusinessLayer.Implementation
{
    public interface IProductBusinessLayer : IBusinessLayer<Product>
    {
        /// <summary>Adds a gallery image at the end of the gallery. The product's first image becomes the primary one.</summary>
        Task<ProductImage> AddImageAsync(int productId, string path, string? altText);

        /// <summary>Sets (or, with null, clears) the image of one attribute value of the product.</summary>
        Task<ProductAttributeValue> SetAttributeValueImageAsync(int productId, int valueId, string? imagePath);
    }
}
