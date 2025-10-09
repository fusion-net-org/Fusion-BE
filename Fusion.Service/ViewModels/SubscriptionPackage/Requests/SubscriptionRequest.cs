

namespace Fusion.Service.ViewModels.SubscriptionPackage.Requests
{
    public class SubscriptionRequest
    {
        public string Name { get; set; } = null!;
        public decimal Price { get; set; }
        public int QuotaCompany { get; set; }
        public int QuotaProject { get; set; }
        public string Description { get; set; } = null!;
    }
}
