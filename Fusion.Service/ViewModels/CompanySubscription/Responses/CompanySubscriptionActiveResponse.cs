
using Fusion.Repository.Enums;

namespace Fusion.Service.ViewModels.CompanySubscription.Responses;

public class CompanySubscriptionActiveResponse
{
    public Guid Id { get; set; }
    public string? NameSubscription { get; set; }
    public string? Status { get; set; }
    public DateTime ExpiredAt { get; set; }
    public List<CompanySubscriptionEntitlementDropdownResponse> CompanySubscriptionEntitlements { get; set; } = new();
}

public class CompanySubscriptionEntitlementDropdownResponse
{
    public Guid Id { get; set; }
    public FeatureKeys FeatureKey { get; set; }

    public int Quantity { get; set; }

    public int Remaining { get; set; }
}