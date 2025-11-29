using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fusion.Service.ViewModels.Tickets.Responses
{
    public sealed class TicketProcessItemResponse
    {
        public Guid TaskId { get; set; }
        public string TaskCode { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;

        public string StatusName { get; set; } = string.Empty;
        public string StatusCategory { get; set; } = string.Empty;
        public bool IsDone { get; set; }

        public DateTimeOffset? StartedAt { get; set; }

        public DateTimeOffset? LastMovedAt { get; set; }

        public DateTimeOffset? DoneAt { get; set; }
    }
}
