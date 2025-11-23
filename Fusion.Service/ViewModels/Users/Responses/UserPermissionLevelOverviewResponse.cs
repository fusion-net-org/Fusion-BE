
namespace Fusion.Service.ViewModels.Users.Responses;

public class UserPermissionLevelPointResponse
{
    public string Level { get; set; } = string.Empty;
    public int Count { get; set; }
}

public class UserPermissionLevelOverviewResponse
{
    public int TotalUsers { get; set; }
    public List<UserPermissionLevelPointResponse> Levels { get; set; } = new();
}
