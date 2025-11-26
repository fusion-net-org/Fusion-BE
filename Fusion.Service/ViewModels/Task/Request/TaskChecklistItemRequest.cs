using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fusion.Service.ViewModels.Task.Request
{
    public class TaskChecklistItemCreateRequest
    {
        public Guid TaskId { get; set; }
        public string Label { get; set; } = string.Empty;
    }

    public class TaskChecklistItemUpdateRequest
    {
        public Guid Id { get; set; }
        public string Label { get; set; } = string.Empty;
        public bool IsDone { get; set; }
        public int? OrderIndex { get; set; }
    }

    public class ToggleChecklistItemRequest
    {
        public bool? IsDone { get; set; }   // null => toggle
    }
}
