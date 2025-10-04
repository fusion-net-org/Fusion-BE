using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fusion.Repository.Enums
{
    public enum ProjectRequestStatusEnum
    {
        Pending = 0,
        Accepted = 1,
        Rejected = 2,
        Finished = 3
    }

    public enum ProjectRequestViewMode
    {
        AsRequester, // Công ty thuê
        AsExecutor   // Công ty được thuê
    }
}
