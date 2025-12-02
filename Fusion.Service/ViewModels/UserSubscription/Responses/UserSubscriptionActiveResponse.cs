

using Fusion.Service.ViewModels.CompanySubscription.Responses;

namespace Fusion.Service.ViewModels.UserSubscription.Responses
{
    public class UserSubscriptionActiveResponse
    {
        public Guid Id { get; set; }
        public string? NameSubscription { get; set; }
        public List<UserSubscriptionEntitlementDropdownResponse> UserSubscriptionEntitlements { get; set; } = new();
    }

    public class UserSubscriptionEntitlementDropdownResponse
    {
        public Guid Id { get; set; }
        public Guid FeatureId { get; set; }
        public string FeatureName { get; set; } = string.Empty;
        public int? MonthlyLimit { get; set; }
    }
}
