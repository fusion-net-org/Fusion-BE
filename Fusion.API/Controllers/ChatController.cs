
using Fusion.Repository.Bases.Responses;
using Fusion.Service.Commons.BaseResponses;
using Fusion.Service.IServices;
using Fusion.Service.ViewModels.ChatMessage.Requests;
using Fusion.Service.ViewModels.ChatMessage.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Fusion.API.Controllers
{
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

        // 1) A bấm Message với B => Create-or-Get DM
        [HttpPost("direct/{otherUserId:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseModel<ChatConversationResponse>))]
        public async Task<IActionResult> OpenDirect([FromRoute] Guid otherUserId, CancellationToken ct)
        {
            var result = await _chatService.OpenDirectChatAsync(otherUserId, ct);

            return Ok(ResponseModel<ChatConversationResponse>.Ok(
                data: result,
                message: ResponseMessageHelper.FormatMessage(ResponseMessages.GET_SUCCESS, "direct conversation")));
        }

        // 2) A tạo group và mời B,C (Option A)
        [HttpPost("group")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseModel<ChatConversationResponse>))]
        public async Task<IActionResult> CreateGroup([FromBody] CreateGroupChatRequest request, CancellationToken ct)
        {
            var result = await _chatService.CreateGroupChatAsync(request, ct);

            return Ok(ResponseModel<ChatConversationResponse>.Ok(
                data: result,
                message: ResponseMessageHelper.FormatMessage(ResponseMessages.CREATE_SUCCESS, "group conversation")));
        }
    }
}
