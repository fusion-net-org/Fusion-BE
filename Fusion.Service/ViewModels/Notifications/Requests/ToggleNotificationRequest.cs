using Fusion.Repository.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fusion.Service.ViewModels.Notifications.Requests
{
    public record ToggleNotificationRequest (NotificationTypeEnum? type, bool? isEnable);

}
