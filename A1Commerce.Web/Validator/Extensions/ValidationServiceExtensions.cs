using FluentValidation;
using System.Reflection;

namespace A1Commerce.Web.Validator.Extensions
{
    public static class ValidationServiceExtensions
    {
        public static IServiceCollection AddFoundationValidators(
            this IServiceCollection services,
            Assembly assembly)
        {
            services.AddValidatorsFromAssembly(assembly);

            return services;
        }
    }
}
