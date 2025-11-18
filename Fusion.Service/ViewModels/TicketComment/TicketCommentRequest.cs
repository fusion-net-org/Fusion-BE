using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fusion.Service.ViewModels.TicketComment
{
    public class TicketCommentRequest
    {
        public Guid? TicketId { get; set; }
        public Guid? AuthorUserId;  
        public string? Body { get; set; }          
    }

    public class TicketCommentRequestUpdate
    {
        public string? Body { get; set; }          
    }
}
