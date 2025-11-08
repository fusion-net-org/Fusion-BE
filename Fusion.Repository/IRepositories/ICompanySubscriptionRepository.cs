

using Fusion.Repository.Bases.Page;
using Fusion.Repository.Bases.Page.CompanySubscriptions;
using Fusion.Repository.Entities;
using Fusion.Repository.Enums;

namespace Fusion.Repository.IRepositories;

public interface ICompanySubscriptionRepository
{
    Task<CompanySubscription?> GetByIdWithNavAsync(Guid id, CancellationToken ct = default);
    Task<CompanySubscription> CreateAsync(CompanySubscription companySubscription, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken ct = default);
    Task<CompanySubscription> UpdateAsync(Guid id, CompanySubscription update, CancellationToken ct = default);
    Task<CompanySubscription> UpdateStatusAsync(Guid id, SubscriptionStatus status, CancellationToken ct = default);
    Task<PagedResult<CompanySubscription>> GetAllAsync(CompanySubscriptionPagedRequest request, CancellationToken ct = default);
    Task<PagedResult<CompanySubscription>> GetAllByCompanyIdAsync(Guid companyId, CompanySubscriptionPagedRequest request, CancellationToken ct = default);
}
