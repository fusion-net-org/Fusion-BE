using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fusion.Service.ViewModels.Notifications.Requests
{
    public class SendNotificationRequest
    {
        /// <summary>Người nhận thông báo</summary>
        public Guid UserId { get; set; }

        /// <summary>Tiêu đề thông báo</summary>
        public string Title { get; set; } = default!;

        /// <summary>Nội dung chi tiết (optional)</summary>
        public string? Body { get; set; }

        /// <summary>Khóa route (trỏ đến NotificationRouteMap)</summary>
        public string? LinkKey { get; set; }

        /// <summary>Id tham chiếu (thường là TaskId, ProjectId...)</summary>
        public Guid? IdLink { get; set; }

        /// <summary>Tên sự kiện (vd: TaskUpdated, ProjectCreated...)</summary>
        public string? Event { get; set; }

        /// <summary>Dữ liệu bổ sung (JSON hoặc context business)</summary>
        public string? Context { get; set; }

        public string? NotificationType { get; set; }
    }

    public record SendAllNotificationRequest(string? Title, string? Body, string? Event);

    public record SendTaskCommentNotificationRequest(string? Title, string? Body, string? Event);


}
