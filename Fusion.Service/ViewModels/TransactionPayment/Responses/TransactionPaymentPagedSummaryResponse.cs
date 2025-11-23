
namespace Fusion.Service.ViewModels.TransactionPayment.Responses;

public class TransactionPaymentPagedSummaryResponse
{
    public List<TransactionPaymentResponse> Items { get; set; } = new();
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }

    // Phần thống kê tổng (theo toàn bộ tập sau khi filter, không chỉ riêng page)
    public int TotalTransactions { get; set; }   // = TotalCount (cho dễ bind UI)
    public decimal TotalRevenue { get; set; }    // sum Amount các transaction SUCCESS & CHARGE

    public int TotalSuccess { get; set; }
    public int TotalFailed { get; set; }
    public int TotalPending { get; set; }
}

