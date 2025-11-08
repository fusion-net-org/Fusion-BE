


using Fusion.Repository.Enums;

namespace Fusion.Service.ViewModels.UserSubscription.Responses;

public class UserSubscriptionDetailResponse
{
    public Guid Id { get; set; }
    public string? NamePlan { get; set; }
    public decimal Price { get; set; }
    public string? Currency { get; set; }
    public SubscriptionStatus Status { get; set; }
    public DateTime CreatAt { get; set; }
    public DateTime ExpiredAt { get; set; }
    public DateTime? UpdateAt { get; set; }

    public List<UserSubscriptionEntitlementResponse> Entitlements { get; set; } = new();
}
public class UserSubscriptionEntitlementResponse
{
    public Guid Id { get; set; }
    public string FeatureKey { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public int Remaining { get; set; }
}