using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fusion.Service.ViewModels.Task.Request
{
    public class TaskWorkflowAssignmentItemRequest
    {
        /// <summary>Status (trong workflow của Project / Task) mà ta muốn gán người</summary>
        public Guid WorkflowStatusId { get; set; }

        /// <summary>User được assign cho status đó. Null = unassign</summary>
        public Guid? AssignUserId { get; set; }
    }

    public class TaskWorkflowAssignmentsRequest
    {
        /// <summary>Task cần cấu hình workflow assignee</summary>
        public Guid TaskId { get; set; }

        /// <summary>Danh sách (status → user)</summary>
        public List<TaskWorkflowAssignmentItemRequest> Items { get; set; } = new();
    }
}
