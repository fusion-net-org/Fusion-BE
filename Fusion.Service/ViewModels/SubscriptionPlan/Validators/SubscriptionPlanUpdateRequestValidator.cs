using FluentValidation;
using Fusion.Service.ViewModels.SubscriptionPlan.Requests;
namespace Fusion.Service.ViewModels.SubscriptionPlan.Validators;

public class SubscriptionPlanUpdateRequestValidator : AbstractValidator<SubscriptionPlanUpdateRequest>
{
    public SubscriptionPlanUpdateRequestValidator()
    {
        Include(new SubscriptionPlanCreateRequestValidator());

        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Id is required.");
    }
}
