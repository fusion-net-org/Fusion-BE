

using Fusion.Service.ViewModels.SubscriptionPlan.Requests;

namespace Fusion.Service.ViewModels.SubscriptionPlan.Responses;

public class SubscriptionPlanResponse
{
    public Guid Id { get; set; }
    public string Code { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public bool IsActive { get; set; }

    public IEnumerable<SubscriptionPlanFeatureRequest>? Features { get; set; }
    public SubscriptionPlanPriceRequest? Price { get; set; }
}
