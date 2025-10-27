

namespace Fusion.Service.ViewModels.Projects.Responses
{
    public class ProjectResponse
    {
        public Guid Id { get; set; }

        public Guid? CompanyId { get; set; }

        public bool IsHired { get; set; }

        public Guid? CompanyHiredId { get; set; }

        public Guid? ProjectRequestId { get; set; }

        public string? Code { get; set; }

        public string? Name { get; set; }

        public string? Description { get; set; }

        public string? Status { get; set; }

        public Guid? WorkflowId { get; set; }

        public DateOnly? StartDate { get; set; }

        public DateOnly? EndDate { get; set; }

        public Guid? CreatedBy { get; set; }

        public DateTime CreateAt { get; set; }

        public DateTime UpdateAt { get; set; }
    }
}
