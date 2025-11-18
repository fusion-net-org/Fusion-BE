using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fusion.Service.ViewModels.TicketComment
{
    public class TicketCommentResponse
    {
        public long Id { get; set; }
        public Guid? TicketId { get; set; }
        public Guid? AuthorUserId { get; set; }
        public string? AuthorUserName { get; set; }
        public string? AuthorUserAvatar { get; set; }
        public string? Body { get; set; }
        public DateTime CreateAt { get; set; }
        public DateTime UpdateAt { get; set; }
        public bool? IsDeleted { get; set; }
        public bool IsOwner { get; set; } 
    }
}
