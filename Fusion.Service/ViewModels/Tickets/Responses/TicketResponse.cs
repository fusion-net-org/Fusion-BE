using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fusion.Service.ViewModels.Tickets.Responses
{
	public class TicketResponse
	{
		public Guid? Id { get; set; }
		public Guid? ProjectId { get; set; }
		public string? Priority { get; set; }
		public string? Urgency { get; set; }
		public bool? IsHighestUrgen { get; set; }
		public string? TicketName { get; set; }
		public string? Description { get; set; }
		public Guid? StatusId { get; set; }
		public Guid? SubmittedBy { get; set; }
		public bool? IsBillable { get; set; }
		public decimal? Budget { get; set; }
		public bool? IsDeleted { get; set; }
		public DateTime? ResolvedAt { get; set; }
		public DateTime? ClosedAt { get; set; }
	}
}
