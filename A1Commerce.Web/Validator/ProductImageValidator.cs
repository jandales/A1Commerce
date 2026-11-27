using A1Commerce.Core.Entities;
using A1Commerce.Web.Validator.Base;
using FluentValidation;

namespace A1Commerce.Web.Validator
{
    /// <summary>Rules for one gallery image sent inside a product.</summary>
    public class ProductImageValidator : BaseValidator<ProductImage>
    {
        public ProductImageValidator()
        {
            RuleFor(x => x.Path).NotEmpty().MaximumLength(512);
            RuleFor(x => x.AltText).MaximumLength(300);
            RuleFor(x => x.SortOrder).GreaterThanOrEqualTo(0);
        }
    }
}
