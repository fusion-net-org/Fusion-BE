

namespace Fusion.Service.ViewModels.CompanySubscription.Responses;

public class CompanySubscriptionActiveResponse
{
    public Guid Id { get; set; }
    public string? NameSubscription { get; set; }
    public int? SeatsLimitSnapshot { get; set; }
    public int? SeatsLimitUnit { get; set; }
    public List<CompanySubscriptionEntitlementDropdownResponse> CompanySubscriptionEntitlements { get; set; } = new();
}

public class CompanySubscriptionEntitlementDropdownResponse
{
    public Guid Id { get; set; }
    public string FeatureName { get; set; } = string.Empty;
}