

using Fusion.Repository.Bases.Page;
using Fusion.Repository.Bases.Page.CompanySubscriptions;
using Fusion.Repository.ViewModels.CompanySubscriptionEntry;
using Fusion.Service.ViewModels.CompanySubscription.Requests;
using Fusion.Service.ViewModels.CompanySubscription.Responses;

namespace Fusion.Service.IServices;

public interface ICompanySubscriptionService
{
    Task<CompanySubscriptionDetailResponse> CreateAsync(CompanySubscriptionCreateRequest request,CancellationToken ct = default);
    Task<CompanySubscriptionDetailResponse?> GetDetailAsync(Guid id,CancellationToken ct = default);
    Task<PagedResult<CompanySubscriptionListResponse>> GetAllByCompanyAsync(Guid companyId,CompanySubscriptionPagedRequest request,CancellationToken ct = default);
    Task<List<CompanySubscriptionActiveResponse>> GetAllActiveByCompanyIdAsync(Guid companyId,CancellationToken ct = default);
    Task<bool> UseFeatureInCompanyAsync(UserFeatureRequest request, CancellationToken ct = default);
    Task<bool> UseFeatureInUserAsync(Guid userSubscriptionId, Guid userId, string featureName, CancellationToken ct = default);
    Task<List<CompanySubscriptionUserUsageItem>> GetUserUsageAsync(
     Guid companySubscriptionId,
     CancellationToken ct = default);
}
