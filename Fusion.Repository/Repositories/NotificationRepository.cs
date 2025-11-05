using Fusion.Repository.Bases.Exceptions;
using Fusion.Repository.Bases.Responses;
using Fusion.Repository.Data;
using Fusion.Repository.Entities;
using Fusion.Repository.Enums;
using Fusion.Repository.IRepositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fusion.Repository.Repositories
{
    public class NotificationRepository : GenericRepository<Notification>, INotificationRepository
    {
        private readonly FusionDbContext _context;

        public NotificationRepository(FusionDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<Notification> CreateAsync(Notification notification, string? type, string? linkUrlWeb = null, string? linkUrlMobile = null, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(notification.Title))
                throw CustomExceptionFactory.CreateBadRequestError(ResponseMessages.NOT_FOUND.FormatMessage("Notification"));

            var exists = await _context.Users.FindAsync(notification.UserId, cancellationToken);
            if (exists == null)
                throw CustomExceptionFactory.CreateNotFoundError(ResponseMessages.NOT_FOUND.FormatMessage("User"));

            if (type == NotificationTypeEnum.SYSTEM.ToString())
            {
                notification.IsRead = true;
                notification.ReadAt = DateTime.UtcNow.AddHours(7);
            }
            else
            {
                notification.IsRead = false;
            }

            notification.NotificationType = type;
            notification.IsDeleted = false;
            notification.CreateAt = DateTime.UtcNow.AddHours(7);
            notification.LinkUrlWeb = linkUrlWeb;
            notification.LinkUrlMobile = linkUrlMobile;

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();

            return notification;
        }

        public async Task<Notification> CreateAdminNotificationAsync(Notification notification, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(notification.Title))
                throw CustomExceptionFactory.CreateBadRequestError("Notification Title does not exist");
       

            notification.IsRead = true;
            notification.IsDeleted = false;
            notification.NotificationType = NotificationTypeEnum.INFO.ToString();
            notification.ReadAt = DateTime.UtcNow.AddHours(7);
            notification.CreateAt = DateTime.UtcNow.AddHours(7);
            notification.LinkUrlWeb = null;
            notification.LinkUrlMobile = null;

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();

            return notification;
        }

        public async Task<IEnumerable<Notification>> GetUserNotificationsAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            var exists = await _context.Users.FindAsync(userId, cancellationToken);
            if (exists == null)
                throw CustomExceptionFactory.CreateNotFoundError(ResponseMessages.NOT_FOUND.FormatMessage("User"));

            return await _context.Notifications
                .Where(x => x.UserId == userId && x.IsDeleted == false)
                .OrderByDescending(x => x.CreateAt)
               .ToListAsync(cancellationToken);
        }

        public async Task MarkAsReadAsync(Guid userId, Guid notificationId, CancellationToken cancellationToken = default)
        {
            if (userId == Guid.Empty || notificationId == Guid.Empty)
                throw CustomExceptionFactory.CreateBadRequestError(ResponseMessages.NOT_FOUND.FormatMessage("UserId or NotificationId"));


            var notification = await _context.Notifications.FirstOrDefaultAsync(x => x.Id == notificationId && x.UserId == userId, cancellationToken);
            if (notification == null) return;
            notification.IsRead = true;
            notification.ReadAt = DateTime.UtcNow.AddHours(7);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task DeleteNotificationAsync(Guid userId, Guid notificationId, CancellationToken cancellationToken = default)
        {
            if (userId == Guid.Empty || notificationId == Guid.Empty)
                throw CustomExceptionFactory.CreateBadRequestError(ResponseMessages.NOT_FOUND.FormatMessage("UserId or NotificationId"));

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                throw CustomExceptionFactory.CreateBadRequestError("User not exsited in system");

            var notification = await _context.Notifications.FirstOrDefaultAsync(x => x.Id == notificationId && x.UserId == userId, cancellationToken);
            if (notification == null)
                throw CustomExceptionFactory.CreateNotFoundError("Notification is not belong to user");

            notification.ReadAt = DateTime.UtcNow.AddHours(7);
            notification.IsDeleted = true;
            notification.IsRead = true;
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task DeleteAllNotificationByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            if (userId == Guid.Empty)
                throw CustomExceptionFactory.CreateBadRequestError(ResponseMessages.NOT_FOUND.FormatMessage("UserId"));

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                throw CustomExceptionFactory.CreateBadRequestError(ResponseMessages.NOT_FOUND.FormatMessage("UserId or NotificationId"));

            var notifications = await _context.Notifications.Where(x => x.UserId == userId).ToListAsync();
            if (notifications.Any())
                throw CustomExceptionFactory.CreateNotFoundError("User do not receive any notifications");

            foreach (var noti in notifications)
            {
                noti.ReadAt = DateTime.UtcNow.AddHours(7);
                noti.IsDeleted = true;
                noti.IsRead = true;
            }

            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task ToggleNotificationByTypeAsync(Guid userId, NotificationTypeEnum type, bool? isEnable, CancellationToken cancellationToken = default)
        {
            if (userId == Guid.Empty)
                throw CustomExceptionFactory.CreateBadRequestError(ResponseMessages.NOT_FOUND.FormatMessage("UserId"));

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                throw CustomExceptionFactory.CreateNotFoundError("User not found in the system");

            var setting = await _context.UserNotificationSettings
                .FirstOrDefaultAsync(x => x.UserId == userId && x.NotificationType == type.ToString(), cancellationToken);

            if (setting == null)
            {
                setting = new UserNotificationSetting
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    NotificationType = type.ToString(),
                    IsEnabled = isEnable,
                };
                await _context.UserNotificationSettings.AddAsync(setting, cancellationToken);
            }
            else
            {
                // Nếu có rồi -> đảo trạng thái (toggle)
                setting.IsEnabled = isEnable;
            }

            await _context.SaveChangesAsync(cancellationToken);
        }



    }
}
