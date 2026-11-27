using A1Commerce.Core.Entities;
using A1Commerce.Web.Validator.Base;
using FluentValidation;

namespace A1Commerce.Web.Validator
{
    /// <summary>Rules for one attribute value sent inside a product attribute.</summary>
    public class ProductAttributeValueValidator : BaseValidator<ProductAttributeValue>
    {
        public ProductAttributeValueValidator()
        {
            RuleFor(x => x.Value).NotEmpty().MaximumLength(100);
            RuleFor(x => x.ImagePath).MaximumLength(512);
            RuleFor(x => x.DisplayOrder).GreaterThanOrEqualTo(0);
        }
    }
}
