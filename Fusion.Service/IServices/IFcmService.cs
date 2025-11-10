using Fusion.Service.ViewModels.Notifications.Requests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fusion.Service.IServices
{
    public interface IFcmService
    {
        public Task SendToUserAsync(FCMNotificationRequest request, string notificationType, CancellationToken cancellationToken = default);

        Task SendToAllAsync(FCMNotificationRequest request, CancellationToken cancellationToken = default);

    }
}
