

using Fusion.Service.ViewModels.Admin.Responses;
using Fusion.Service.ViewModels.TransactionPayment.Responses.Overview;

namespace Fusion.Service.IServices;

public interface IAdminService
{
    Task<OverviewDashBoardResponse> GetTotalsAsync(CancellationToken ct = default);

    Task<IReadOnlyList<PlanPurchaseRatioItemResponse>> GetPlanPurchaseRatioAsync(
          CancellationToken ct = default);
}
