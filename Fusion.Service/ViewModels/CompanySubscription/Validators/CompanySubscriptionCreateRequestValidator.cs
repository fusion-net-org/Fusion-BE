
using FluentValidation;
using Fusion.Repository.Enums;
using Fusion.Service.ViewModels.CompanySubscription.Requests;

namespace Fusion.Service.ViewModels.CompanySubscription.Validators;

public class CompanySubscriptionCreateRequestValidator : AbstractValidator<CompanySubscriptionCreateRequest>
{
    public CompanySubscriptionCreateRequestValidator()
    {
        RuleFor(x => x.CompanyId).NotEmpty();
        RuleFor(x => x.UserSubscriptionId).NotEmpty();

        RuleFor(x => x.Entitlements)
            .NotEmpty().WithMessage("At least one entitlement is required.");

        RuleForEach(x => x.Entitlements).ChildRules(ent =>
        {
            // Chỉ cho phép FeatureKey.Project
            ent.RuleFor(e => e.FeatureKey)
                .Equal(FeatureKeys.Project)
                .WithMessage("Only FeatureKey 'Project' can be created.");

            // Quantity > 0
            ent.RuleFor(e => e.Quantity)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Quantity must be greater than 0.");
        });
    }
}
