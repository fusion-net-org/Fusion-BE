
using FluentValidation;
using Fusion.Service.ViewModels.FeatureCatalog.Requests;
using System.Text.RegularExpressions;

namespace Fusion.Service.ViewModels.FeatureCatalog.Validators;

public class FeatureUpdateRequestValidator : AbstractValidator<FeatureUpdateRequest>
{
    private static readonly Regex CodeRegex =
           new(@"^[a-z0-9][a-z0-9._-]{1,63}$", RegexOptions.Compiled | RegexOptions.IgnoreCase);


    public FeatureUpdateRequestValidator()
    {
        RuleFor(x => x.Id)
               .NotEmpty().WithMessage("Id is required.");

        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Code is required.")
            .MaximumLength(64)
            .Must(c => CodeRegex.IsMatch(c ?? string.Empty))
            .WithMessage("Code must contain only a-z, 0-9, '.', '-', '_' and be 2–64 characters long, starting with a letter/number.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(200);

        RuleFor(x => x.Description)
            .MaximumLength(400)
            .When(x => !string.IsNullOrWhiteSpace(x.Description));

        RuleFor(x => x.Category)
            .MaximumLength(50)
            .When(x => !string.IsNullOrWhiteSpace(x.Category));
    }
}
