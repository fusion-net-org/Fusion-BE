

using Fusion.Repository.Bases.Page;
using Fusion.Repository.Bases.Page.SubscriptionPlans;
using Fusion.Repository.Data;
using Fusion.Repository.Entities;

namespace Fusion.Repository.IRepositories;

public interface ISubscriptionPlanRepository : IGenericRepository<SubscriptionPlan>
{
    Task<SubscriptionPlan> CreatePlanAsync(SubscriptionPlan req, CancellationToken cancellationToken = default);
    Task<SubscriptionPlan> UpdatePlanAsync(SubscriptionPlan payload, CancellationToken cancellationToken = default);
    Task<PagedResult<SubscriptionPlan>> GetAllAsync(SubscriptionPlanPagedRequest request, CancellationToken cancellationToken = default);
    Task<SubscriptionPlan?> GetByIdWithNavAsync(Guid id, CancellationToken ct = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken ct = default);
    Task<List<SubscriptionPlan>> GetAllForCusromerAsync(CancellationToken cancellationToken = default);
    //Task<bool> ExistsUsed(Guid planId);
}
