

namespace Fusion.Service.ViewModels.TransactionPayment.Responses;

public class TransactionPaymentDetailResponse
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid PlanId { get; set; }
    public long? OrderCode { get; set; }
    public string? PaymentLinkId { get; set; }
    public decimal Amount { get; set; }
    public string? Description { get; set; }
    public string? AccountNumber { get; set; }
    public string? Reference { get; set; }
    public DateTimeOffset? TransactionDateTime { get; set; }
    public string? Currency { get; set; } = "VND";
    public string? CounterAccountBankId { get; set; }
    public string? CounterAccountBankName { get; set; }
    public string? CounterAccountName { get; set; }
    public string? CounterAccountNumber { get; set; }
    public string? PaymentMethod { get; set; }
    public string Status { get; set; }
}
