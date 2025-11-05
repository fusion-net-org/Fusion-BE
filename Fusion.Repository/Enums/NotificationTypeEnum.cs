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
        INFO
    }

    public enum NotificationStatusEnum
    {
        TURN_OFF,
        TURN_ON
    }
}
