using Fusion.Repository.Bases.Page;
using Fusion.Repository.Bases.Page.Friend;
using Fusion.Repository.Entities;
using Fusion.Repository.ViewModels;

namespace Fusion.Repository.IRepositories;

public interface IUserFriendshipRepository
{
    Task<UserFriendship?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<UserFriendship?> GetByPairKeyAsync(string pairKey, CancellationToken ct = default);

    Task<List<UserFriendship>> GetPendingSentAsync(Guid requesterId, CancellationToken ct = default);
    Task<List<UserFriendship>> GetPendingReceivedAsync(Guid addresseeId, CancellationToken ct = default);

    Task AddAsync(UserFriendship entity, CancellationToken ct = default);
    void Update(UserFriendship entity);

    Task<PagedResult<FriendLiteResponse>> GetPagedUserFriendsAsync(Guid userId, UserFriendPagedRequest request, CancellationToken ct = default);

}
