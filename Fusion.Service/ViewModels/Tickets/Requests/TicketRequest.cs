using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fusion.Service.ViewModels.Tickets.Requests
{
	public class TicketRequest
	{
		public Guid? ProjectId { get; set; }
		public string? Priority { get; set; }
		public bool? IsHighestUrgen { get; set; }
		public string? TicketName { get; set; }
		public string? Description { get; set; }
		//public Guid? StatusId { get; set; }
		public Guid? SubmittedBy;
		public decimal? Budget { get; set; }
	}
}
