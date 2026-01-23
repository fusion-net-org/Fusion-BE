using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fusion.Repository.Enums
{
    public enum TicketStatusEnum
    {
        Pending = 0,
        Accepted = 1,
        Rejected = 2,
        WaitingForCloseApproval = 3,
        Closed = 4,
    }
    public enum TicketViewMode
    {
        AsRequester, 
        AsExecutor   
    }
    public enum TicketType
    {
        Bug,
        Enhancement,
        NewFeature,
        Hotfix,
        Improvement,
        Spike
    }

}
