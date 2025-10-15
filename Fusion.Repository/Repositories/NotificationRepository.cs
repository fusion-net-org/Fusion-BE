using Fusion.Repository.Bases.Exceptions;
using Fusion.Repository.Bases.Responses;
using Fusion.Repository.Data;
using Fusion.Repository.Entities;
using Fusion.Repository.IRepositories;
using Microsoft.EntityFrameworkCore;
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

        public async Task CreateAsync(Notification notification, string? linkUrlWeb = null,string? linkUrlMobile = null, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(notification.Title))
                throw CustomExceptionFactory.CreateBadRequestError(ResponseMessages.NOT_FOUND.FormatMessage("Notification"));

            var exists = await _context.Users.FindAsync(notification.UserId, cancellationToken);
            if (exists == null)
                throw CustomExceptionFactory.CreateNotFoundError(ResponseMessages.NOT_FOUND.FormatMessage("User"));

            notification.IsRead = false;
            notification.CreateAt = DateTime.UtcNow.AddHours(7);
            notification.LinkUrlWeb = linkUrlWeb;
            notification.LinkUrlMobile = linkUrlMobile;

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<Notification>> GetUserNotificationsAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            var exists = await _context.Users.FindAsync(userId, cancellationToken);
            if (exists == null)
                throw CustomExceptionFactory.CreateNotFoundError(ResponseMessages.NOT_FOUND.FormatMessage("User"));

            return await _context.Notifications
                .Where(x => x.UserId == userId)
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
            notification.ReadAt = DateTime.UtcNow;
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
