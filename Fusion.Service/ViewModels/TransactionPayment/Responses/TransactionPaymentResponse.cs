
namespace Fusion.Service.ViewModels.TransactionPayment.Responses
{
    public record TransactionPaymentResponse
    {
        public Guid id { get; set; }
        public string CustomerName { get; set; }
        public string PackageName { get; set; }
        public string TransactionCode {  get; set; }
        public decimal Amount { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
