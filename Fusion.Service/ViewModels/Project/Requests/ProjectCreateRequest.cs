


namespace Fusion.Service.ViewModels.Project.Requests
{
    public class ProjectCreateRequest
    {
        public bool IsHired { get; set; }
        public Guid? CompanyRequestId { get; set; }
        public Guid? ProjectRequestId { get; set; }
        public Guid CompanySubscriptionId { get; set; }
        public string Code { get; set; } = default!;
        public string Name { get; set; } = default!;
        public string? Description { get; set; }
        public string Status { get; set; } = "Planned";

        public DateOnly? StartDate { get; set; }
        public DateOnly? EndDate { get; set; }
        public int SprintLengthWeeks { get; set; } = 1;

        // IMPORTANT: chỉ nhận sẵn WorkflowId (đã có workflow rồi)
        public Guid WorkflowId { get; set; }

        // Các user (thuộc công ty chính hoặc công ty thuê nếu IsHired)
        public List<Guid> MemberIds { get; set; } = new();
    }
   

   
}
