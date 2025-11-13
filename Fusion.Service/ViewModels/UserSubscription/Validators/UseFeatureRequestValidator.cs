

//using FluentValidation;
//using Fusion.Repository.Enums;
//using Fusion.Service.ViewModels.UserSubscription.Requests;

//namespace Fusion.Service.ViewModels.UserSubscription.Validators;

//public class UseFeatureRequestValidator : AbstractValidator<UseFeatureRequest>
//{
//    public UseFeatureRequestValidator()
//    {
//        RuleFor(x => x.UserSubscriptionId)
//            .NotEmpty().WithMessage("UserSubscriptionId is required.");

//        RuleFor(x => x.FeatureKey)
//            .Must(fk => fk == FeatureKeys.Company || fk == FeatureKeys.Share)
//            .WithMessage("FeatureKey must be either Company or Share.");
//    }
//}
