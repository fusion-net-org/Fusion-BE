using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fusion.Service.ViewModels.Notifications.Requests
{
    public class FCMNotificationRequest
    {
        public Guid NotificationId { get; set; }   // Id của notification trong DB
        public Guid? UserId { get; set; }           // User nhận notification
        public string Title { get; set; }          // Tiêu đề notification
        public string? Body { get; set; }          // Nội dung notification (optional)
        public string? Type { get; set; }          // SYSTEM / BUSINESS / ... (optional)
        public string? LinkUrlWeb { get; set; }    // Link cho web (optional)
        public string? LinkUrlMobile { get; set; } // Link cho mobile (optional)
    }
}
