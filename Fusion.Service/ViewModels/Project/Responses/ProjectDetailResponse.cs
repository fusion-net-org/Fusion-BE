using Fusion.Service.ViewModels.Project.Requests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fusion.Service.ViewModels.Project.Responses
{
    public class ProjectDetailResponse
    {
        // ---- Project Info ----
        public Guid Id { get; set; }
        public string? Code { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? Status { get; set; }

        public bool IsHired { get; set; }
        public Guid? CompanyId { get; set; }
        public Guid? CompanyHiredId { get; set; }

        public DateTime? StartDate { get; set; }   // giữ DateTime? để khớp entity hiện tại
        public DateTime? EndDate { get; set; }

        public Guid? CreatedBy { get; set; }
        public DateTime CreateAt { get; set; }
        public DateTime UpdateAt { get; set; }

        // ---- Navigation data (optional) ----
        public string? CompanyName { get; set; }
        public string? CompanyHiredName { get; set; }
        public string? CreatedByName { get; set; }

        // ---- Sprint ----
        public List<SprintDto> Sprints { get; set; } = new();
    }
}
