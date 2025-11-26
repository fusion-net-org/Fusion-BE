

namespace Fusion.Service.ViewModels.TransactionPayment.Responses.Overview;

public class MonthlyRevenuePoint
{
    public int Month { get; set; }              // 1..12
    public decimal TotalAmount { get; set; }    // sum Amount
    public int TransactionCount { get; set; }   // optional: số transaction
}
public class TransactionMonthlyRevenueResponse
{
    public int Year { get; set; }
    public List<MonthlyRevenuePoint> Items { get; set; } = new();
}
