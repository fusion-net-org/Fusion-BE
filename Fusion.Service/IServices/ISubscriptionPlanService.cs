
using Fusion.Repository.Bases.Page;
using Fusion.Repository.Bases.Page.SubscriptionPlans;
using Fusion.Service.ViewModels.SubscriptionPlan.Requests;
using Fusion.Service.ViewModels.SubscriptionPlan.Responses;
namespace Fusion.Service.IServices
{
    public interface ISubscriptionPlanService
    {
        Task<SubscriptionPlanDetailResponse> CreatePlanAsync(SubscriptionPlanCreateRequest req, CancellationToken cancellationToken = default);
        Task<SubscriptionPlanDetailResponse> UpdatePlanAsync(SubscriptionPlanUpdateRequest req, CancellationToken cancellationToken = default);
        Task<bool> DeletePlanAsync(Guid planId, CancellationToken cancellationToken = default);
        Task<PagedResult<SubscriptionPlanListItemResponse>> GetAllAsync(SubscriptionPlanPagedRequest request, CancellationToken cancellationToken = default);
        Task<SubscriptionPlanDetailResponse?> GetPlanByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<List<SubscriptionPlanCustomerResponse>> GetAllForCusromerAsync(CancellationToken cancellationToken = default);
    }
}