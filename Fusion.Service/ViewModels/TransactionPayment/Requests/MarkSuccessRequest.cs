

namespace Fusion.Service.ViewModels.TransactionPayment.Requests;

public class MarkSuccessRequest
{
    public decimal? Amount { get; set; }
    public DateTimeOffset PaidAt { get; set; }
    public string? Reference { get; set; }
}
