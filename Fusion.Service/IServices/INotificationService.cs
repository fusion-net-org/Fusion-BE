using Fusion.Repository.Bases.Page;
using Fusion.Repository.Enums;
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
        public Task<PagedResult<NotificationResponse>> GetAdminNotificationsAsync(PagedRequest pagedRequest, CancellationToken cancellationToken = default);
        public Task MarkAsReadAsync(Guid userId, Guid notificationId, CancellationToken cancellationToken = default);
        public Task CreateNotificationAsync(SendNotificationRequest request, CancellationToken cancellationToken = default);

        public Task SendNotificationToTaskMembersAsync(Guid taskId, Guid userId, SendTaskCommentNotificationRequest request, CancellationToken cancellationToken = default);
        public Task SendAllNotificationAsync(SendAllNotificationRequest request, CancellationToken cancellationToken = default);
        public Task DeleteNotificationAsync(Guid userId, Guid notificationId, CancellationToken cancellationToken = default);
        public Task DeleteAllNotificationByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
        public Task DeleteAdminNotificationAsync(Guid userId, CancellationToken cancellationToken = default);
        public Task ToggleNotificationByTypeAsync(Guid userId, ToggleNotificationRequest? request, CancellationToken cancellationToken = default);


    }
}
