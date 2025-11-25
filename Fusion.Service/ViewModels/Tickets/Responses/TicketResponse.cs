using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fusion.Repository.Bases.Page;

namespace Fusion.Service.ViewModels.Tickets.Responses
{
	public class TicketResponse
	{
		public Guid? Id { get; set; }
		public Guid? ProjectId { get; set; }
		public string? ProjectName { get; set; }
		public string? Priority { get; set; }
		public bool? IsHighestUrgen { get; set; }
		public string? TicketName { get; set; }
		public string? Description { get; set; }
		public Guid? StatusId { get; set; }
		public Guid? SubmittedBy { get; set; }
		public string? SubmittedByName {get;set;}
		public bool? IsBillable { get; set; }
		public decimal? Budget { get; set; }
		public bool? IsDeleted { get; set; }
		public string? Status { get; set; }
        public DateTime? ResolvedAt { get; set; }
		public DateTime? ClosedAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public Dictionary<string, int> StatusCounts { get; set; } = new();
        public int Total { get; set; }

    }
    public class TicketPagedResponse
    {
        public PagedResult<TicketResponse> PageData { get; set; }
        public Dictionary<string, int> StatusCounts { get; set; }
        public int Total { get; set; }
    }

}
