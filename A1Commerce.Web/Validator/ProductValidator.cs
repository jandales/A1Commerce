using A1Commerce.Core.Entities;
using A1Commerce.Web.Validator.Base;
using FluentValidation;

namespace A1Commerce.Web.Validator
{
    /// <summary>
    /// Rules for creating and updating a product with its attributes, values and images.
    /// Slug uniqueness is left to the unique index; ProductBusinessLayer checks that the category and attributes exist.
    /// </summary>
    public class ProductValidator : BaseValidator<Product>
    {
        public ProductValidator()
        {
            RuleFor(x => x.Name).NotEmpty().MaximumLength(200);

            RuleFor(x => x.Slug)
                .NotEmpty()
                .MaximumLength(220)
                .Matches("^[a-z0-9]+(-[a-z0-9]+)*$")
                .WithMessage("Slug may contain only lowercase letters, digits and single hyphens (e.g. \"classic-t-shirt\").");

            RuleFor(x => x.Description).MaximumLength(4000);
            RuleFor(x => x.ShortDescription).MaximumLength(500);

            RuleFor(x => x.CategoryId).GreaterThan(0).WithMessage("A category is required.");

            RuleFor(x => x.RegularPrice)
                .GreaterThanOrEqualTo(0)
                .PrecisionScale(18, 2, ignoreTrailingZeros: true);

            RuleFor(x => x.SalePrice!.Value)
                .GreaterThanOrEqualTo(0)
                .PrecisionScale(18, 2, ignoreTrailingZeros: true)
                .LessThan(x => x.RegularPrice).WithMessage("Sale price must be lower than the regular price.")
                .OverridePropertyName(nameof(Product.SalePrice))
                .When(x => x.SalePrice.HasValue);

            RuleForEach(x => x.Attributes).SetValidator(new ProductAttributeValidator());

            RuleFor(x => x.Attributes)
                .Must(attributes => attributes.GroupBy(a => a.AttributeId).All(g => g.Count() == 1))
                .WithMessage("An attribute can be added to a product only once.");

            RuleForEach(x => x.Images).SetValidator(new ProductImageValidator());

            RuleFor(x => x.Images)
                .Must(images => images.Count(i => i.IsPrimary) <= 1)
                .WithMessage("Only one image can be the primary image.");
        }
    }
}
