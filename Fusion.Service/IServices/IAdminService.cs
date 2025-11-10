

using Fusion.Service.ViewModels.Admin.Responses;

namespace Fusion.Service.IServices;

public interface IAdminService
{
    Task<OverviewDashBoardResponse> OverviewDashBoard(CancellationToken cancellationToken = default);
}
