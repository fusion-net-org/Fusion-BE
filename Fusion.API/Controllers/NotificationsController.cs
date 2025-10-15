using Fusion.API.Context;
using Fusion.Service.Commons.BaseResponses;
using Fusion.Service.IServices;
using Fusion.Service.ViewModels.Notifications.Requests;
using Fusion.Service.ViewModels.Notifications.Responses;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Fusion.API.Controllers
{
    [Route("api/notifications")]
    [ApiController]
    public class NotificationsController : ControllerBase
    {
        private readonly INotificationService _notificationService;
        private readonly ICompanyContextAccessor _companyContextAccessor;

        public NotificationsController(INotificationService notificationService, ICompanyContextAccessor companyContextAccessor)
        {
            _notificationService = notificationService;
            _companyContextAccessor = companyContextAccessor;
        }

        [HttpGet]
        public async Task<IActionResult> GetMyNotifications(CancellationToken ct)
        {
            var result = await _notificationService.GetUserNotificationsAsync(_companyContextAccessor.Current.UserId, ct);

            return Ok(ResponseModel<IEnumerable<NotificationResponse>>.Ok(
                data: result,
                message: "Get notifications successfully"
            ));
        }

        [HttpPut("{notificationId:guid}/read")]
        public async Task<IActionResult> MarkAsRead(Guid notificationId, CancellationToken ct)
        {
            await _notificationService.MarkAsReadAsync(_companyContextAccessor.Current.UserId, notificationId, ct);

            return Ok(ResponseModel<string>.Ok(
                data: null,
                message: "Marked notification as read successfully"
            ));
        }

     
    }
}
