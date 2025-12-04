using Fusion.Repository.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fusion.Repository.Bases.Page.ProjectRequest
{
    public class ProjectRequestSearchRequest : PagedRequest
    {
        public string? Keyword { get; set; } 
        public ProjectRequestStatusEnum? Status { get; set; }
        public bool? Deleted { get; set; }
        public bool? IsHaveProject { get; set; }
        public ProjectRequestViewMode? ViewMode { get; set; }

        public DateFilterType? DateFilterType { get; set; }
        public DateRange<DateOnly>? DateRange { get; set; }

    }

    public class ProjectRequestSearchAdminRequest : PagedRequest
    {
        public Guid? CompanyId { get; set; }
        public string? Keyword { get; set; }
        public ProjectRequestStatusEnum? Status { get; set; }
        public bool? Deleted { get; set; }
        public bool? IsHaveProject { get; set; }
        public ProjectRequestViewMode? ViewMode { get; set; }

        public DateFilterType? DateFilterType { get; set; }
        public DateRange<DateOnly>? DateRange { get; set; }

    }
}
