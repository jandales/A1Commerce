using A1Commerce.Web.Validator.Base;
using FluentValidation;
using Attribute = A1Commerce.Core.Entities.Attribute;

namespace A1Commerce.Web.Validator
{
    /// <summary>
    /// Rules for creating and updating an attribute. Name and slug uniqueness is not checked here:
    /// the unique indexes reject a duplicate and GlobalExceptionHandler turns it into an error response.
    /// </summary>
    public class AttributeValidator : BaseValidator<Attribute>
    {
        public AttributeValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                .MaximumLength(100);

            RuleFor(x => x.Slug)
                .NotEmpty()
                .MaximumLength(120)
                .Matches("^[a-z0-9]+(-[a-z0-9]+)*$")
                .WithMessage("Slug may contain only lowercase letters, digits and single hyphens (e.g. \"shoe-size\").");

            RuleFor(x => x.Description).MaximumLength(500);
            RuleFor(x => x.DisplayOrder).GreaterThanOrEqualTo(0);
        }
    }
}
