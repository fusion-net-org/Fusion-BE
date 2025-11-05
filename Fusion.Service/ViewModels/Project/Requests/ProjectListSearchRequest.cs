using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fusion.Service.ViewModels.Project.Requests
{
    public class ProjectListSearchRequest
    {
        public string? Q { get; set; }
        // Nếu FE gửi enum -> để enum; nếu FE gửi string -> để string.
        // Ở đây chọn enum cho an toàn rồi convert sang string ở service.
        public List<ProjectStatus>? Statuses { get; set; }
        public string? Sort { get; set; } = "recent"; // "recent" | "start" | "name"
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }

    public enum ProjectStatus { Planned, InProgress, OnHold, Completed }
}
