using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fusion.Repository.Bases.Page.Ticket
{
    public class TicketStatusCountResponse
    {
        public Dictionary<string, int> StatusCounts { get; set; } = new();
        public int Total { get; set; }
    }
}
