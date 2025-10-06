

using Fusion.Repository.Bases.Page;
using Fusion.Repository.Data;
using Fusion.Repository.Entities;

namespace Fusion.Repository.IRepositories;

public interface IUserSubscriptionRepository : IGenericRepository<UserSubscription>
{
    Task<int> GetAllQuotaProjectRemainingHasActiveAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<int> GetAllQuotaComapnyRemainingHasActiveAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<PagedResult<UserSubscription>> GetPagedSubscriptionsByUserIdAsync(Guid userId, PagedRequest request, CancellationToken cancellationToken = default);
}
