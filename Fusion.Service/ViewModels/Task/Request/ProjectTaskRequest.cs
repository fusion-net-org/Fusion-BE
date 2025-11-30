// Fusion.Service/ViewModels/Task/Request/ProjectTaskRequest.cs
using System.ComponentModel.DataAnnotations;

namespace Fusion.Service.ViewModels.Task.Request;

public class ProjectTaskRequest
{
    public Guid Id { get; set; }                   // dùng cho update
    [Required] public Guid ProjectId { get; set; } // vì route /api/tasks, ProjectId nhận từ body
    public Guid? SprintId { get; set; }            // null => Backlog

    [Required, MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }
    public string? Type { get; set; } = "Feature";           // Feature/Bug/Chore…
    public string? Priority { get; set; } = "Medium";        // Urgent/High/Medium/Low
    public string? Severity { get; set; }                    // Critical/High/Medium/Low

    public int? Point { get; set; }                          // story points
    public int? EstimateHours { get; set; }
    public DateTime? DueDate { get; set; }

    public Guid? WorkflowStatusId { get; set; }              // nếu null → pick status đầu (TODO)
    public string? StatusCode { get; set; }                  // fallback nếu bạn gửi code thay vì Guid

    public Guid? ParentTaskId { get; set; }
    public Guid? SourceTaskId { get; set; }
    public List<Guid>? AssigneeIds { get; set; }             // nhiều ngườ
    public List<TaskWorkflowAssignmentItemRequest>? WorkflowAssignments { get; set; }
    public Guid? TicketId { get; set; }

}
