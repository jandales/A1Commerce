using A1Commerce.Core.Entities;
using A1Commerce.Web.Validator.Base;
using FluentValidation;

namespace A1Commerce.Web.Validator
{
    /// <summary>Rules for one product attribute sent inside a product (see ProductValidator).</summary>
    public class ProductAttributeValidator : BaseValidator<ProductAttribute>
    {
        public ProductAttributeValidator()
        {
            RuleFor(x => x.AttributeId).GreaterThan(0).WithMessage("An attribute is required.");
            RuleFor(x => x.DisplayOrder).GreaterThanOrEqualTo(0);

            RuleForEach(x => x.Values).SetValidator(new ProductAttributeValueValidator());

            // The database collation ignores case, so "Red" and "red" are duplicates.
            RuleFor(x => x.Values)
                .Must(values => values
                    .Where(v => !string.IsNullOrWhiteSpace(v.Value))
                    .GroupBy(v => v.Value.Trim(), StringComparer.OrdinalIgnoreCase)
                    .All(g => g.Count() == 1))
                .WithMessage("Values must be unique within an attribute.");
        }
    }
}
