

using Fusion.Repository.Bases.Page;
using Fusion.Repository.Bases.Page.UserSubscriptions;
using Fusion.Repository.Data;
using Fusion.Repository.Entities;
using Fusion.Repository.Enums;

namespace Fusion.Repository.IRepositories;

public interface IUserSubscriptionRepository : IGenericRepository<UserSubscription>
{
    Task<UserSubscription> CreateAsync(UserSubscription userSubscription, CancellationToken cancellationToken = default);
    Task<UserSubscription> UpdateAsync(Guid userId, UserSubscription userSubscription, CancellationToken cancellationToken = default);
    Task<bool> Delete(Guid id, CancellationToken ct = default);
    Task<UserSubscription?> GetByIdWithNavAsync(Guid id, CancellationToken ct = default);
    Task<PagedResult<UserSubscription>> GetAllAsync(UserSubscriptionPagedRequest request, CancellationToken cancellationToken = default);
    Task<UserSubscription> UpdateStatusAsync(Guid id,Guid userId, SubscriptionStatus status, CancellationToken cancellationToken = default);
    Task<PagedResult<UserSubscription>> GetAllByUserIdAsync(Guid userId, UserSubscriptionPagedRequest request, CancellationToken cancellationToken = default);
    Task ValidateAndConsumeEntitlementsAsync(Guid userSubscriptionId, IEnumerable<CompanySubscriptionEntitlement> requestedEntitlements, CancellationToken cancellationToken = default);

}
