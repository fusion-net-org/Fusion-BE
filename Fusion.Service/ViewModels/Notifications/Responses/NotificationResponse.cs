using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fusion.Service.ViewModels.Notifications.Responses
{
    public class NotificationResponse
    {
        public Guid Id { get; set; }
        public string? Event { get; set; }
        public string? Title { get; set; }
        public string? Body { get; set; }
        public string? Context { get; set; }
        public string? LinkUrl { get; set; }
        public string? LinkUrlWeb { get; set; }
        public bool IsRead { get; set; }
        public DateTime CreateAt { get; set; }
        public DateTime? ReadAt { get; set; }
    }
}
