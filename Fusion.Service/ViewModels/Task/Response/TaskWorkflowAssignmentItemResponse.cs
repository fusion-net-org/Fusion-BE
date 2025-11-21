using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fusion.Service.ViewModels.Task.Response
{
    public class TaskWorkflowAssignmentItemResponse
    {
        public Guid WorkflowStatusId { get; set; }
        public string StatusCode { get; set; } = "";
        public string StatusName { get; set; } = "";
        public string Category { get; set; } = ""; // TODO / IN_PROGRESS / REVIEW / DONE
        public int Position { get; set; }

        public Guid? AssignUserId { get; set; }
        public string? AssignUserName { get; set; }
        public string? AssignUserEmail { get; set; }
        public string? AssignUserAvatarUrl { get; set; }
    }

    public class TaskWorkflowAssignmentsResponse
    {
        public Guid TaskId { get; set; }
        public Guid WorkflowId { get; set; }
        public List<TaskWorkflowAssignmentItemResponse> Items { get; set; } = new();
    }
}
