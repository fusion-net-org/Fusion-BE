

using FluentValidation;
using Fusion.Service.ViewModels.TransactionPayment.Requests;

namespace Fusion.Service.ViewModels.TransactionPayment.Validators;

public class CreateTransactionValidator : AbstractValidator<CreateTransactionRequest>
{
    public CreateTransactionValidator()
    {
        // PackageId
        RuleFor(x => x.PackageId)
            .NotEmpty().WithMessage("PackageId is required.");
    }
}
