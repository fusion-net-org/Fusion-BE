

using FluentValidation;
using Fusion.Service.ViewModels.Users.Requests;

namespace Fusion.Service.ViewModels.Users.Validations;

public class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.Email)
               .Cascade(CascadeMode.Stop)
               .NotEmpty().WithMessage("Email must not be empty!")
               .EmailAddress().WithMessage("Invalid email format!")
               .Matches(@"@gmail\.com$").WithMessage("Only ...@gmail.com email addresses are allowed!");

        RuleFor(x => x.Password)
              .Cascade(CascadeMode.Stop)
              .NotEmpty().WithMessage("Password mus not be empty!")
              .MinimumLength(6).WithMessage("Password must be at least 6 characters long!")
              .MaximumLength(100).WithMessage("Password must not exceed 100 characters!")
              .Matches(@"[A-Z]").WithMessage("Password must contain at least one uppercase letter!")
              .Matches(@"[a-z]").WithMessage("Password must contain at least one lowercase letter!")
              .Matches(@"\d").WithMessage("Password must contain at least one digit!")
              .Matches(@"[\W_]").WithMessage("Password must contain at least one special character!");
    }
}
