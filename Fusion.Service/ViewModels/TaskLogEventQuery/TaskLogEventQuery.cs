using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fusion.Service.ViewModels.TaskLogEventQuery
{

    public sealed class TaskLogEventQuery
    {
        public string? Keyword { get; set; }
        public DateOnly? From { get; set; }
        public DateOnly? To { get; set; }

        public string? Action { get; set; }
        public Guid? ActorId { get; set; }

        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;

        public string? SortColumn { get; set; }
        public bool SortDescending { get; set; } = true;
    }
}
