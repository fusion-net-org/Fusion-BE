
using FluentValidation;
using Fusion.Service.ViewModels.TransactionPayment.Requests;

namespace Fusion.Service.ViewModels.TransactionPayment.Validators;

public class UpdateTransactionValidator : AbstractValidator<UpdateTransactionRequest>
{
    public UpdateTransactionValidator()
    {
        RuleFor(x => x.Status)
           .NotEmpty().WithMessage("Status is required.")
           .Must(s => s == "Pending" || s == "Success" || s == "Failed")
           .WithMessage("Status must be one of: Pending, Success, Failed.");
    }
}
