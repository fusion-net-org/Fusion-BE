

namespace Fusion.Service.ViewModels.TransactionPayment.Requests;

public class TransactionPaymentUpdateRequest
{
    public long? OrderCode { get; set; }
    public string? PaymentLinkId { get; set; }

    // giữ nguyên/tuỳ chọn các field hiện có
    public decimal? Amount { get; set; }               // thường không đổi ở flow này
    public string? Description { get; set; }           // <= 25 ký tự (PayOS)
    public string? AccountNumber { get; set; }
    public string? Reference { get; set; }
    public DateTimeOffset? TransactionDateTime { get; set; }
    public string? Currency { get; set; }
    public string? CounterAccountBankId { get; set; }
    public string? CounterAccountBankName { get; set; }
    public string? CounterAccountName { get; set; }
    public string? CounterAccountNumber { get; set; }
    public string? PaymentMethod { get; set; }
    public string? Status { get; set; }
}
