

namespace Fusion.Service.ViewModels.TransactionPayment.Responses;

public sealed class PaymentLinkInfoResponse
{
    public string CheckoutUrl { get; set; } = default!;
    public long? OrderCode { get; set; }
    public string? PaymentLinkId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "VND";
    public string Description { get; set; } = default!;      // Nội dung chuyển khoản (addInfo)
    public string AccountNumber { get; set; } = default!;
    public string AccountName { get; set; } = default!;
    public string BankName { get; set; } = default!;
    public string? QrImageBase64 { get; set; }               // nếu SDK trả sẵn base64
    public string? QrImageUrl { get; set; }
}
