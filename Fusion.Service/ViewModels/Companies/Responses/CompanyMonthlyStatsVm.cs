
namespace Fusion.Service.ViewModels.Companies.Responses;

public class CompanyMonthlyStatsVm
{
    public int Year { get; set; }
    public int[] MonthlyCounts { get; set; } = new int[12];
    public int Total { get; init; }
}
