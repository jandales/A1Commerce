using A1Commerce.BusinessLayer.Implementation;
using A1Commerce.Core.Auth;
using A1Commerce.Core.Entities;
using A1Commerce.Web.FileStorage;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace A1Commerce.Web.Controllers
{
    /// <summary>
    /// Products with their attributes, attribute values and gallery images. Administrators only.
    /// From BaseController: POST / PUT take the whole tree (PUT replaces it), GET /{id} returns it in display order.
    /// </summary>
    [Authorize(Roles = Roles.Admin)]
    public class ProductController : BaseController<Product>
    {
        private static readonly string[] ImageExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };

        private readonly IProductBusinessLayer _products;
        private readonly IFileUploader _uploader;

        public ProductController(IProductBusinessLayer products, IFileUploader uploader) : base(products)
        {
            _products = products;
            _uploader = uploader;
        }

        /// <summary>Uploads a gallery image and adds it at the end of the gallery.</summary>
        [HttpPost("{id}/images")]
        public async Task<IActionResult> UploadImage(int id, IFormFile file, [FromForm] string? altText)
        {
            var path = await StoreImageAsync(file, $"products/{id}");
            var image = await _products.AddImageAsync(id, path, altText);

            return OkResponse(image, Translate("messages.created"));
        }

        /// <summary>Uploads the image of one attribute value (e.g. the picture shown when "Red" is selected).</summary>
        [HttpPost("{id}/attributes/values/{valueId}/image")]
        public async Task<IActionResult> UploadAttributeValueImage(int id, int valueId, IFormFile file)
        {
            var path = await StoreImageAsync(file, $"products/{id}/options");
            var value = await _products.SetAttributeValueImageAsync(id, valueId, path);

            return OkResponse(value, Translate("messages.updated"));
        }

        /// <summary>Removes the image of one attribute value. The stored file is kept.</summary>
        [HttpDelete("{id}/attributes/values/{valueId}/image")]
        public async Task<IActionResult> ClearAttributeValueImage(int id, int valueId)
        {
            var value = await _products.SetAttributeValueImageAsync(id, valueId, null);

            return OkResponse(value, Translate("messages.updated"));
        }

        /// <summary>Stores an image file and returns what to save in the database: its public URL, or its path on a private disk.</summary>
        private async Task<string> StoreImageAsync(IFormFile file, string directory)
        {
            var extension = Path.GetExtension(file?.FileName ?? string.Empty).ToLowerInvariant();

            // Narrower than the general upload rules (which also allow documents): only images here.
            if (!ImageExtensions.Contains(extension))
                throw new ArgumentException($"Only images can be uploaded here ({string.Join(", ", ImageExtensions)}).");

            var stored = await _uploader.StoreAsync(file!, directory, cancellationToken: HttpContext.RequestAborted);

            return stored.Url ?? stored.Path;
        }
    }
}
