using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fusion.Service.ViewModels.Tickets.Responses
{
    public class TicketDashboardResponse
    {
        public List<TicketStatusChartItem> TicketStatusData { get; set; } = new();
        public List<BudgetByPriorityItem> BudgetByPriority { get; set; } = new();
        public List<TicketPriorityChartItem> TicketPriorityData { get; set; } = new();
        public List<ResolvedClosedChartItem> ResolvedAndClosedData { get; set; } = new();
        public List<ResolvedClosedTimelineItem> ResolvedClosedTimeline { get; set; } = new();
    }

    public class TicketStatusChartItem
    {
        public string Name { get; set; } = string.Empty;
        public int Value { get; set; }
    }

    public class BudgetByPriorityItem
    {
        public string Status { get; set; } = string.Empty;
        public decimal Budget { get; set; }
    }

    public class TicketPriorityChartItem
    {
        public string Priority { get; set; } = string.Empty;
        public int Value { get; set; }
    }

    public class ResolvedClosedChartItem
    {
        public string Name { get; set; } = string.Empty;
        public int Value { get; set; }
    }

    public class ResolvedClosedTimelineItem
    {
        public string Date { get; set; } = string.Empty;
        public int Resolved { get; set; }
        public int Closed { get; set; }
    }
}
