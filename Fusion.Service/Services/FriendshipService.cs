using AutoMapper;
using Fusion.Repository.Bases.Exceptions;
using Fusion.Repository.Bases.Page;
using Fusion.Repository.Bases.Page.Friend;
using Fusion.Repository.Common;
using Fusion.Repository.Data;
using Fusion.Repository.Entities;
using Fusion.Repository.IRepositories;
using Fusion.Repository.ViewModels;
using Fusion.Service.Commons.Helpers;
using Fusion.Service.IServices;
using Fusion.Service.ViewModels.UserFriend.Requests;
using Fusion.Service.ViewModels.UserFriend.Responses;


namespace Fusion.Service.Services
{
    public class FriendshipService : IFriendshipService
    {
        private readonly IUserFriendshipRepository _friendRepo;
        private readonly IUserRepository _userRepo;
        private readonly IUnitOfWork _uow;
        private readonly ICurrentService _current;
        private readonly IChatService _chatService;

        public FriendshipService(
            IUserFriendshipRepository friendRepo,
            IUserRepository userRepo,
            IUnitOfWork uow,
            ICurrentService current,
            IChatService chatService)
        {
            _friendRepo = friendRepo;
            _userRepo = userRepo;
            _uow = uow;
            _current = current;
            _chatService = chatService;
        }

        public async Task<FriendshipResponse> SendRequestByEmailAsync(CreateFriendRequest dto, CancellationToken ct = default)
        {
            var me = _current.GetUserId();
            if (me == Guid.Empty) throw CustomExceptionFactory.CreateBadRequestError("Current user is not found.");

            var email = (dto.Email ?? "").Trim().ToLowerInvariant();
            if (string.IsNullOrWhiteSpace(email)) throw CustomExceptionFactory.CreateBadRequestError("Email is required.");

            var target = await _userRepo.GetUserByEmailAsync(email, ct);
            if (target == null) throw new KeyNotFoundException("User not found.");
            if (target.Id == me) throw CustomExceptionFactory.CreateBadRequestError("You cannot send a friend request to yourself.");

            var pairKey = PairKeyHelper.Build(me, target.Id);
            var existed = await _friendRepo.GetByPairKeyAsync(pairKey, ct);

            if (existed == null)
            {
                var fr = new UserFriendship
                {
                    Id = Guid.NewGuid(),
                    PairKey = pairKey,
                    RequesterId = me,
                    AddresseeId = target.Id,
                    Status = FriendshipStatus.Pending,
                    RequestedAt = DateTime.UtcNow,
                    RespondedAt = null
                };

                await _friendRepo.AddAsync(fr, ct);
                await _uow.SaveChangesAsync(ct);
                return ToVm(fr);
            }

            var st = existed.Status ?? -1;

            if (st == FriendshipStatus.Accepted)
                throw CustomExceptionFactory.CreateBadRequestError("You are already friends.");

            if (st == FriendshipStatus.Pending)
            {
                if (existed.RequesterId == me) return ToVm(existed);
                throw CustomExceptionFactory.CreateBadRequestError("You already have a pending request from this user. Please accept or reject it.");
            }

            if (st == FriendshipStatus.Rejected)
            {
                existed.RequesterId = me;
                existed.AddresseeId = target.Id;
                existed.Status = FriendshipStatus.Pending;
                existed.RequestedAt = DateTime.UtcNow;
                existed.RespondedAt = null;

                _friendRepo.Update(existed);
                await _uow.SaveChangesAsync(ct);
                return ToVm(existed);
            }

            throw CustomExceptionFactory.CreateBadRequestError("Cannot send friend request in current state.");
        }

        public async Task<List<FriendshipResponse>> GetPendingReceivedAsync(CancellationToken ct = default)
        {
            var me = _current.GetUserId();
            var list = await _friendRepo.GetPendingReceivedAsync(me, ct);
            return list.Select(ToVm).ToList();
        }

        public async Task<List<FriendshipResponse>> GetPendingSentAsync(CancellationToken ct = default)
        {
            var me = _current.GetUserId();
            var list = await _friendRepo.GetPendingSentAsync(me, ct);
            return list.Select(ToVm).ToList();
        }

        public async Task<FriendshipResponse> AcceptAsync(Guid friendshipId, CancellationToken ct = default)
        {
            var me = _current.GetUserId();

            var fr = await _friendRepo.GetByIdAsync(friendshipId, ct);
            if (fr == null) throw new KeyNotFoundException("Friend request not found.");
            if (fr.AddresseeId != me) throw CustomExceptionFactory.CreateForbiddenError();
            if ((fr.Status ?? -1) != FriendshipStatus.Pending) throw CustomExceptionFactory.CreateBadRequestError("This request is not pending.");

            fr.Status = FriendshipStatus.Accepted;
            fr.RespondedAt = DateTime.UtcNow;

            _friendRepo.Update(fr);
            await _uow.SaveChangesAsync(ct);

            // ✅ Auto create DM when friendship forms
            var a = fr.RequesterId ?? Guid.Empty;
            var b = fr.AddresseeId ?? Guid.Empty;
            if (a != Guid.Empty && b != Guid.Empty)
                await _chatService.EnsureDirectConversationExistsAsync(a, b, ct);

            return ToVm(fr);
        }

        public async Task<FriendshipResponse> RejectAsync(Guid friendshipId, CancellationToken ct = default)
        {
            var me = _current.GetUserId();

            var fr = await _friendRepo.GetByIdAsync(friendshipId, ct);
            if (fr == null) throw CustomExceptionFactory.CreateBadRequestError("Friend request not found.");
            if (fr.AddresseeId != me) throw CustomExceptionFactory.CreateForbiddenError();
            if ((fr.Status ?? -1) != FriendshipStatus.Pending) throw CustomExceptionFactory.CreateBadRequestError("This request is not pending.");

            fr.Status = FriendshipStatus.Rejected;
            fr.RespondedAt = DateTime.UtcNow;

            _friendRepo.Update(fr);
            await _uow.SaveChangesAsync(ct);
            return ToVm(fr);
        }

        // requester cancels pending
        public async Task<FriendshipResponse> CancelAsync(Guid friendshipId, CancellationToken ct = default)
        {
            var me = _current.GetUserId();

            var fr = await _friendRepo.GetByIdAsync(friendshipId, ct);
            if (fr == null) throw CustomExceptionFactory.CreateBadRequestError("Friend request not found.");
            if (fr.RequesterId != me) throw CustomExceptionFactory.CreateForbiddenError();
            if ((fr.Status ?? -1) != FriendshipStatus.Pending) throw CustomExceptionFactory.CreateBadRequestError("This request is not pending.");

            fr.Status = FriendshipStatus.Rejected;
            fr.RespondedAt = DateTime.UtcNow;

            _friendRepo.Update(fr);
            await _uow.SaveChangesAsync(ct);
            return ToVm(fr);
        }

        // unfriend (any side)
        public async Task<FriendshipResponse> UnfriendAsync(Guid friendshipId, CancellationToken ct = default)
        {
            var me = _current.GetUserId();

            var fr = await _friendRepo.GetByIdAsync(friendshipId, ct);
            if (fr == null) throw CustomExceptionFactory.CreateBadRequestError("Friendship not found.");
            if (fr.RequesterId != me && fr.AddresseeId != me) throw CustomExceptionFactory.CreateForbiddenError();
            if ((fr.Status ?? -1) != FriendshipStatus.Accepted) throw CustomExceptionFactory.CreateBadRequestError("You are not friends.");

            fr.Status = FriendshipStatus.Rejected;
            fr.RespondedAt = DateTime.UtcNow;

            _friendRepo.Update(fr);
            await _uow.SaveChangesAsync(ct);
            return ToVm(fr);
        }

        public async Task<PagedResult<FriendLiteResponse>> GetPagedFriendsByUserIdAsync(UserFriendPagedRequest request, CancellationToken ct = default)
        {
            var me = _current.GetUserId();
            if (me == Guid.Empty) throw CustomExceptionFactory.CreateBadRequestError("Current user is not found.");
            return await _friendRepo.GetPagedUserFriendsAsync(me, request, ct);
        }

        private static FriendshipResponse ToVm(UserFriendship x) => new()
        {
            Id = x.Id,
            RequesterId = x.RequesterId ?? Guid.Empty,
            AddresseeId = x.AddresseeId ?? Guid.Empty,
            Status = x.Status ?? -1,
            RequestedAt = x.RequestedAt,
            RespondedAt = x.RespondedAt
        };
    }
}
