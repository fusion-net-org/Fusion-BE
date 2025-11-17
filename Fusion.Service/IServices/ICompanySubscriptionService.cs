

using Fusion.Service.ViewModels.CompanySubscription.Requests;
using Fusion.Service.ViewModels.CompanySubscription.Responses;

namespace Fusion.Service.IServices;

public interface ICompanySubscriptionService
{
    Task<CompanySubscriptionDetailResponse> CreateAsync(CompanySubscriptionCreateRequest request,CancellationToken ct = default);

    Task<CompanySubscriptionDetailResponse?> GetDetailAsync(Guid id,CancellationToken ct = default);
}
