using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fusion.Repository.Bases.Page.TicketComment
{
    public class TicketCommentPagedRequest : PagedRequest
    {
        public Guid? TicketId { get; set; }
        public string? SearchText { get; set; } 
        public DateTime? From { get; set; }     
        public DateTime? To { get; set; }   
    }

}
