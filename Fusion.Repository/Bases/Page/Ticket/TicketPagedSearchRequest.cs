using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fusion.Repository.Bases.Page.Ticket
{
	public class TicketPagedSearchRequest : PagedRequest
	{
		public string? TicketName { get; set; }
	}
    public class TicketByProjectPagedRequest : PagedRequest
    {
        public Guid ProjectId { get; set; }
        public string? TicketName { get; set; }

        public string? Priority { get; set; }
        public decimal? MinBudget { get; set; }
        public decimal? MaxBudget { get; set; }

        public DateTime? ResolvedFrom { get; set; }
        public DateTime? ResolvedTo { get; set; }

        public DateTime? ClosedFrom { get; set; }
        public DateTime? ClosedTo { get; set; }

        public DateTime? CreateFrom { get; set; }
        public DateTime? CreateTo { get; set; }
    }
}
