using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fusion.Service.ViewModels.Project.Responses
{
    public class ProjectListItemResponse
    {
        public Guid Id { get; set; }
        public string? Code { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }

        public Guid? CompanyId { get; set; }
        public string? OwnerCompany { get; set; }
        public string? HiredCompany { get; set; }
        public Guid? RequestCompany { get; set; }
        public string? RequestName { get; set; }
        public string? Workflow { get; set; } // "Company — WorkflowName" (nếu có)
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool IsClosed { get; set; } = false;

        public Guid? ClosedBy { get; set; }
        public string? Status { get; set; }    // Planned | InProgress | OnHold | Completed
        public string Ptype { get; set; } = "Internal"; // Internal | Outsourced
        public bool IsRequest { get; set; } = false;
    }
}
