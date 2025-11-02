

namespace Fusion.Service.ViewModels.TransactionPayment.Responses;

public class MonthlyRevenueItem
{
    public int Month { get; set; } // 1 -> 12
    public decimal Revenue { get; set; } // total each moth
}

public class YearlyRevenueResponse
{
    public int Year { get; set; }
    public decimal TotalRevenue { get; set; }
    public List<MonthlyRevenueItem> Moths { get; set; } = new();
} 


