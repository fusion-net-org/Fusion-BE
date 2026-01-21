using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fusion.Repository.Enums
{
    public enum TicketHistoryAction
    {
        Created,
        Accepted,
        Rejected,
        CommentAdded,
        CommentUpdated,
        CommentDeleted,
        Closed,
        RequestClose,
    }
}
