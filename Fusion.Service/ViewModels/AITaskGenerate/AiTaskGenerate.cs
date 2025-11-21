using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fusion.Service.ViewModels.AITaskGenerate
{
    public sealed class AiExistingTaskSnapshotDto
    {
        public Guid Id { get; set; }
        public string? Code { get; set; }
        public string Title { get; set; } = default!;
        public string Type { get; set; } = "Feature";
        public string? Module { get; set; }
        public string StatusCategory { get; set; } = "TODO"; // map StatusCategory
        public string? Priority { get; set; }
        public string? Severity { get; set; }
        public int? EstimateHours { get; set; }
        public int? StoryPoints { get; set; }
    }

    public sealed class AiWorkflowStatusDto
    {
        public Guid Id { get; set; }
        public string Code { get; set; } = default!;
        public string Name { get; set; } = default!;
        public string Category { get; set; } = "TODO";
        public bool IsDone { get; set; }
        public int Order { get; set; }
    }

    public sealed class AiTaskGenerateRequestDto
    {
        public Guid ProjectId { get; set; }
        public string ProjectName { get; set; } = default!;

        public Guid SprintId { get; set; }
        public string SprintName { get; set; } = default!;
        public DateTime? SprintStart { get; set; }
        public DateTime? SprintEnd { get; set; }
        public int? SprintCapacityHours { get; set; }

        public List<AiWorkflowStatusDto> WorkflowStatuses { get; set; } = new();
        public Guid DefaultStatusId { get; set; }

        public string Goal { get; set; } = default!;
        public string? Context { get; set; }

        public List<string> WorkTypes { get; set; } = new();
        public List<string> Modules { get; set; } = new();

        public int Quantity { get; set; }
        public string Granularity { get; set; } = "Task";

        public string EstimateUnit { get; set; } = "Hours"; // or StoryPoints
        public bool WithEstimate { get; set; } = true;
        public int? EstimateMin { get; set; }
        public int? EstimateMax { get; set; }
        public int? TotalEffortHours { get; set; }

        public DateTime? Deadline { get; set; }

        public int TeamMemberCount { get; set; }
        public List<string> TeamRoles { get; set; } = new();
        public List<string> TechStack { get; set; } = new();

        public string? FunctionalRequirements { get; set; }
        public string? NonFunctionalRequirements { get; set; }
        public string? AcceptanceHint { get; set; }

        public bool IncludeTitle { get; set; } = true;
        public bool IncludeDescription { get; set; } = true;
        public bool IncludeType { get; set; } = true;
        public bool IncludePriority { get; set; } = true;
        public bool IncludeEstimate { get; set; } = true;
        public bool IncludeAcceptanceCriteria { get; set; } = true;
        public bool IncludeDependencies { get; set; } = true;
        public bool IncludeStatusSuggestion { get; set; } = true;
        public bool IncludeChecklist { get; set; } = true;

        public bool IncludeExistingTasks { get; set; } = true;
        public bool AvoidSameTitle { get; set; } = true;
        public bool AvoidSameDescription { get; set; } = true;

        public List<AiExistingTaskSnapshotDto>? ExistingTasksSnapshot { get; set; }
        public IReadOnlyList<AiBoardSprintDto>? BoardSprints { get; set; }
        public IReadOnlyList<AiBoardTaskSnapshotDto>? BoardTasks { get; set; }
    }
    public sealed class AiBoardSprintDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = default!;
        public DateTime? Start { get; set; }
        public DateTime? End { get; set; }
        public string? State { get; set; }
        public int? CapacityHours { get; set; }
        public int? CommittedPoints { get; set; }
    }

    public sealed class AiBoardTaskSnapshotDto
    {
        public Guid Id { get; set; }
        public Guid? SprintId { get; set; }
        public string? SprintName { get; set; }

        public string? Code { get; set; }
        public string Title { get; set; } = default!;
        public string Type { get; set; } = "Feature";
        public string Priority { get; set; } = "Medium";
        public string? Severity { get; set; }

        public string? StatusCode { get; set; }
        public string? StatusCategory { get; set; }

        public int? EstimateHours { get; set; }
        public int? StoryPoints { get; set; }
    }

    public sealed class AiGeneratedTaskDraftDto
    {
        public string Title { get; set; } = default!;
        public string? Description { get; set; }
        public string Type { get; set; } = "Feature";
        public string Priority { get; set; } = "Medium";
        public string? Severity { get; set; }

        public string? StatusCategory { get; set; }
        public string? StatusCode { get; set; }

        public int? EstimateHours { get; set; }
        public int? StoryPoints { get; set; }

        public DateTime? DueDate { get; set; }
        public string? Module { get; set; }
        public Guid? SprintId { get; set; }

        public string? AcceptanceCriteria { get; set; }
        public List<string>? Checklist { get; set; }

        public List<string>? DependsOnCodes { get; set; }
        public List<string>? DependsOnTitles { get; set; }
    }

    public sealed class AiGenerateTasksResponseDto
    {
        public List<AiGeneratedTaskDraftDto> Tasks { get; set; } = new();
    }

}
