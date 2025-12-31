using AutoMapper;
using Fusion.Repository.Bases.Exceptions;
using Fusion.Repository.Bases.Page;
using Fusion.Repository.Bases.Page.Friend;
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
        private const int Pending = 0;
        private const int Accepted = 1;
        private const int Rejected = 2;

        private readonly IUserFriendshipRepository _friendRepo;
        private readonly IUserRepository _userRepo;
        private readonly IUnitOfWork _uow;
        private readonly ICurrentService _current;
        private readonly IMapper _mapper;

        public FriendshipService(IUserFriendshipRepository friendRepo,IUserRepository userRepo, IUnitOfWork uow, ICurrentService current, IMapper mapper)
        {
            _friendRepo = friendRepo;
            _userRepo = userRepo;
            _uow = uow;
            _current = current;
            _mapper = mapper;
        }


        public async Task<FriendshipResponse> SendRequestByEmailAsync(CreateFriendRequest dto, CancellationToken ct = default)
        {
            var currentUserId = _current.GetUserId();
            if (currentUserId == Guid.Empty)
                throw CustomExceptionFactory.CreateBadRequestError("Current user is not found.");

            var email = (dto.Email ?? "").Trim().ToLowerInvariant();
            if (string.IsNullOrWhiteSpace(email))
                throw CustomExceptionFactory.CreateBadRequestError("Email is required.");

            var target = await _userRepo.GetUserByEmailAsync(email, ct);
            if (target == null)
                throw new KeyNotFoundException("User not found.");

            if (target.Id == currentUserId)
                throw CustomExceptionFactory.CreateBadRequestError("You cannot send a friend request to yourself.");

            var pairKey = PairKeyHelper.Build(currentUserId, target.Id);

            var existed = await _friendRepo.GetByPairKeyAsync(pairKey, ct);

            // CHƯA CÓ => tạo mới pending
            if (existed == null)
            {
                var fr = new UserFriendship
                {
                    Id = Guid.NewGuid(),
                    PairKey = pairKey,
                    RequesterId = currentUserId,
                    AddresseeId = target.Id,
                    Status = Pending,
                    RequestedAt = DateTime.UtcNow,
                    RespondedAt = null
                };

                await _friendRepo.AddAsync(fr, ct);
                await _uow.SaveChangesAsync(ct);
                return ToVm(fr);
            }
            var st = existed.Status ?? -1;
            if (st == Accepted)
                throw CustomExceptionFactory.CreateBadRequestError("You are already friends.");

            // ĐANG PENDING
            if (st == Pending)
            {
                // mình đã gửi rồi => trả lại record
                if (existed.RequesterId == currentUserId)
                    return ToVm(existed);

                // người kia đã gửi mình => bắt buộc accept/reject
                throw CustomExceptionFactory.CreateBadRequestError("You already have a pending request from this user. Please accept or reject it.");
            }

            // BỊ REJECT => cho phép gửi lại (reset thành Pending)
            if (st == Rejected)
            {
                existed.RequesterId = currentUserId;
                existed.AddresseeId = target.Id;
                existed.Status = Pending;
                existed.RequestedAt = DateTime.UtcNow;
                existed.RespondedAt = null;

                _friendRepo.Update(existed);
                await _uow.SaveChangesAsync(ct);
                return ToVm(existed);
            }

            throw CustomExceptionFactory.CreateBadRequestError("Cannot send friend request in current state.");
        }

        public async Task<List<FriendshipResponse>> GetPendingReceivedAsync( CancellationToken ct = default)
        {
            var currentUserId = _current.GetUserId();

            var list = await _friendRepo.GetPendingReceivedAsync(currentUserId, ct);
            return list.Select(ToVm).ToList();
        }

        public async Task<List<FriendshipResponse>> GetPendingSentAsync( CancellationToken ct = default)
        {
            var currentUserId = _current.GetUserId();

            var list = await _friendRepo.GetPendingSentAsync(currentUserId, ct);
            return list.Select(ToVm).ToList();
        }
        public async Task<FriendshipResponse> AcceptAsync(Guid friendshipId, CancellationToken ct = default)
        {
            var currentUserId = _current.GetUserId();

            var fr = await _friendRepo.GetByIdAsync(friendshipId, ct);
            if (fr == null) throw new KeyNotFoundException("Friend request not found.");

            if (fr.AddresseeId != currentUserId)
                throw CustomExceptionFactory.CreateForbiddenError();

            if ((fr.Status ?? -1) != Pending)
                throw CustomExceptionFactory.CreateBadRequestError("This request is not pending.");

            fr.Status = Accepted;
            fr.RespondedAt = DateTime.UtcNow;

            _friendRepo.Update(fr);
            await _uow.SaveChangesAsync(ct);
            return ToVm(fr);

        }

        public async Task<FriendshipResponse> RejectAsync(Guid friendshipId, CancellationToken ct = default)
        {
            var currentUserId = _current.GetUserId();

            var fr = await _friendRepo.GetByIdAsync(friendshipId, ct);
            if (fr == null) throw CustomExceptionFactory.CreateBadRequestError("Friend request not found.");

            if (fr.AddresseeId != currentUserId)
                throw CustomExceptionFactory.CreateForbiddenError();

            if ((fr.Status ?? -1) != Pending)
                throw CustomExceptionFactory.CreateBadRequestError("This request is not pending.");

            fr.Status = Rejected;
            fr.RespondedAt = DateTime.UtcNow;

            _friendRepo.Update(fr);
            await _uow.SaveChangesAsync(ct);
            return ToVm(fr);
        }

        public async Task<PagedResult<FriendLiteResponse>> GetPagedFriendsByUserIdAsync(UserFriendPagedRequest request, CancellationToken ct = default)
        {
            var currentUserId = _current.GetUserId();
            if (currentUserId == Guid.Empty)
                throw CustomExceptionFactory.CreateBadRequestError("Current user is not found.");

            return await _friendRepo.GetPagedUserFriendsAsync(currentUserId, request, ct);
        }
        private static FriendshipResponse ToVm(UserFriendship x) => new FriendshipResponse
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
