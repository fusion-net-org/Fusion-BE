using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fusion.Repository.Enums
{
    public enum NotificationTypeEnum
    {
        BUSINESS,
        SYSTEM,
        PROJECT,
        TASK,
        COMPANY,
        WARNING,
        PARTNER,
        PROJECT_REQUEST,
        INFO,
        ADMIN_NOTIFICATE
    }

    public enum NotificationStatusEnum
    {
        TURN_OFF,
        TURN_ON
    }
}
