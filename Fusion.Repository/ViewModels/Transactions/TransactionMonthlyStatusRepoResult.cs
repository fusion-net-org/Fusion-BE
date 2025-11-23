

namespace Fusion.Repository.ViewModels.Transactions;

public class TransactionMonthlyStatusRepoItem
{
    public int Month { get; set; }
    public int SuccessCount { get; set; }
    public int PendingCount { get; set; }
    public int FailedCount { get; set; }
}
public class TransactionMonthlyStatusRepoResult
{
    public int Year { get; set; }
    public List<TransactionMonthlyStatusRepoItem> Items { get; set; } = new();
}