using Fusion.Repository.Repositories;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fusion.Service.ViewModels.Project.Requests
{
    public class ProjectCreateRequest
    {
        public bool IsHired { get; set; }
        public Guid? CompanyHiredId { get; set; }

        public string Code { get; set; } = default!;
        public string Name { get; set; } = default!;
        public string? Description { get; set; }
        public string Status { get; set; } = "Planned";

        public DateOnly? StartDate { get; set; }
        public DateOnly? EndDate { get; set; }
        public int SprintLengthWeeks { get; set; } = 1;

        // IMPORTANT: chỉ nhận sẵn WorkflowId (đã có workflow rồi)
        public Guid WorkflowId { get; set; }

        // Các user (thuộc công ty chính hoặc công ty thuê nếu IsHired)
        public List<Guid> MemberIds { get; set; } = new();
    }
    public class SprintDto
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }
        public DateTime? StartDate { get; set; }   // map thẳng kiểu DateTime? để tránh mismatch
        public DateTime? EndDate { get; set; }
        public int? Status { get; set; }           // nếu Sprint.Status đang là int
        public int? TotalTask { get; set; }        // optional, có thể null nếu chưa tính
    }

   
}
