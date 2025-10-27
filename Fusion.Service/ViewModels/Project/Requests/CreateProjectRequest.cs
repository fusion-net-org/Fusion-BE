

namespace Fusion.Service.ViewModels.Project.Requests
{
    public class CreateProjectRequest
    {
        public Guid CompanyId { get; set; }
        public bool isHired { get; set; }
        public Guid? CompanyHiredId { get; set; } = null;
        public Guid? ProjectRequestId { get; set; } = null;
        public string? Code { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public Guid? WorkflowId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
}
