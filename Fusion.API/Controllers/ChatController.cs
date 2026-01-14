
using Fusion.Repository.Bases.Page;
using Fusion.Repository.Bases.Page.Chat;
using Fusion.Repository.Bases.Responses;
using Fusion.Repository.ViewModels.Chat;
using Fusion.Service.Commons.BaseResponses;
using Fusion.Service.IServices;
using Fusion.Service.ViewModels.ChatMessage.Requests;
using Fusion.Service.ViewModels.ChatMessage.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Fusion.API.Controllers
{

    /// <summary>
    /// ChatController cung cấp API phục vụ UI Chat:
    /// - Mở direct chat (A bấm message B)
    /// - Tạo group chat dựa trên quan hệ bạn bè (Accepted)
    /// - Invite thêm thành viên vào group (member/owner đều mời được friend accepted)
    /// - Lấy danh sách conversations (sidebar)
    /// - Lấy conversation detail theo id (members)
    /// - Lấy messages phân trang, và endpoint REST send message (optional ngoài SignalR)
    /// </summary>
    /// 
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ChatController : ControllerBase
    {
        private readonly IChatService _chatService;

        public ChatController(IChatService chatService)
        {
            _chatService = chatService;
        }

        /// <summary>
        /// Mở direct conversation giữa user hiện tại và otherUserId.
        /// - Chỉ cho phép nếu 2 user là bạn (Friendship = Accepted)
        /// - Create-or-Get theo chatKey "Dr_{pairKey}"
        /// - Trả về conversationId để FE join SignalR + load messages
        /// </summary>
        [HttpPost("direct/{otherUserId:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseModel<ChatConversationResponse>))]
        public async Task<IActionResult> OpenDirect([FromRoute] Guid otherUserId, CancellationToken ct)
        {
            var result = await _chatService.OpenDirectChatAsync(otherUserId, ct);

            return Ok(ResponseModel<ChatConversationResponse>.Ok(
                data: result,
                message: ResponseMessageHelper.FormatMessage(ResponseMessages.GET_SUCCESS, "direct conversation")));
        }

        /// <summary>
        /// Tạo group conversation.
        /// Rule:
        /// - Người tạo chỉ được thêm các member là bạn bè Accepted với mình.
        /// - Lưu chatKey dạng "Gr_{conversationId}" vào DirectPairKey (không đổi schema).
        /// </summary>
        [HttpPost("group")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseModel<ChatConversationResponse>))]
        public async Task<IActionResult> CreateGroup([FromBody] CreateGroupChatRequest request, CancellationToken ct)
        {
            var result = await _chatService.CreateGroupChatAsync(request, ct);

            return Ok(ResponseModel<ChatConversationResponse>.Ok(
                data: result,
                message: ResponseMessageHelper.FormatMessage(ResponseMessages.CREATE_SUCCESS, "group conversation")));
        }

        /// <summary>
        /// Invite thêm thành viên vào group.
        /// Rule:
        /// - Inviter (người mời) phải là member của group
        /// - Invitee phải là friend Accepted với inviter
        /// - Owner hoặc member đều có quyền invite
        /// </summary>
        [HttpPost("group/{conversationId:guid}/invite")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseModel<object>))]
        public async Task<IActionResult> InviteToGroup([FromRoute] Guid conversationId, [FromBody] InviteGroupMembersRequest request, CancellationToken ct)
        {
            await _chatService.InviteMembersToGroupAsync(conversationId, request, ct);

            return Ok(ResponseModel<object>.Ok(
                data: null,
                message: ResponseMessageHelper.FormatMessage(ResponseMessages.UPDATE_SUCCESS, "invite members")));
        }

        /// <summary>
        /// Lấy danh sách conversations của user hiện tại (sidebar chat).
        /// - Có thể filter type (Direct/Group)
        /// - Có thể search keyword (group: title, direct: peer email)
        /// - Sort theo LastMessageAt
        /// </summary>
        [HttpGet("conversations/paged")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseModel<PagedResult<ChatConversationListItemVm>>))]
        public async Task<IActionResult> GetConversationsPaged([FromQuery] ChatConversationPagedRequest request, CancellationToken ct)
        {
            var result = await _chatService.GetMyConversationsPagedAsync(request, ct);

            return Ok(ResponseModel<PagedResult<ChatConversationListItemVm>>.Ok(
                data: result,
                message: ResponseMessageHelper.FormatMessage(ResponseMessages.GET_SUCCESS, "conversations")));
        }

        /// <summary>
        /// Lấy thông tin chi tiết 1 conversation theo id.
        /// - Check quyền: user hiện tại phải là member của conversation
        /// - Trả về danh sách members để FE render header/participants
        /// </summary>
        [HttpGet("conversations/{conversationId:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseModel<ChatConversationDetailVm>))]
        public async Task<IActionResult> GetConversation([FromRoute] Guid conversationId, CancellationToken ct)
        {
            var result = await _chatService.GetConversationByIdAsync(conversationId, ct);

            return Ok(ResponseModel<ChatConversationDetailVm>.Ok(
                data: result,
                message: ResponseMessageHelper.FormatMessage(ResponseMessages.GET_SUCCESS, "conversation")));
        }

        /// <summary>
        /// Lấy danh sách messages theo phân trang.
        /// - Check quyền: user hiện tại phải là member của conversation
        /// - Mặc định sort newest -> oldest (tuỳ bạn trong repo)
        /// </summary>
        [HttpGet("conversations/{conversationId:guid}/messages/paged")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseModel<PagedResult<ChatMessageVm>>))]
        public async Task<IActionResult> GetMessagesPaged([FromRoute] Guid conversationId, [FromQuery] ChatMessagePagedRequest request, CancellationToken ct)
        {
            request.ConversationId = conversationId;
            var result = await _chatService.GetMessagesPagedAsync(conversationId, request, ct);

            return Ok(ResponseModel<PagedResult<ChatMessageVm>>.Ok(
                data: result,
                message: ResponseMessageHelper.FormatMessage(ResponseMessages.GET_SUCCESS, "messages")));
        }

        /// <summary>
        /// REST endpoint gửi message (optional ngoài SignalR).
        /// - Check member của conversation
        /// - Với direct: check friendship vẫn Accepted (anti-spam)
        /// - Có idempotency theo (ConversationId + SenderId + ClientMessageId)
        /// </summary>
        [HttpPost("conversations/{conversationId:guid}/messages")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseModel<ChatMessageResponse>))]
        public async Task<IActionResult> SendMessage([FromRoute] Guid conversationId, [FromBody] SendMessageRequest request, CancellationToken ct)
        {
            request.ConversationId = conversationId;
            var result = await _chatService.SendMessageAsync(request, ct);

            return Ok(ResponseModel<ChatMessageResponse>.Ok(
                data: result,
                message: ResponseMessageHelper.FormatMessage(ResponseMessages.CREATE_SUCCESS, "message")));
        }

        /// <summary>
        /// Kick 1 member khỏi group.
        /// Điều kiện: người gọi API phải là Owner của group.
        /// </summary>
        [HttpDelete("group/{conversationId:guid}/members/{targetUserId:guid}")]
        public async Task<IActionResult> KickMember(
            [FromRoute] Guid conversationId,
            [FromRoute] Guid targetUserId,
            CancellationToken ct)
        {
            await _chatService.KickMemberAsync(conversationId, targetUserId, ct);

            return Ok(ResponseModel<object>.Ok(
                data: null,
                message: ResponseMessageHelper.FormatMessage(ResponseMessages.UPDATE_SUCCESS, "kick member")));
        }
    }
}
