using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fusion.Repository.ViewModels.Project
{
    public class CloseProjectResponse
    {
        public bool NeedConfirm { get; set; }
        public bool SentToRequester { get; set; }
        public object? Summary { get; set; }
    }

    public class CloseProjectSummaryDto
    {
        public int ProjectPercent { get; set; }
        public int TicketPercent { get; set; }
        public int TotalTickets { get; set; }

        public bool IsMaintenance { get; set; }

        public List<ComponentSummaryDto>? Components { get; set; }
    }

    public class ComponentSummaryDto
    {
        public Guid ComponentId { get; set; }
        public string ComponentName { get; set; } = null!;

        public int TotalTasks { get; set; }
        public int ClosedTasks { get; set; }
        public int Percent { get; set; }
    }
}
