

using FluentValidation;
using Fusion.Service.ViewModels.SubscriptionPackage.Requests;

namespace Fusion.Service.ViewModels.SubscriptionPackage.Validator;

public class SubscriptionRequestValidator : AbstractValidator<SubscriptionRequest>
{
    public SubscriptionRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Package name cannot be empty.")
            .MaximumLength(100);

        RuleFor(x => x.Price)
            .GreaterThanOrEqualTo(0).WithMessage("Price ≥ 0.");

        RuleFor(x => x.QuotaCompany)
            .GreaterThanOrEqualTo(0).WithMessage("Quota company ≥ 0.");
        RuleFor(x => x.QuotaProject)
            .GreaterThanOrEqualTo(0).WithMessage("Quota project ≥ 0.");


        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Description cannot be empty.")
            .MaximumLength(250);
    }
}
