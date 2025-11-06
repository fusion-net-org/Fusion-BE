using Fusion.Repository.Data;
using Fusion.Repository.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fusion.Repository.IRepositories
{
    public interface IUserNotificationSettingRepository: IGenericRepository<UserNotificationSetting> 
    {
        Task<UserNotificationSetting?> GetUserNotificationByType(Guid userId, string? notificationType, CancellationToken cancellationToken = default);
    }
}
