using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fusion.Service.ViewModels.Project.Requests
{
    public class ProjectCreateRequestValidator : AbstractValidator<ProjectCreateRequest>
    {
        public ProjectCreateRequestValidator()
        {
            RuleFor(x => x.Code).NotEmpty().MaximumLength(50);
            RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
            RuleFor(x => x.Status).NotEmpty();

            RuleFor(x => x.StartDate).NotNull().WithMessage("Start date is required.");
            RuleFor(x => x.EndDate).NotNull().WithMessage("End date is required.");
            RuleFor(x => x)
                .Must(x => x.StartDate.HasValue && x.EndDate.HasValue && x.EndDate.Value >= x.StartDate.Value)
                .WithMessage("End date must be same or after start date.");

            RuleFor(x => x.SprintLengthWeeks).GreaterThanOrEqualTo(1);
            RuleFor(x => x.WorkflowId).NotEmpty().WithMessage("WorkflowId is required.");

            When(x => x.IsHired, () =>
            {
                RuleFor(x => x.CompanyHiredId).NotNull().WithMessage("Hired company is required.");
            });
        }
    }
}
