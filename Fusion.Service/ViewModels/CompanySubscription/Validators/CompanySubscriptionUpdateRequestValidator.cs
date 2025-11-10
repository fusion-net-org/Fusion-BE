using FluentValidation;
using Fusion.Repository.Enums;
using Fusion.Service.ViewModels.CompanySubscription.Requests;


namespace Fusion.Service.ViewModels.CompanySubscription.Validators;

public class CompanySubscriptionUpdateRequestValidator : AbstractValidator<CompanySubscriptionUpdateRequest>
{
    public CompanySubscriptionUpdateRequestValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        When(x => x.Entitlements != null && x.Entitlements.Any(), () =>
        {
            RuleForEach(x => x.Entitlements).ChildRules(ent =>
            {
                // Chỉ cho phép feature Project được update
                ent.RuleFor(e => e.FeatureKey)
                    .Must(fk => fk == FeatureKeys.Project)
                    .WithMessage("Only FeatureKey 'Project' can be updated.");

                ent.RuleFor(e => e.Quantity)
                    .GreaterThanOrEqualTo(0)
                    .WithMessage("Quantity must be non-negative");
            });
        });
    }
}
