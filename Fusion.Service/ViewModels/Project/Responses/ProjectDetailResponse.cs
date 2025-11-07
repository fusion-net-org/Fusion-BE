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
        public Guid? ProjectRequestId { get; set; }
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
        public List<ProjectTaskDto> Tasks { get; set; } = new();
    }

    public class SprintDto
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? Color { get; set; }
        public string? Goal { get; set; }
        public string? Status { get; set; }
        public bool IsDeleted { get; set; }
    }

    public class ProjectTaskDto
    {
        public Guid Id { get; set; }
        public Guid? SprintId { get; set; }
        public string? Title { get; set; }
        public string? Type { get; set; }
        public string? Priority { get; set; }
        public string? Status { get; set; }
        public bool IsBacklog { get; set; }
        public int? Point { get; set; }
        public DateTime? DueDate { get; set; }
        public bool IsDeleted { get; set; }
        public string? Img { get; set; }

        public int? OrderInSprint { get; set; }
        public string? Source { get; set; }
    }
}