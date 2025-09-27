using FluentValidation;
using Fusion.Service.ViewModels.Companies.Requests;

namespace Fusion.Service.ViewModels.Companies.Validators
{
    public class CompanyFriendshipRequestValidator : AbstractValidator<CompanyFriendshipRequest>
    {
        public CompanyFriendshipRequestValidator()
        {
            RuleFor(x => x.CompanyAId)
                .NotEmpty().WithMessage("CompanyAId is required");

            RuleFor(x => x.CompanyBId)
                .NotEmpty().WithMessage("CompanyBId is required");

            RuleFor(x => x.CompanyAId)
                .NotEqual(x => x.CompanyBId)
                .WithMessage("CompanyAId and CompanyBId cannot be the same");

            RuleFor(x => x.RequesterId)
                .NotEmpty().WithMessage("RequesterId is required");
        }
    }
}
