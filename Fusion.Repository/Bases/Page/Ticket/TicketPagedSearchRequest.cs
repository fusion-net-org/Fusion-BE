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
}
