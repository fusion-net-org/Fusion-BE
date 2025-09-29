using FluentValidation;
using Fusion.Service.ViewModels.Tickets.Requests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fusion.Service.ViewModels.Tickets.Validators
{
	public class TicketValidator : AbstractValidator<TicketRequest>
	{
		public TicketValidator()
		{
			RuleSet("Create", () =>
			{
				RuleFor(x => x.TicketName)
					.NotEmpty().WithMessage("Ticket name must not be empty!")
					.MaximumLength(100).WithMessage("Ticket name must not exceed 100 characters!");

				RuleFor(x => x.Priority)
					.NotEmpty().WithMessage("Priority is required!")
					.Must(p => new[] { "Low", "Medium", "High" }.Contains(p))
					.WithMessage("Priority must be one of: Low, Medium, High.");

				RuleFor(x => x.Urgency)
					.NotEmpty().WithMessage("Urgency is required!")
					.Must(u => new[] { "Low", "Medium", "High" }.Contains(u))
					.WithMessage("Urgency must be one of: Low, Medium, High.");

				RuleFor(x => x.SubmittedBy)
					.NotEmpty().WithMessage("SubmittedBy must not be empty!");
			});

			RuleSet("Update", () =>
			{
				RuleFor(x => x.TicketName)
					.MaximumLength(100).WithMessage("Ticket name must not exceed 100 characters!");

				RuleFor(x => x.Priority)
					.Must(p => string.IsNullOrEmpty(p) || new[] { "Low", "Medium", "High" }.Contains(p))
					.WithMessage("Priority must be one of: Low, Medium, High.");

				RuleFor(x => x.Urgency)
					.Must(u => string.IsNullOrEmpty(u) || new[] { "Low", "Medium", "High" }.Contains(u))
					.WithMessage("Urgency must be one of: Low, Medium, High.");

				RuleFor(x => x.ClosedAt)
					.GreaterThanOrEqualTo(x => x.ResolvedAt)
					.When(x => x.ClosedAt.HasValue && x.ResolvedAt.HasValue)
					.WithMessage("ClosedAt must be greater than or equal to ResolvedAt.");
			});
		}
	}
}
