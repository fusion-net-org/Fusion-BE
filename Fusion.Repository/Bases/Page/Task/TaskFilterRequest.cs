using Fusion.Repository.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fusion.Repository.Bases.Page.Task
{
    public class TaskFilterRequest: PagedRequest
    {
        public DateRange<DateOnly>? DateRange { get; set; }

        public TaskEnum? Type { get; set; }

        public TaskPriorityEnum? Priority { get; set; }

        public string? Keyword { get; set; }

        public Guid? ProjectId { get; set; }

        public Guid? SprintId { get; set; }

        public Guid? StatusId { get; set; }

        public bool? OverDue {  get; set; }

    }
}
