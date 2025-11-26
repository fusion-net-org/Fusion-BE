using Fusion.API.Context;
using Fusion.Repository.Bases.Page;
using Fusion.Service.Commons.BaseResponses;
using Fusion.Service.IServices;
using Fusion.Service.ViewModels.Companies.Responses;
using Fusion.Service.ViewModels.Notifications.Requests;
using Fusion.Service.ViewModels.Notifications.Responses;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;

namespace Fusion.API.Controllers
{
    [Route("api/notifications")]
    [ApiController]
    public class NotificationsController : ControllerBase
    {
        private readonly INotificationService _notificationService;

        public NotificationsController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        [HttpGet("user")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseModel<NotificationResponse>))]
        public async Task<IActionResult> GetMyNotifications(CancellationToken ct)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized(ResponseModel<string>.Error(
                    StatusCodes.Status401Unauthorized,
                    "Don't find token!"));
            }

            var result = await _notificationService.GetUserNotificationsAsync(userId, ct);

            return Ok(ResponseModel<IEnumerable<NotificationResponse>>.Ok(
                data: result,
                message: "Get notifications successfully"
            ));
        }

        [HttpGet("admin")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseModel<PagedResult<NotificationResponse>>))]
        public async Task<IActionResult> GetAdminNotifications([FromQuery] PagedRequest request, CancellationToken ct)
        {
            var result = await _notificationService.GetAdminNotificationsAsync(request, ct);

            return Ok(ResponseModel<PagedResult<NotificationResponse>>.Ok(
                data: result,
                message: "Get notifications admin successfully"
            ));
        }

        [HttpPut("{notificationId:guid}/read")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseModel<string>))]

        public async Task<IActionResult> MarkAsRead(Guid notificationId, CancellationToken ct)
        {

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized(ResponseModel<string>.Error(
                    StatusCodes.Status401Unauthorized,
                    "Don't find token!"));
            }

            await _notificationService.MarkAsReadAsync(userId, notificationId, ct);

            return Ok(ResponseModel<string>.Ok(
                data: null,
                message: "Marked notification as read successfully"
            ));
        }

        [HttpPost("send")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseModel<string>))]
        public async Task<IActionResult> SendNotification([FromBody] SendNotificationRequest request)
        {
            await _notificationService.CreateNotificationAsync(request);

            return Ok(ResponseModel<string>.Ok(
                data: null,
                message: "Send Notification success"
                ));
        }

        [HttpPost("send/all")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseModel<string>))]
        public async Task<IActionResult> SendAllNotification([FromBody] SendAllNotificationRequest request)
        {
            await _notificationService.SendAllNotificationAsync(request);

            return Ok(ResponseModel<string>.Ok(
                data: null,
                message: "Send Notification success"
                ));
        }

        [HttpPost("send/task/{taskId:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseModel<string>))]
        public async Task<IActionResult> SendTaskCommentNotification(Guid taskId, [FromBody] SendTaskCommentNotificationRequest request, CancellationToken token = default)
        {

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized(ResponseModel<string>.Error(
                    StatusCodes.Status401Unauthorized,
                    "Don't find token!"));
            }

            await _notificationService.SendNotificationToTaskMembersAsync(taskId, userId, request, token);

            return Ok(ResponseModel<string>.Ok(
                data: null,
                message: "Send Notification success"
                ));
        }

        [HttpDelete("{notificationId:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseModel<string>))]
        public async Task<IActionResult> DeleteNotificationById(Guid notificationId, CancellationToken cancellationToken)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized(ResponseModel<string>.Error(
                    StatusCodes.Status401Unauthorized,
                    "Don't find token!"));
            }

            await _notificationService.DeleteNotificationAsync(userId, notificationId, cancellationToken);

            return Ok(ResponseModel<string>.Ok(
                data: null,
                message: "Delete Notification success"
                ));
        }

        [HttpDelete("all")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseModel<string>))]
        public async Task<IActionResult> DeleteAllNotification(CancellationToken cancellationToken)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized(ResponseModel<string>.Error(
                    StatusCodes.Status401Unauthorized,
                    "Don't find token!"));
            }

            await _notificationService.DeleteAllNotificationByUserIdAsync(userId, cancellationToken);

            return Ok(ResponseModel<string>.Ok(
                data: null,
                message: "Delete all Notification success"
                ));
        }

        [HttpDelete("admin")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseModel<string>))]
        public async Task<IActionResult> DeleteAdminNotification(CancellationToken cancellationToken)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized(ResponseModel<string>.Error(
                    StatusCodes.Status401Unauthorized,
                    "Don't find token!"));
            }

            await _notificationService.DeleteAdminNotificationAsync(userId, cancellationToken);

            return Ok(ResponseModel<string>.Ok(
                data: null,
                message: "Delete admin Notification success"
                ));
        }

        [HttpPost("toggle")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseModel<string>))]
        public async Task<IActionResult> ToggleNotification([FromBody] ToggleNotificationRequest request, CancellationToken cancellationToken)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized(ResponseModel<string>.Error(
                    StatusCodes.Status401Unauthorized,
                    "Don't find token!"));
            }

            await _notificationService.ToggleNotificationByTypeAsync(userId, request, cancellationToken);
            var status = request.isEnable.Value ? "enabled" : "disabled";

            return Ok(ResponseModel<string>.Ok(
                data: null,
                message: $"Notification type '{request.type}' has been {status} for user {userId}"
                ));
        }

    }
}
