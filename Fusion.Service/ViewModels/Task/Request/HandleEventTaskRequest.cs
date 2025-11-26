using Fusion.Service.ViewModels.Task.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fusion.Service.ViewModels.Task.Request
{
    public class ReorderTaskRequest
    {
        public Guid TaskId { get; set; }
        public Guid ToStatusId { get; set; }
        public int ToIndex { get; set; } // 0-based
    }

    public sealed class ChangeStatusRequest { public string? Status { get; set; } }

    public class TaskMoveRequest
    {
        public Guid ToSprintId { get; set; }
    }


    public class ChangeStatusByIdRequest
    {
        public Guid StatusId { get; set; }
    }


    public class SplitTaskResponse
    {
        public ProjectTaskResponse PartA { get; set; } = default!;
        public ProjectTaskResponse? PartB { get; set; }
    }
}
