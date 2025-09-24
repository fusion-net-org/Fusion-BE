

using FluentValidation;
using Fusion.Service.ViewModels.Users.Requests;

namespace Fusion.Service.ViewModels.Users.Validators;

public class GoogleLoginRequestValidator : AbstractValidator<GoogleLoginRequest>
{
    public GoogleLoginRequestValidator()
    {
        RuleFor(x => x.IdToken)
            .NotEmpty().WithMessage("Google IdToken is required");
    }
}