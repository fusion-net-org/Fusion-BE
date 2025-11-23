
using Fusion.Repository.Bases.Page;
using Fusion.Repository.Bases.Page.CompanySubscriptions;
using Fusion.Repository.Entities;
namespace Fusion.Repository.IRepositories;

public interface ICompanySubscriptionRepository
{
    Task<CompanySubscription?> GetByIdWithNavAsync(Guid id, CancellationToken ct = default);
    Task<CompanySubscription> CreateAsync(CompanySubscription companySubscription, CancellationToken cancellationToken = default);
    Task<PagedResult<CompanySubscription>> GetAllByCompanyIdAsync(Guid companyId, CompanySubscriptionPagedRequest request, CancellationToken ct = default);
    Task<List<CompanySubscription>> GetAllActiveByCompanyIdAsync( Guid companyId, CancellationToken ct = default);
    Task<int> UpdateEnabledByFeatureIdAsync(Guid featureId, bool newStatus, CancellationToken ct = default);
    Task UseFeatureInCompanyAsync(Guid companySubscriptionId, Guid ActorUserId,Guid companyId, string featureName, CancellationToken ct = default);
    Task UseFeatureInUserAsync(Guid userSubscriptionId, Guid userId,string featureName, CancellationToken ct = default);
}
