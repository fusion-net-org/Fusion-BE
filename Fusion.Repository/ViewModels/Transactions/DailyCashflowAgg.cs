

namespace Fusion.Repository.ViewModels.Transactions;

public class DailyCashflowAgg
{
    public DateOnly Date { get; set; }
    public decimal Revenue { get; set; }
    public int SuccessCount { get; set; }
}
