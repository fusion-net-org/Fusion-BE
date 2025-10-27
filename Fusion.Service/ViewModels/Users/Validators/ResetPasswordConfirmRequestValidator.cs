

using FluentValidation;
using Fusion.Service.ViewModels.Users.Requests;

namespace Fusion.Service.ViewModels.Users.Validators;

public class ResetPasswordConfirmRequestValidator : AbstractValidator<ResetPasswordConfirmRequest>
{
    public ResetPasswordConfirmRequestValidator()
    {
        RuleFor(x => x.ResetToken)
           .NotEmpty().WithMessage("Reset token is required.");

        RuleFor(x => x.NewPassword)
            .Cascade(CascadeMode.Stop)
           .NotEmpty().WithMessage("New password is required.")
           .MinimumLength(8).WithMessage("Password must be at least 8 characters long.")
           .Matches("[A-Z]").WithMessage("Password must contain at least one uppercase letter.")
           .Matches("[a-z]").WithMessage("Password must contain at least one lowercase letter.")
           .Matches("[0-9]").WithMessage("Password must contain at least one number.")
           .Matches("[^a-zA-Z0-9]").WithMessage("Password must contain at least one special character.");

        RuleFor(x => x.ConfirmPassword)
           .Equal(x => x.NewPassword)
           .WithMessage("Passwords do not match.");
    }
}
