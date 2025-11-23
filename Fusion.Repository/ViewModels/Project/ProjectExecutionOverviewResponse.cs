
namespace Fusion.Repository.ViewModels.Project;

public class TaskFlowPointResponse
{
    public int Year { get; set; }
    public int Month { get; set; } // 1-12

    public int CreatedTasks { get; set; }
    public int CompletedTasks { get; set; }
}

public class SprintVelocityPointResponse
{
    public Guid SprintId { get; set; }
    public string? SprintName { get; set; }

    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }

    public int CommittedPoints { get; set; }
    public int CompletedPoints { get; set; }
}

public class ProjectExecutionOverviewResponse
{
    // Task stats
    public int TotalTasks { get; set; }
    public int CompletedTasks { get; set; }
    public int OverdueTasks { get; set; }

    // Sprint stats
    public int TotalSprints { get; set; }
    public int ActiveSprints { get; set; }
    public int CompletedSprints { get; set; }

    // Time series
    public List<TaskFlowPointResponse> TaskFlow { get; set; } = new();
    public List<SprintVelocityPointResponse> SprintVelocity { get; set; } = new();
}
