using Fusion.Repository.Data;
using Fusion.Repository.Entities;
using Fusion.Repository.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fusion.Repository.IRepositories
{
    public interface INotificationRepository: IGenericRepository<Notification>
    {
        Task<IEnumerable<Notification>> GetUserNotificationsAsync(Guid userId, CancellationToken cancellationToken = default);
        Task MarkAsReadAsync(Guid userId, Guid notificationId, CancellationToken cancellationToken = default);
        Task<Notification> CreateAsync(Notification notification, string? type, string? linkUrlWeb = null, string? linkUrlMobile = null, CancellationToken cancellationToken = default);

        Task<Notification> CreateAdminNotificationAsync(Notification notification, CancellationToken cancellationToken = default);

        Task DeleteNotificationAsync(Guid userId, Guid notificationId, CancellationToken cancellationToken = default);

        Task DeleteAllNotificationByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

        Task ToggleNotificationByTypeAsync(Guid userId, NotificationTypeEnum type, bool? isEnable, CancellationToken cancellationToken = default);
    }
}
