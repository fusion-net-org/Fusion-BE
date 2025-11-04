using Fusion.Repository.Bases.Page;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fusion.Service.ViewModels.Project.Requests
{
    public class ProjectPagedSearchRequest : PagedRequest
    {
        public string? Keyword { get; set; }
        public string? Status { get; set; }
        public Guid? CompanyId { get; set; }
        public DateOnly? DateFrom { get; set; }
        public DateOnly? DateTo { get; set; }
    }
}
