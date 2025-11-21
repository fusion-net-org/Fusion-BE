// Fusion.Repository.Bases.Page.ProjectBoard/BoardDtos.cs
using System.ComponentModel.DataAnnotations;

namespace Fusion.Repository.Bases.Page.ProjectBoard;

public record MemberRefDto(string Id, string Name, string? AvatarUrl);

public record StatusMetaDto
{
    [Required] public Guid Id { get; init; }
    [Required] public string Code { get; init; } = "";            // "todo" | "inprogress" | ...
    [Required] public string Name { get; init; } = "";
    [Required] public string Category { get; init; } = "TODO";     // TODO | IN_PROGRESS | REVIEW | DONE
    public int Order { get; init; }
    public int? WipLimit { get; init; } = null;                    // optional theo nghiệp vụ
    public string? Color { get; init; }
    public bool IsFinal { get; init; }
    public bool IsStart { get; init; }
    public IReadOnlyList<string> Roles { get; init; } = Array.Empty<string>();

}

public record SprintVmDto
{
    [Required] public Guid Id { get; init; }
    [Required] public string Name { get; init; } = "";
    public DateTime? Start { get; init; }
    public DateTime? End { get; init; }
    public string State { get; init; } = "Planning";               // Planning | Active | Closed
    public int? CapacityHours { get; init; }
    public int? CommittedPoints { get; init; }

    public Guid? WorkflowId { get; init; }
    [Required] public List<Guid> StatusOrder { get; init; } = new();
    [Required] public Dictionary<Guid, StatusMetaDto> StatusMeta { get; init; } = new();
}

public record TaskVmDto
{
    [Required] public Guid Id { get; init; }
    public string Code { get; init; } = "";
    public string Title { get; init; } = "";
    public string Type { get; init; } = "Task";
    public string Priority { get; init; } = "Medium";
    public string? Severity { get; init; }
    public int? StoryPoints { get; init; }
    public int? EstimateHours { get; init; }
    public int? RemainingHours { get; init; }
    public DateTimeOffset? DueDate { get; init; }

    public Guid? SprintId { get; init; }
    [Required] public Guid WorkflowStatusId { get; init; }
    public string StatusCode { get; init; } = "todo";
    public string StatusCategory { get; init; } = "TODO";

    public List<MemberRefDto> Assignees { get; init; } = new();
    public string? StatusName { get; init; }
    public List<Guid> DependsOn { get; init; } = new();
    public Guid? ParentTaskId { get; init; }
    public int CarryOverCount { get; init; }

    public DateTime OpenedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
    public DateTime CreatedAt { get; init; }

    public Guid? SourceTicketId { get; init; }
    public string? SourceTicketCode { get; init; }
}

public record MultiSprintBoardResponseDto
{
    public List<SprintVmDto> Sprints { get; init; } = new();
    public List<TaskVmDto> Tasks { get; init; } = new();
}
