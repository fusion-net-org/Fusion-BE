using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fusion.Service.ViewModels.Tickets.Responses
{
    public sealed class TicketProcessSummaryResponse
    {
        
        public bool HasExecution { get; set; }

      
        public int TotalNonBacklogTasks { get; set; }

      
        public int StartedCount { get; set; }

       
        public int DoneCount { get; set; }

      
        public decimal ProgressPercent { get; set; }

     
        public DateTimeOffset? FirstStartedAt { get; set; }

      
        public DateTimeOffset? LastDoneAt { get; set; }

     
        public List<TicketProcessItemResponse> Items { get; set; } = new();
    }
}
