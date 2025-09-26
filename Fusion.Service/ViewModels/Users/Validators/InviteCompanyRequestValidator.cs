using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;
using Fusion.Service.ViewModels.Users.Requests;

namespace Fusion.Service.ViewModels.Users.Validators
{
    public class InviteCompanyRequestValidator : AbstractValidator<InviteCompanyRequest>
    {
        public InviteCompanyRequestValidator()
        {
            RuleFor(x => x.CompanyBID)
                .NotEmpty().WithMessage("CompanyBID is required.")
                .NotEqual(Guid.Empty).WithMessage("CompanyBID must be a valid GUID.");
        }
    }
}
