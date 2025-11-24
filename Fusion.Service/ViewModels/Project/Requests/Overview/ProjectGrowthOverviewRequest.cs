
namespace Fusion.Service.ViewModels.Project.Requests.Overview;

public class ProjectGrowthOverviewRequest
{
    public Guid? CompanyId { get; set; }

    /// <summary>
    /// Inclusive start time (UTC). Nếu null => auto 12 tháng gần nhất.
    /// </summary>
    public DateTime? From { get; set; }

    /// <summary>
    /// Inclusive end time (UTC). Nếu null => DateTime.UtcNow.
    /// </summary>
    public DateTime? To { get; set; }
}
