using Fusion.Service.ViewModels.Comment.Response;
using Fusion.Service.ViewModels.Project.Responses;
using Fusion.Service.ViewModels.ProjectMembers.Responses;
using Fusion.Service.ViewModels.Projects.Responses;
using Fusion.Service.ViewModels.Sprint.Responses;
using Fusion.Service.ViewModels.WorkflowStatus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fusion.Service.ViewModels.Task.Response
{
    public class TaskResponse
    {
        public Guid TaskId { get; set; }
        public string? Code { get; set; }
        public string? Title { get; set; }
        public string? Img { get; set; }
        public string? Type { get; set; }
        public string? Priority { get; set; }
        public string? Severity { get; set; }
        public string? Status { get; set; }
        public int? Point { get; set; }
        public int? EstimateHours { get; set; }
        public int? RemainingHours { get; set; }
        public int CarryOverCount { get; set; } = 0;
        public int? OrderInSprint { get; set; }

        public bool IsBacklog { get; set; }
        public DateTime? CreateAt { get; set; }
        public DateTime? DueDate { get; set; }

        public string? CreateByName { get; set; }
        public Guid? CreateBy { get; set; }

        public Guid? ParentTaskId { get; set; }
        public Guid? SourceTaskId { get; set; }

        public ProjectResponse? Project { get; set; }

        public SprintResponse? Sprint { get; set; }

        public WorkflowStatusResponse? WorkflowStatus { get; set; }


        public List<ProjectMemberSummaryResponse>? Members { get; set; }

        public List<TaskChecklistItemResponse>? Checklist { get; set; }

        public List<TaskDependencyResponse>? Dependencies { get; set; }

        public List<CommentResponse>? Comments { get; set; }


    }

    public class TaskDependencyResponse
    {
        public Guid TaskId { get; set; }
        public string? Title { get; set; }
        public string? Code { get; set; }
        public string? Priority { get; set; }
        public string? Status { get; set; }
        public int? Point { get; set; }
        public int? EstimateHours { get; set; }
    }
}
