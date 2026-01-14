

using Fusion.Repository.Bases.Page;
using Fusion.Repository.Bases.Page.Friend;
using Fusion.Repository.ViewModels;
using Fusion.Service.ViewModels.UserFriend.Requests;
using Fusion.Service.ViewModels.UserFriend.Responses;

namespace Fusion.Service.IServices;

public interface IFriendshipService
{
    Task<FriendshipResponse> SendRequestByEmailAsync(CreateFriendRequest dto, CancellationToken ct = default);
    Task<List<FriendshipResponseV2>> GetPendingReceivedAsync(CancellationToken ct = default);
    Task<List<FriendshipResponseV2>> GetPendingSentAsync(CancellationToken ct = default);

    Task<FriendshipResponse> AcceptAsync(Guid friendshipId, CancellationToken ct = default);
    Task<FriendshipResponse> RejectAsync(Guid friendshipId, CancellationToken ct = default);

    Task<FriendshipResponse> CancelAsync(Guid friendshipId, CancellationToken ct = default);
    Task<FriendshipResponse> UnfriendAsync(Guid friendshipId, CancellationToken ct = default);

    Task<PagedResult<FriendLiteResponse>> GetPagedFriendsByUserIdAsync(UserFriendPagedRequest request, CancellationToken ct = default);
}
