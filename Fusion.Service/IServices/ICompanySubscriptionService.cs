
using Fusion.Repository.Bases.Page;
using Fusion.Repository.Bases.Page.CompanySubscriptions;
using Fusion.Repository.Entities;
using Fusion.Service.ViewModels.CompanySubscription.Requests;
using Fusion.Service.ViewModels.CompanySubscription.Responses;

namespace Fusion.Service.IServices;

public interface ICompanySubscriptionService
{
    Task<CompanySubscriptionDetailResponse> CreateAsync(CompanySubscriptionCreateRequest dto, CancellationToken cancellationToken = default);
    Task<CompanySubscriptionDetailResponse> UpdateAsync(CompanySubscriptionUpdateRequest dto, CancellationToken cancellationToken = default);
    Task<CompanySubscriptionDetailResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<PagedResult<CompanySubscriptionListResponse>> GetAllAsync(
        CompanySubscriptionPagedRequest request,
        CancellationToken cancellationToken = default);

    Task<PagedResult<CompanySubscriptionListResponse>> GetAllByCompanyAsync(
        Guid companyId,
        CompanySubscriptionPagedRequest request,
        CancellationToken cancellationToken = default);

    Task<List<CompanySubscriptionActiveResponse>> GetAllActiveByCompanyIdAsync(Guid companyId, CancellationToken ct = default);
}
