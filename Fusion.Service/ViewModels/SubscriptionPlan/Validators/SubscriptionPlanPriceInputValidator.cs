
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

            // Validate từng discount
            RuleForEach(x => x.Discounts)
                .SetValidator(new SubscriptionPlanPriceDiscountInputValidator());

            // Không trùng InstallmentIndex
            RuleFor(x => x.Discounts)
                .Must(list => list == null || list.Select(d => d.InstallmentIndex).Distinct().Count() == list.Count)
                .WithMessage("Discounts contains duplicate InstallmentIndex.");

            // Nếu có InstallmentCount thì InstallmentIndex không được > InstallmentCount
            RuleFor(x => x)
                .Must(x => x.Discounts == null || !x.InstallmentCount.HasValue ||
                           x.Discounts.All(d => d.InstallmentIndex <= x.InstallmentCount.Value))
                .WithMessage("Discount InstallmentIndex cannot be greater than InstallmentCount.");
        });

        When(x => x.PaymentMode == PaymentMode.Prepaid, () =>
        {
            RuleFor(x => x.InstallmentCount)
                .Null().WithMessage("Prepaid must not have InstallmentCount.");
            RuleFor(x => x.InstallmentInterval)
                .Null().WithMessage("Prepaid must not have InstallmentInterval.");
            // Tuỳ nghiệp vụ: có thể bắt Discounts phải null luôn nếu Prepaid
            RuleFor(x => x.Discounts)
                .Must(d => d == null || d.Count == 0)
                .WithMessage("Prepaid must not have Discounts.");
        });
    }
}
