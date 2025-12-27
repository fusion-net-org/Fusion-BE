

namespace Fusion.Repository.Bases.Page.TransactionPayment;

public class TransactionPaymentUserPagedRequest : PagedRequest
{
    public string? Status { get; set; }
    public string? Keyword { get; set; }
    public DateRange<DateTimeOffset> TransactionAt { get; set; } = new();
}
