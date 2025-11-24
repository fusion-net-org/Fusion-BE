

namespace Fusion.Service.ViewModels.Users.Responses
{
    public class UserGrowthPointResponse
    {
        public string Period { get; set; } = default!;

        public int NewUsers { get; set; }
    }
    public class UserGrowthAndStatusOverviewResponse
    {
        public List<UserGrowthPointResponse> Growth { get; set; } = new();

        public int TotalUsers { get; set; }

        public int ActiveUsers { get; set; }

        public int InactiveUsers { get; set; }
    }
}
