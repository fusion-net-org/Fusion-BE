

namespace Fusion.Repository.Bases.Page.TransactionPayment
{
    public class TransactionPaymentPagedRequest : PagedRequest
    {
        public string? UserName { get; set; } 
        public string? PlanName { get; set; }   
        public string? Status { get; set; }     
        public string? Keyword { get; set; }   
        public DateRange<DateTimeOffset> TransactionAt { get; set; } = new();

    }
}
