using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fusion.Service.ViewModels.Tickets.Responses
{
    public class TicketHistoryResponse
    {
        public long Id { get; set; }
        public string Action { get; set; } = null!;
        public string? Description { get; set; }
        public Guid? PerformedBy { get; set; }
        public string? PerformedByName { get; set; }
        public DateTime CreatedAt { get; set; }
    }

}
