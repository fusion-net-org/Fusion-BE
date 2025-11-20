using FluentValidation;
using Fusion.Service.ViewModels.SubscriptionPlan.Requests;

namespace Fusion.Service.ViewModels.SubscriptionPlan.Validators;

public class SubscriptionPlanPriceDiscountInputValidator
      : AbstractValidator<SubscriptionPlanPriceDiscountInput>
{
    public SubscriptionPlanPriceDiscountInputValidator()
    {
        RuleFor(x => x.InstallmentIndex)
            .GreaterThan(0).WithMessage("InstallmentIndex must be > 0.");

        RuleFor(x => x.DiscountValue)
            .InclusiveBetween(0, 100)
            .WithMessage("DiscountValue must be between 0 and 100 (percent).");

        RuleFor(x => x.Note)
            .MaximumLength(250)
            .When(x => !string.IsNullOrWhiteSpace(x.Note));
    }
}