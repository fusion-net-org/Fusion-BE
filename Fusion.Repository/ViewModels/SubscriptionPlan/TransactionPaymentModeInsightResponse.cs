

using Fusion.Repository.Enums;

namespace Fusion.Repository.ViewModels.SubscriptionPlan;

public class TransactionPaymentModeInsightItemDto
{
    public PaymentMode PaymentMode { get; set; }  // "Prepaid", "Installments", ...
    public int TransactionCount { get; set; }
    public int SuccessCount { get; set; }
    public int PendingCount { get; set; }
    public int FailedCount { get; set; }
    public decimal TotalAmount { get; set; }  // chỉ tính success
}

public class TransactionPaymentModeInsightResponse
{
    public int Year { get; set; }
    public IReadOnlyList<TransactionPaymentModeInsightItemDto> Items { get; set; }
        = Array.Empty<TransactionPaymentModeInsightItemDto>();
}
