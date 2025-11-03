

namespace Fusion.Service.ViewModels.Project.Responses;

public class ProjectListResponse
{
    public Guid Id { get; set; }
    public string? Code { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? Status { get; set; }

    public bool IsHired { get; set; }
    public Guid? CompanyId { get; set; }
    public Guid? CompanyHiredId { get; set; }

    public DateOnly? StartDate { get; set; }
    public DateOnly? EndDate { get; set; }

    public DateTime CreateAt { get; set; }
    public DateTime UpdateAt { get; set; }

    // Optionally for UI convenience
    public string? CompanyName { get; set; }
    public string? CreatedByName { get; set; }
}
