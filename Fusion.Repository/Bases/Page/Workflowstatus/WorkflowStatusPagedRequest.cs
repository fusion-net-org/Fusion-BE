using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fusion.Repository.Bases.Page.Workflowstatus
{
    public class WorkflowStatusPagedRequest : PagedRequest
    {
        public Guid? ProjectId { get; set; }
        public string? Name { get; set; } 
    }
}
