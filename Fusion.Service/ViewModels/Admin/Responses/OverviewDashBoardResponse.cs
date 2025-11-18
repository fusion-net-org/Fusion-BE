
namespace Fusion.Service.ViewModels.Admin.Responses;

public class OverviewDashBoardResponse
{
    public int UserCount { get; set; }
    public int CompanyCount { get; set; }
    public int ProjectCount { get; set; }
    public decimal? RevenueSum { get; set; }
}
