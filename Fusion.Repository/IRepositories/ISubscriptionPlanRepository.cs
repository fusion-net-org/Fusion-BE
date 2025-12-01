

using Fusion.Repository.Bases.Page;
using Fusion.Repository.Bases.Page.SubscriptionPlans;
using Fusion.Repository.Data;
using Fusion.Repository.Entities;

namespace Fusion.Repository.IRepositories;

public interface ISubscriptionPlanRepository : IGenericRepository<SubscriptionPlan>
{
    Task<SubscriptionPlan> CreatePlanAsync(SubscriptionPlan req, CancellationToken ct = default);
    Task<SubscriptionPlan> UpdatePlanAsync(SubscriptionPlan payload, CancellationToken ct = default);
    Task<PagedResult<SubscriptionPlan>> GetAllAsync(SubscriptionPlanPagedRequest request, CancellationToken ct = default);
    Task<SubscriptionPlan?> GetByIdWithNavAsync(Guid id, CancellationToken ct = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken ct = default);
    Task<List<SubscriptionPlan>> GetAllForCusromerAsync(CancellationToken ct = default);
    Task<int> UpdateEnabledByFeatureIdAsync(Guid featureId, bool newStatus, CancellationToken ct = default);
}
