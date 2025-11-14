

namespace Fusion.Service.ViewModels.TransactionPayment.Requests;

public class AttachPaymentLinkRequest
{
    public long OrderCode { get; set; }
    public string PaymentLinkId { get; set; } = string.Empty;
    public string? Provider { get; set; }
}
