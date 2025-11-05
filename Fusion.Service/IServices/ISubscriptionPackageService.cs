
using Fusion.Repository.Entities;
using Fusion.Service.ViewModels.SubscriptionPackage.Requests;
using Fusion.Service.ViewModels.SubscriptionPackage.Responses;

namespace Fusion.Service.IServices
{
    public interface ISubscriptionPackageService
    {
        Task<SubscriptionPlan> GetSubscriptionByIdAsync(Guid? id, CancellationToken cancellationToken = default);
        Task<List<SubscriptionResponse?>> GetAllSubscriptionForCustomerAsync(CancellationToken cancellationToken = default);
        Task<List<SubscriptionAdminResponse?>> GetAllSubscriptionForAdminAsync(CancellationToken cancellationToken = default);
        Task<SubscriptionAdminResponse> CreateSubscriptionAsync(SubscriptionRequest request, CancellationToken cancellationToken = default);
        Task<SubscriptionAdminResponse> UpdateSubscriptionAsync(Guid id,SubscriptionRequest request, CancellationToken cancellationToken = default);
        Task<bool> DeleteSubscriptionAsync(Guid id, CancellationToken cancellationToken = default);
    }
}
