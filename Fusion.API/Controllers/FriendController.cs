using Fusion.Repository.Bases.Page;
using Fusion.Repository.Bases.Page.Friend;
using Fusion.Repository.Bases.Responses;
using Fusion.Repository.ViewModels;
using Fusion.Service.Commons.BaseResponses;
using Fusion.Service.IServices;
using Fusion.Service.ViewModels.UserFriend.Requests;
using Fusion.Service.ViewModels.UserFriend.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Fusion.API.Controllers;


/// <summary>
/// FriendController chịu trách nhiệm quản lý toàn bộ luồng "kết bạn":
/// - Gửi lời mời kết bạn theo email
/// - Xem danh sách lời mời pending (đã gửi / đã nhận)
/// - Accept / Reject / Cancel / Unfriend
/// - Lấy danh sách bạn bè/pending dạng phân trang để FE hiển thị

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class FriendController : ControllerBase
{
    private readonly IFriendshipService _friendshipService;

    public FriendController(IFriendshipService friendshipService)
    {
        _friendshipService = friendshipService;
    }

        /// <summary>
        /// Gửi lời mời kết bạn theo email.
        /// - Nếu chưa tồn tại quan hệ => tạo record Pending
        /// - Nếu đã Pending do mình gửi => trả lại record
        /// - Nếu đã Pending do đối phương gửi => báo lỗi (yêu cầu accept/reject)
        /// - Nếu đã Accepted => báo lỗi "already friends"
        /// - Nếu Rejected => cho phép gửi lại (reset Pending)
        /// </summary>
        [HttpPost("request")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseModel<FriendshipResponse>))]
        public async Task<IActionResult> SendRequest([FromBody] CreateFriendRequest request, CancellationToken ct)
        {
            var result = await _friendshipService.SendRequestByEmailAsync(request, ct);

            return Ok(ResponseModel<FriendshipResponse>.Ok(
                data: result,
                message: ResponseMessageHelper.FormatMessage(ResponseMessages.CREATE_SUCCESS, "friend request")));
        }

        /// <summary>
        /// Lấy danh sách các lời mời kết bạn Pending mà user hiện tại ĐÃ NHẬN (addressee = me).
        /// Dùng cho màn hình "Incoming requests".
        /// </summary>
        [HttpGet("pending/received")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseModel<List<FriendshipResponseV2>>))]
        public async Task<IActionResult> GetPendingReceived(CancellationToken ct)
        {
            var result = await _friendshipService.GetPendingReceivedAsync(ct);

            return Ok(ResponseModel<List<FriendshipResponseV2>>.Ok(
                data: result,
                message: ResponseMessageHelper.FormatMessage(ResponseMessages.GET_SUCCESS, "pending received friend requests")));
        }

        /// <summary>
        /// Lấy danh sách các lời mời kết bạn Pending mà user hiện tại ĐÃ GỬI (requester = me).
        /// Dùng cho màn hình "Sent requests".
        /// </summary>
        [HttpGet("pending/sent")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseModel<List<FriendshipResponseV2>>))]
        public async Task<IActionResult> GetPendingSent(CancellationToken ct)
        {
            var result = await _friendshipService.GetPendingSentAsync(ct);

            return Ok(ResponseModel<List<FriendshipResponseV2>>.Ok(
                data: result,
                message: ResponseMessageHelper.FormatMessage(ResponseMessages.GET_SUCCESS, "pending sent friend requests")));
        }

        /// <summary>
        /// Accept lời mời kết bạn.
        /// Rule:
        /// - Chỉ người nhận (addressee) mới được accept
        /// - Chỉ accept được khi status = Pending
        ///
        /// Side-effect (quan trọng):
        /// - Sau khi accept thành công => hệ thống tự tạo (Create-or-Get) direct conversation A_B
        ///   để 2 bên có phòng chat ngay khi friendship hình thành.
        /// </summary>
        [HttpPost("{friendshipId:guid}/accept")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseModel<FriendshipResponse>))]
        public async Task<IActionResult> Accept([FromRoute] Guid friendshipId, CancellationToken ct)
        {
            var result = await _friendshipService.AcceptAsync(friendshipId, ct);

            return Ok(ResponseModel<FriendshipResponse>.Ok(
                data: result,
                message: ResponseMessageHelper.FormatMessage(ResponseMessages.UPDATE_SUCCESS, "friend request")));
        }

        /// <summary>
        /// Reject lời mời kết bạn.
        /// Rule:
        /// - Chỉ người nhận (addressee) mới được reject
        /// - Chỉ reject được khi status = Pending
        /// </summary>
        [HttpPost("{friendshipId:guid}/reject")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseModel<FriendshipResponse>))]
        public async Task<IActionResult> Reject([FromRoute] Guid friendshipId, CancellationToken ct)
        {
            var result = await _friendshipService.RejectAsync(friendshipId, ct);

            return Ok(ResponseModel<FriendshipResponse>.Ok(
                data: result,
                message: ResponseMessageHelper.FormatMessage(ResponseMessages.UPDATE_SUCCESS, "friend request")));
        }

        /// <summary>
        /// Hủy lời mời kết bạn (Cancel).
        /// Rule:
        /// - Chỉ người gửi (requester) mới được cancel
        /// - Chỉ cancel được khi status = Pending
        /// </summary>
        [HttpPost("{friendshipId:guid}/cancel")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseModel<FriendshipResponse>))]
        public async Task<IActionResult> Cancel([FromRoute] Guid friendshipId, CancellationToken ct)
        {
            var result = await _friendshipService.CancelAsync(friendshipId, ct);

            return Ok(ResponseModel<FriendshipResponse>.Ok(
                data: result,
                message: ResponseMessageHelper.FormatMessage(ResponseMessages.UPDATE_SUCCESS, "friend request")));
        }

        /// <summary>
        /// Hủy kết bạn (Unfriend).
        /// Rule:
        /// - Một trong hai phía (requester hoặc addressee) đều có thể unfriend
        /// - Chỉ unfriend được khi status = Accepted
        /// </summary>
        [HttpPost("{friendshipId:guid}/unfriend")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseModel<FriendshipResponse>))]
        public async Task<IActionResult> Unfriend([FromRoute] Guid friendshipId, CancellationToken ct)
        {
            var result = await _friendshipService.UnfriendAsync(friendshipId, ct);

            return Ok(ResponseModel<FriendshipResponse>.Ok(
                data: result,
                message: ResponseMessageHelper.FormatMessage(ResponseMessages.UPDATE_SUCCESS, "friendship")));
        }

        /// <summary>
        /// Lấy danh sách bạn bè/pending dạng phân trang.
        /// Dùng cho:
        /// - Trang danh sách friend để chat/invite group
        /// - Search theo email
        /// - Filter theo status (Pending/Accepted)
        /// </summary>
        [HttpGet("paged")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseModel<PagedResult<FriendLiteResponse>>))]
        public async Task<IActionResult> GetPagedFriends([FromQuery] UserFriendPagedRequest request, CancellationToken ct)
        {
            var result = await _friendshipService.GetPagedFriendsByUserIdAsync(request, ct);

            return Ok(ResponseModel<PagedResult<FriendLiteResponse>>.Ok(
                data: result,
                message: ResponseMessageHelper.FormatMessage(ResponseMessages.GET_SUCCESS, "friends")));
        }
}
