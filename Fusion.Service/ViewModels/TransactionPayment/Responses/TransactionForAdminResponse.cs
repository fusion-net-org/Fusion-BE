
namespace Fusion.Service.ViewModels.TransactionPayment.Responses
{
    public class TransactionForAdminResponse
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string UserName { get; set; }
        public Guid PackageId { get; set; }
        public string PackageName { get; set; }
        public string TransactionCode { get; set; } = null!;
        public decimal Amount { get; set; }
        public string? PaymentMethod { get; set; }
        public string? Status { get; set; } 
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
