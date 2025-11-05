

using Fusion.Repository.Data;
using Fusion.Repository.Entities;

namespace Fusion.Repository.IRepositories;

public interface ISubscriptionRepository : IGenericRepository<SubscriptionPlan>
{
    Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken = default);
}
