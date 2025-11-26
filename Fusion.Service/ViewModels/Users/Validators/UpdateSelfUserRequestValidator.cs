
using FluentValidation;
using Fusion.Service.ViewModels.Users.Requests;

namespace Fusion.Service.ViewModels.Users.Validators
{
    public class UpdateSelfUserRequestValidator : AbstractValidator<UpdateSelfUserRequest>
    {
        public UpdateSelfUserRequestValidator()
        {
            RuleFor(x => x.Phone)
                .Cascade(CascadeMode.Stop)
                 .Matches(@"^[0-9]{10,11}$").When(x => x.Phone != null)
                 .WithMessage("Phone must be 10-11 digits!");

            RuleFor(x => x.Address)
            .Cascade(CascadeMode.Stop)
            .MaximumLength(200).When(x => x.Address != null)
            .WithMessage("Address must be less than 200 characters!");

            RuleFor(x => x.Gender)
              .Cascade(CascadeMode.Stop)
              .Must(g => g == "Male" || g == "Female" || g == "Other")
              .When(x => x.Gender != null)
              .WithMessage("Gender must be Male, Female, or Other !");
        }
    }
}
