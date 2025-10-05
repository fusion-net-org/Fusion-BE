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
        public string? Keyword { get; set; }   // search cả Name + Code
        public ProjectRequestStatusEnum? Status { get; set; }    // Pending, Accepted, Rejected

        // 🔀 Vai trò xem
        public ProjectRequestViewMode ViewMode { get; set; }

        // 📅 Date filters (gom vào DateRange)
        public DateRange<DateOnly>? StartDate { get; set; }
        public DateRange<DateOnly>? EndDate { get; set; }

        // ↕️ Sorting
        public string SortField { get; set; } = "CreateAt";   // Name, Code, StartDate, EndDate, CreateAt
        public string SortDirection { get; set; } = "desc";   // asc | desc

    }

    public class DateRange<T> where T : struct
    {
        public T? From { get; set; }
        public T? To { get; set; }
    }
}
