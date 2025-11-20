
using FluentValidation;
using Fusion.Service.ViewModels.Users.Requests;

namespace Fusion.Service.ViewModels.Users.Validators;

public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    public RegisterRequestValidator()
    {
        RuleFor(x => x.FirstName)
            .Cascade(CascadeMode.Stop)
              .NotEmpty().WithMessage("First name must not be empty!")
              .MaximumLength(50).WithMessage("First name must not exceed 50 characters!");

        RuleFor(x => x.LastName)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Last name must not be empty!")
            .MaximumLength(50).WithMessage("Last name must not exceed 50 characters");

        RuleFor(x => x.Email)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Email must not be empty")
            .EmailAddress().WithMessage("Invalid email format")
            .Matches(@"@gmail\.com$").WithMessage("Only ...@gmail.com email addresses are allowed");

        RuleFor(x => x.Password)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Password must not br empty!")
            .MinimumLength(6).WithMessage("Password must be at least 6 characters!")
            .MaximumLength(100).WithMessage("Password must not exceed 100 characters!")
            .Matches(@"[A-Z]").WithMessage("Password must contain at least one uppercase letter")
            .Matches(@"[a-z]").WithMessage("Password must contain at least one lowercase letter")
            .Matches(@"\d").WithMessage("Password must contain at least one digit")
            .Matches(@"[\W_]").WithMessage("Password must contain at least one special character!");

        RuleFor(x => x.ConfirmPassword)
            .Equal(x => x.Password).WithMessage("Password confirmation does not match!");

       
}
}
