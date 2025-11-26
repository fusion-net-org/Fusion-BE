

namespace Fusion.Service.ViewModels.TransactionPayment.Responses.Overview;

public class MonthlyRevenueThreeYearsPoint
{
    public int Month { get; set; }  // 1..12

    // Năm -2 (cách 2 năm)
    public decimal YearMinus2Amount { get; set; }
    public int YearMinus2TransactionCount { get; set; }

    // Năm -1 (năm trước)
    public decimal YearMinus1Amount { get; set; }
    public int YearMinus1TransactionCount { get; set; }

    // Năm hiện tại (base year)
    public decimal YearAmount { get; set; }
    public int YearTransactionCount { get; set; }
}

public class TransactionMonthlyRevenueThreeYearsResponse
{
    /// <summary>Base year (ví dụ 2025)</summary>
    public int Year { get; set; }

    /// <summary>Year - 1 (ví dụ 2024)</summary>
    public int YearMinus1 { get; set; }

    /// <summary>Year - 2 (ví dụ 2023)</summary>
    public int YearMinus2 { get; set; }

    public List<MonthlyRevenueThreeYearsPoint> Items { get; set; } = new();
}
