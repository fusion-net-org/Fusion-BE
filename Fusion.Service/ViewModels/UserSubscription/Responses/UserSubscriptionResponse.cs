
namespace Fusion.Service.ViewModels.UserSubscription.Responses
{
    public class UserSubscriptionResponse
    {
        public string PackageName { get; set; } = string.Empty;
        public int QuotaCompanyRemaining { get; set; }
        public int QuotaProjectRemaining { get; set; }
        public DateTime PurchaseDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public bool IsActive { get; set; }
    }
}
