

namespace Fusion.Service.ViewModels.TransactionPayment.Responses.Overview;

public class TransactionMonthlyStatusItemResponse
{
    public int Month { get; set; }
    public int SuccessCount { get; set; }
    public int PendingCount { get; set; }
    public int FailedCount { get; set; }
}
public class TransactionMonthlyStatusResponse
{
    public int Year { get; set; }
    public List<TransactionMonthlyStatusItemResponse> Items { get; set; } = new();
}