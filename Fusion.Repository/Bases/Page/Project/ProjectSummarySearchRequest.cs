using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fusion.Repository.Bases.Page.Project
{
    public class ProjectSummarySearchRequest : PagedRequest
    {
        public Guid? CompanyId { get; set; }

        public string? CompanyName { get; set; }

        public string? ProjectName { get; set; }
    }
}
