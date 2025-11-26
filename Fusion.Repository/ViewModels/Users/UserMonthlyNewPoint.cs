

namespace Fusion.Repository.ViewModels.Users;

public class UserMonthlyNewPoint
{
    public int Year { get; set; }
    public int Month { get; set; }      // 1..12
    public int NewUsers { get; set; }
}
