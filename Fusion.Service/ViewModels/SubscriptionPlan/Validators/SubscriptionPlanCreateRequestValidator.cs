

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

        // EntireCompany ⇒ SeatsPerCompanyLimit phải null
        When(x => x.LicenseScope == LicenseScope.EntireCompany, () =>
        {
            RuleFor(x => x.SeatsPerCompanyLimit)
                .Must(v => v == null)
                .WithMessage("SeatsPerCompanyLimit must be null for EntireCompany plans.");
        });

        // Limits >= 0 nếu có
        RuleFor(x => x.CompanyShareLimit)
           .Must(v => v == null || v >= 0)
           .WithMessage("CompanyShareLimit must be null or >= 0.");

        RuleFor(x => x.SeatsPerCompanyLimit)
              .Must(v => v == null || v >= 0)
              .WithMessage("SeatsPerCompanyLimit must be null or >= 0.");

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