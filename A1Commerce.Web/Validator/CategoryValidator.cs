using A1Commerce.Core.Entities;
using A1Commerce.Web.Validator.Base;
using FluentValidation;

namespace A1Commerce.Web.Validator
{
    /// <summary>
    /// Rules for creating and updating a category. Slug uniqueness is not checked here:
    /// the unique index rejects a duplicate and GlobalExceptionHandler turns it into an error response.
    /// </summary>
    public class CategoryValidator : BaseValidator<Category>
    {
        public CategoryValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                .MaximumLength(128);

            RuleFor(x => x.Slug)
                .NotEmpty()
                .MaximumLength(160)
                .Matches("^[a-z0-9]+(-[a-z0-9]+)*$")
                .WithMessage("Slug may contain only lowercase letters, digits and single hyphens (e.g. \"mens-shirts\").");

            RuleFor(x => x.Description).MaximumLength(1000);
            RuleFor(x => x.Image).MaximumLength(512);
        }
    }
}
