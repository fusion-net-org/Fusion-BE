

using FluentValidation;
using Fusion.Repository.Enums;
using Fusion.Service.ViewModels.SubscriptionPlan.Requests;

namespace Fusion.Service.ViewModels.SubscriptionPlan.Validators;

public class SubscriptionPlanCreateRequestValidator : AbstractValidator<SubscriptionPlanCreateRequest>
{
    public SubscriptionPlanCreateRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().MaximumLength(200);

        RuleFor(x => x.Description)
            .MaximumLength(400)
            .When(x => !string.IsNullOrWhiteSpace(x.Description));

        // CompanyWide ⇒ SeatsPerCompanyLimit phải null
        When(x => x.LicenseScope == LicenseScope.CompanyWide, () =>
        {
            RuleFor(x => x.SeatsPerCompanyLimit)
                .Must(v => v == null)
                .WithMessage("SeatsPerCompanyLimit must be null for CompanyWide plans.");
        });

        // Limits > 0 nếu có
        RuleFor(x => x.CompanyShareLimit)
            .Cascade(CascadeMode.Stop)
            .GreaterThan(0).When(x => x.CompanyShareLimit.HasValue)
            .WithMessage("CompanyShareLimit must be > 0 or null.");

        RuleFor(x => x.SeatsPerCompanyLimit)
            .GreaterThan(0).When(x => x.SeatsPerCompanyLimit.HasValue)
            .WithMessage("SeatsPerCompanyLimit must be > 0 or null.");

        // Price bắt buộc
        RuleFor(x => x.Price)
            .NotNull()
            .SetValidator(new SubscriptionPlanPriceInputValidator());

        // FeatureIds không trùng
        RuleFor(x => x.FeatureIds)
            .Must(list => list == null || list.Distinct().Count() == list.Count)
            .WithMessage("FeatureIds contains duplicate ids.");
    }
}