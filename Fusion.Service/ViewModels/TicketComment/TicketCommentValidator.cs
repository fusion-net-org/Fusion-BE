using FluentValidation;
using Fusion.Repository.Entities;

namespace Fusion.Service.Validators
{
    public class TicketCommentValidator : AbstractValidator<TicketComment>
    {
        public TicketCommentValidator()
        {
            RuleFor(x => x.TicketId)
                .NotEmpty().WithMessage("TicketId is required");

            RuleFor(x => x.AuthorUserId)
                .NotEmpty().WithMessage("AuthorUserId is required");

            RuleFor(x => x.Body)
                .NotEmpty().WithMessage("Comment body is required")
                .MaximumLength(2000).WithMessage("Comment body cannot exceed 2000 characters");

        
        }
    }
}
