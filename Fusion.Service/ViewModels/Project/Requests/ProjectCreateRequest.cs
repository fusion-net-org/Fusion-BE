


namespace Fusion.Service.ViewModels.Project.Requests
{
    public class MaintenanceComponentCreateRequest
    {
        public string Name { get; set; } = null!;
        public string? Note { get; set; }
    }
    public class ProjectCreateRequest
    {
        public bool IsHired { get; set; }
        public Guid? CompanyRequestId { get; set; }
        public Guid? ProjectRequestId { get; set; }
        public string Code { get; set; } = default!;
        public string Name { get; set; } = default!;
        public string? Description { get; set; }
        public string Status { get; set; } = "Planned";
        public DateOnly? StartDate { get; set; }
        public DateOnly? EndDate { get; set; }
        public int SprintLengthWeeks { get; set; } = 1;
        public Guid WorkflowId { get; set; }
        public List<Guid> MemberIds { get; set; } = new();
        public bool IsMaintenance { get; set; } = false;
        public Guid? MaintenanceForProjectId { get; set; }
        public List<MaintenanceComponentCreateRequest>? MaintenanceComponents { get; set; }
    } 
}
