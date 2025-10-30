

namespace Fusion.Repository.Bases.Page.TransactionPayment
{
    public class AdminTransactionSearch : PagedRequest
    {
        public string? TransactionCode { get; set; }
        public string? PackageName { get; set; }
        public DateTime? PaymentDateFrom { get; set; }
        public DateTime? PaymentDateTo { get; set; }
        public decimal? AmountMin { get; set; }
        public decimal? AmountMax { get; set; }
        public string? Status { get; set; } 


    }
}
