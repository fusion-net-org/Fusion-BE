using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fusion.Repository.Bases.Page.ProjectBoard
{
    public class ProjectTaskListQuery
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 25;

        public string? Search { get; set; }

        public string? StatusCategory { get; set; }

        public List<Guid> AssigneeIds { get; set; } = new();

        public Guid? SprintId { get; set; }
        public bool OnlyActiveSprints { get; set; } = true;

        public string? Priority { get; set; }  
        public string? Severity { get; set; }  

        public string? Tag { get; set; }

        public DateOnly? DueFrom { get; set; }
        public DateOnly? DueTo { get; set; }

        public string SortBy { get; set; } = "updatedDesc";
    }

}
