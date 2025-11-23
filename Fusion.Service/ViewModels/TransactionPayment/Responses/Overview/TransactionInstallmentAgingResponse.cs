

namespace Fusion.Service.ViewModels.TransactionPayment.Responses.Overview
{
    public class TransactionInstallmentAgingItemResponse
    {
        public string BucketKey { get; set; } = default!;      
        public int InstallmentCount { get; set; }
        public decimal OutstandingAmount { get; set; }
    }
    public class TransactionInstallmentAgingResponse
    {
        public DateTimeOffset AsOf { get; set; }
        public List<TransactionInstallmentAgingItemResponse> Items { get; set; } = new();
        public int TotalInstallments { get; set; }
        public decimal TotalOutstandingAmount { get; set; }
    }
}
