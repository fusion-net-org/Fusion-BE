
namespace Fusion.Service.ViewModels.TransactionPayment.Responses;

public class TransactionPaymentResponse
{
    public Guid Id { get; set; }
    public long? OrderCode { get; set; }
    public string? PaymentLinkId { get; set; }
    public decimal Amount { get; set; }
    public string? Currency { get; set; } = "VND";
    public DateTimeOffset? TransactionDateTime { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? UserName { get; set; }
    public string? PlanName { get; set; }
}
