using FluentValidation;
using FluentValidation.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fusion.Service.Commons.Helpers
{
    public static class ValidatorExtensions
    {
        public static async Task ValidateAndThrowAsync<T>(
            this IValidator<T> validator,
            T instance,
            Action<ValidationStrategy<T>> options,
            CancellationToken cancellation = default)
        {
            var result = await validator.ValidateAsync(instance, options, cancellation);
            if (!result.IsValid)
                throw new ValidationException(result.Errors);
        }
    }
}
