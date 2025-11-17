
namespace Fusion.Service.ViewModels.CompanySubscription.Responses;

public class CompanySubscriptionDetailResponse : CompanySubscriptionListResponse
{
    public Guid CompanyId { get; set; }
    public Guid UserSubscriptionId { get; set; }
    public Guid OwnerUserId { get; set; }
    public List<CompanySubscriptionEntitlementDetailResponse> Entitlements { get; set; } = new();
}
public class CompanySubscriptionEntitlementDetailResponse
{
    public Guid FeatureId { get; set; }
    public string? FeatureCode { get; set; }
    public string? FeatureName { get; set; }
    public bool Enabled { get; set; }
}

