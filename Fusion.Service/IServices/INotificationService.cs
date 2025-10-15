using Fusion.Service.ViewModels.Notifications.Requests;
using Fusion.Service.ViewModels.Notifications.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fusion.Service.IServices
{
    public interface INotificationService
    {
        public Task<IEnumerable<NotificationResponse>> GetUserNotificationsAsync(Guid userId, CancellationToken cancellationToken = default);
        public Task MarkAsReadAsync(Guid userId, Guid notificationId, CancellationToken cancellationToken = default);
        public Task CreateAsync(SendNotificationRequest request, CancellationToken cancellationToken = default);


    }
}
