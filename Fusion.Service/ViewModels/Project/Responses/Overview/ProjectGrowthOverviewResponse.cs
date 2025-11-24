
namespace Fusion.Service.ViewModels.Project.Responses.Overview;

public class ProjectGrowthPointResponse
{
    public int Year { get; set; }
    public int Month { get; set; } 

    public int NewProjects { get; set; }
    public int CompletedProjects { get; set; }

    public int CumulativeProjects { get; set; }
}

public class ProjectGrowthOverviewResponse
{
    public int TotalProjects { get; set; }
    public int ActiveProjects { get; set; }
    public int CompletedProjects { get; set; }
    public int NewProjectsLast30Days { get; set; }

    public List<ProjectGrowthPointResponse> Growth { get; set; } = new();
}
