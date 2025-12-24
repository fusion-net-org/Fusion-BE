using Fusion.Repository.Bases.Page.CompanyActivityLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fusion.Repository.Bases.Page.TaskLogEvent
{
    public sealed class TaskLogEventPagedSearchRequest : PagedRequest
    {
        public string? KeyWord { get; set; }
        public DateRangeRequest? DateRange { get; set; }

        // optional filters
        public string? Action { get; set; }
        public Guid? ActorId { get; set; }
    }
}
