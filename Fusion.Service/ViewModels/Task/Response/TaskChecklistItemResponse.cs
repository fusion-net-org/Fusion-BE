using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fusion.Service.ViewModels.Task.Response
{
    public class TaskChecklistItemResponse
    {
        public Guid Id { get; set; }
        public Guid TaskId { get; set; }
        public string Label { get; set; } = string.Empty;
        public bool IsDone { get; set; }
        public int OrderIndex { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
