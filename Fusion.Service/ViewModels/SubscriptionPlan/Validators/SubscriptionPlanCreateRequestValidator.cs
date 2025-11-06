

using FluentValidation;
using Fusion.Service.ViewModels.SubscriptionPlan.Requests;

namespace Fusion.Service.ViewModels.SubscriptionPlan.Validators
{
    public class SubscriptionPlanCreateRequestValidator : AbstractValidator<SubscriptionPlanCreateRequest>
    {
        public SubscriptionPlanCreateRequestValidator()
        {
            RuleLevelCascadeMode = CascadeMode.Stop;

            // Plan
            RuleFor(x => x.Code)
                .Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("Code not empty")
                .MaximumLength(50).WithMessage("Code max length 50")
                .Matches(@"^[A-Za-z0-9\-_\.]+$").WithMessage("Code allows letters, digits, '-', '_' or '.'");

            RuleFor(x => x.Name)
                .Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("Name not empty")
                .MaximumLength(100).WithMessage("Name max length 100");

            // Price: yêu cầu bắt buộc, chỉ 1
            RuleFor(x => x.Price)
                .Cascade(CascadeMode.Stop)
                .NotNull().WithMessage("Price is required")
                .DependentRules(() =>
                {
                    RuleFor(x => x.Price).ChildRules(price =>
                    {
                        price.RuleFor(p => p.BillingPeriod)
                             .Cascade(CascadeMode.Stop)
                             .IsInEnum().WithMessage("BillingPeriod is invalid");

                        price.RuleFor(p => p.PeriodCount)
                             .Cascade(CascadeMode.Stop)
                             .GreaterThan(0).WithMessage("PeriodCount must be > 0");

                        price.RuleFor(p => p.Price)
                             .Cascade(CascadeMode.Stop)
                             .GreaterThanOrEqualTo(0).WithMessage("Price must be >= 0");

                        price.RuleFor(p => p.Currency)
                             .Cascade(CascadeMode.Stop)
                             .NotEmpty().WithMessage("Currency is required")
                             .Length(3).WithMessage("Currency must be 3 letters")
                             .Must(c => c.All(char.IsLetter)).WithMessage("Currency must contain only letters")
                             .Must(c => c == c.ToUpperInvariant()).WithMessage("Currency must be uppercase");

                        price.RuleFor(p => p.RefundWindowDays)
                             .Cascade(CascadeMode.Stop)
                             .GreaterThanOrEqualTo(0).WithMessage("RefundWindowDays must be >= 0");

                        price.RuleFor(p => p.RefundFeePercent)
                             .Cascade(CascadeMode.Stop)
                             .InclusiveBetween(0, 100).WithMessage("RefundFeePercent must be in [0..100]");
                    });
                });

            // Features: optional, nếu có thì hợp lệ & không trùng
            When(x => x.Features != null && x.Features.Count > 0, () =>
            {
                RuleForEach(x => x.Features!).ChildRules(feature =>
                {
                    feature.RuleFor(f => f.FeatureKey)
                           .Cascade(CascadeMode.Stop)
                           .IsInEnum().WithMessage("FeatureKey is invalid");

                    feature.RuleFor(f => f.LimitValue)
                           .Cascade(CascadeMode.Stop)
                           .GreaterThanOrEqualTo(0).WithMessage("LimitValue must be >= 0")
                           .When(f => f.LimitValue.HasValue);
                });

                RuleFor(x => x.Features!)
                    .Must(list => list.Select(f => f.FeatureKey).Distinct().Count() == list.Count)
                    .WithMessage("FeatureKey must be unique.")
                    .OverridePropertyName("Features"); 
            });
        }
    }
}