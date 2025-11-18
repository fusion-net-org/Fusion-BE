

using Fusion.Repository.Bases.Page;
using Fusion.Repository.Bases.Page.UserSubscriptions;
using Fusion.Repository.Data;
using Fusion.Repository.Entities;

namespace Fusion.Repository.IRepositories;

public interface IUserSubscriptionRepository : IGenericRepository<UserSubscription>
{
    Task<UserSubscription?> GetByIdWithNavAsync(Guid id, CancellationToken ct = default);
    Task<UserSubscription?> GetActiveByUserAsync(Guid userId, CancellationToken ct = default);
    Task<UserSubscription?> GetByTransactionAsync(Guid txId, CancellationToken ct = default);

    Task<UserSubscription> CreateAsync(UserSubscription entity, CancellationToken ct = default);
    Task<bool> UpdateAsync(UserSubscription entity, CancellationToken ct = default);

    Task BulkAddEntitlementsAsync(IEnumerable<UserSubscriptionEntitlement> ents, CancellationToken ct = default);

    Task<PagedResult<UserSubscription>> GetPagedByUserIdAsync(Guid id, UserSubscriptionPagedRequest request, CancellationToken ct = default);
    Task<List<UserSubscription>> GetExpiringAsync(DateTimeOffset until, int take = 100, CancellationToken ct = default);
    Task UpdateNextDueAsync(Guid subId, DateTimeOffset? nextDueAt, CancellationToken ct = default);
    Task DecreaseCompanyShareLimitAsync(Guid userSubscriptionId, int amount = 1, CancellationToken ct = default);

    Task<int> UpdateEnabledByFeatureIdAsync(Guid featureId, bool newStatus, CancellationToken ct = default);
}
