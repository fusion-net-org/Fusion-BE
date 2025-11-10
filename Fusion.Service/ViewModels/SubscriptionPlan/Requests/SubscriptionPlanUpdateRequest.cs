
namespace Fusion.Service.ViewModels.SubscriptionPlan.Requests;

public class SubscriptionPlanUpdateRequest
{
    public Guid Id { get; set; }
    public string Code { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public bool IsActive { get; set; }

    public List<SubscriptionPlanFeatureRequest>? Features { get; set; }
    public SubscriptionPlanPriceRequest Price { get; set; }
}
