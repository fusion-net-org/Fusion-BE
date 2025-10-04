using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fusion.Service.ViewModels.Task.Request
{
    public class ProjectTaskRequest
    {
        public Guid Id;
        public Guid? ProjectId { get; set; }

        public Guid? SprintId { get; set; }

        public string? Type { get; set; }

        public string? Img { get; set; }

        public string? Title { get; set; }

        public string? Description { get; set; }

        public string? Priority { get; set; }

        public bool IsBacklog { get; set; }

        public int? Point { get; set; }

        public string? Source { get; set; }

    }
}
