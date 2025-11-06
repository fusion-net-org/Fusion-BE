

namespace Fusion.Repository.Bases.Page.TransactionPayment
{
    public class TransactionPaymentPagedRequest : PagedRequest
    {
        public string? UserName { get; set; }   // tên user
        public string? PlanName { get; set; }   // tên gói
        public string? Status { get; set; }     // Pending/Success/Failed/Refunded/Cancelled
        public string? Keyword { get; set; }    // tìm trong Reference/Description/PaymentLinkId/OrderCode

        // khoảng thời gian giao dịch (theo TransactionDateTime = datetimeoffset)
        public DateRange<DateTimeOffset> TransactionAt { get; set; } = new();

    }
}
