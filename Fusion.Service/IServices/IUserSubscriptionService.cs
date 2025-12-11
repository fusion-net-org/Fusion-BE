
using Fusion.Repository.Bases.Page;
using Fusion.Repository.Bases.Page.UserSubscriptions;
using Fusion.Service.ViewModels.UserSubscription.Requests;
using Fusion.Service.ViewModels.UserSubscription.Responses;

namespace Fusion.Service.IServices;

public interface IUserSubscriptionService
{

    Task<UserSubscriptionDetailResponse> CreateAsync(UserSubscriptionCreateRequest req, CancellationToken ct = default);
    Task<UserSubscriptionDetailResponse?> GetDetailAsync(Guid id, CancellationToken ct = default);
    Task<PagedResult<UserSubscriptionResponse>> GetPagedByUserIdAsync(UserSubscriptionPagedRequest request, CancellationToken ct = default);

    //Dùng khi: Cần biết user hiện đang có subscription hiệu lực hay không.
    Task<UserSubscriptionDetailResponse?> GetActiveByUserAsync(Guid userId, CancellationToken ct = default);

    //Dùng khi: Hủy gói trước hạn (user hoặc admin).
    Task<bool> CancelAsync(Guid id, CancellationToken ct = default);
    //Dùng khi: Tạm dừng quyền sử dụng (ví dụ vi phạm, bảo trì).
    Task<bool> PauseAsync(Guid id, CancellationToken ct = default);
    //Dùng khi: Mở lại sau khi pause.
    Task<bool> ResumeAsync(Guid id, CancellationToken ct = default);
    Task UpdateNextDueAsync(Guid subId, DateTimeOffset? nextDueAt, CancellationToken ct = default);
    Task DecreaseCompanyShareLimitAsync(Guid userSubscriptionId, int amount = 1, CancellationToken ct = default);
    Task<List<UserSubscriptionActiveResponse>> GetAllActiveByUserIdAsync(CancellationToken ct = default);
    Task<int> EnsureAutoMonthlyForUserAsync(Guid userId, CancellationToken ct = default);
    Task<int> ResetAutoMonthlyEntitlementsAsync(CancellationToken ct = default);
    Task<int> SyncSubscriptionStatusesByTimeAsync(CancellationToken ct = default);
    Task PauseOtherActiveByUserAsync(Guid userId, Guid keepActiveId, CancellationToken ct = default);
}
