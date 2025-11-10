
using Fusion.Repository.Bases.Page;
using Fusion.Repository.Bases.Page.UserSubscriptions;
using Fusion.Repository.Enums;
using Fusion.Service.ViewModels.UserSubscription.Requests;
using Fusion.Service.ViewModels.UserSubscription.Responses;

namespace Fusion.Service.IServices;

public interface IUserSubscriptionService
{

    Task<UserSubscriptionDetailResponse> CreateAsync(UserSubscriptionCreateRequest request, CancellationToken ct = default);
    Task<UserSubscriptionDetailResponse?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<PagedResult<UserSubscriptionListItem>> GetAllAsync(UserSubscriptionPagedRequest request, CancellationToken ct = default);
    Task<UserSubscriptionDetailResponse> UpdateStatusAsync(Guid id, SubscriptionStatus status, CancellationToken ct = default);
    Task<PagedResult<UserSubscriptionListItem>> GetAllByUserIdAsync(UserSubscriptionPagedRequest request, CancellationToken cancellationToken = default);
    Task ConsumeFeatureAsync(UseFeatureRequest request, CancellationToken cancellationToken = default);
}
