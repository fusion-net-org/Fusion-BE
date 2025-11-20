
using FluentValidation;
using Fusion.Repository.Enums;
using Fusion.Service.ViewModels.SubscriptionPlan.Requests;

namespace Fusion.Service.ViewModels.SubscriptionPlan.Validators;

public class SubscriptionPlanPriceInputValidator : AbstractValidator<SubscriptionPlanPriceInput>
{
    public SubscriptionPlanPriceInputValidator()
    {
        RuleFor(x => x.PeriodCount)
            .GreaterThan(0).WithMessage("PeriodCount must be > 0.");

        RuleFor(x => x.Price)
            .GreaterThanOrEqualTo(0m).WithMessage("Price must be >= 0.");

        When(x => x.PaymentMode == PaymentMode.Installments, () =>
        {
            RuleFor(x => x.InstallmentCount)
                .Cascade(CascadeMode.Stop)
                .NotNull().WithMessage("InstallmentCount is required for installments.")
                .GreaterThan(1).WithMessage("InstallmentCount must be > 1 for installments.");

            RuleFor(x => x.InstallmentInterval)
                .NotNull().WithMessage("InstallmentInterval is required for installments.");
        });

        When(x => x.PaymentMode == PaymentMode.Prepaid, () =>
        {
            RuleFor(x => x.InstallmentCount)
                .Null().WithMessage("Prepaid must not have InstallmentCount.");
            RuleFor(x => x.InstallmentInterval)
                .Null().WithMessage("Prepaid must not have InstallmentInterval.");
        });
    }
}
