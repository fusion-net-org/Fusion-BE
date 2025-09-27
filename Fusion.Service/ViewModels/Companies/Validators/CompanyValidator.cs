using FluentValidation;
using Fusion.Service.ViewModels.Companies.Requests;


namespace Fusion.Service.ViewModels.Companies.Validators
{
    public class CompanyValidator : AbstractValidator<CompanyRequest>
    {
        public CompanyValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Company name must not be empty!")
                .MaximumLength(100).WithMessage("Company name must not exceed 100 characters!");

            RuleFor(x => x.TaxCode)
                .NotEmpty().WithMessage("Company tax-code must not be empty!")
                .Matches(@"^\d{10}$").WithMessage("Company tax-code must be exactly 10 digits and contain only numbers!");

            RuleFor(x => x.Detail)
                .NotEmpty().WithMessage("Company Detail must not be empty!");

            //RuleFor(x => x.ImageCompany)
            //    .NotEmpty().WithMessage("Company Image must not be empty!");
        }
    }
}
