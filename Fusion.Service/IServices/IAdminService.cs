

using Fusion.Repository.ViewModels;
using Fusion.Service.ViewModels.Admin.Responses;

namespace Fusion.Service.IServices;

public interface IAdminService
{
    Task<OverviewDashBoardResponse> OverviewDashBoard(CancellationToken cancellationToken = default);

    Task<OverviewDashBoardResponse> GetTotalsAsync(CancellationToken ct = default);

    //Task<IEnumerable<MonthlyStats>> GetMonthlyStatsAsync(CancellationToken ct = default);

    //Task<IEnumerable<PlanRate>> GetTopPlanRateAsync(CancellationToken token = default);

}
