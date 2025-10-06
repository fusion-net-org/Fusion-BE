

using Fusion.Repository.Bases.Page;
using Fusion.Repository.Entities;
using Fusion.Service.ViewModels.UserSubscription.Requests;
using Fusion.Service.ViewModels.UserSubscription.Responses;
namespace Fusion.Service.IServices;

public interface IUserSubscriptionService
{
    Task<UserSubscription> CreateUserSubscriptionAsync(Guid userId, CreateUserSubscriptionRequest request, CancellationToken cancellationToken = default);
    Task<int> GetAllQuotaProjectRemainingHasActiveAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<int> GetAllQuotaComapnyRemainingHasActiveAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<PagedResult<UserSubscriptionResponse>> GetAllUserSubscrptionByUserIdAsync(PagedRequest request, CancellationToken cancellationToken = default);
}
