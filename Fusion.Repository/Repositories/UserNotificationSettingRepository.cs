using Fusion.Repository.Data;
using Fusion.Repository.Entities;
using Fusion.Repository.IRepositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Fusion.Repository.Repositories
{
    public class UserNotificationSettingRepository : GenericRepository<UserNotificationSetting>, IUserNotificationSettingRepository
    {
        private readonly FusionDbContext _context;

        public UserNotificationSettingRepository(FusionDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<UserNotificationSetting?> GetUserNotificationByType(Guid userId, string? notificationType, CancellationToken cancellationToken = default)
        {
            return await _context.UserNotificationSettings.FirstOrDefaultAsync(x => x.UserId == userId
                              && x.NotificationType.ToString() == notificationType, cancellationToken);
        }
    }
}
