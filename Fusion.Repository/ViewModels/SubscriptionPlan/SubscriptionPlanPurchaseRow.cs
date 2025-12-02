

namespace Fusion.Repository.ViewModels.SubscriptionPlan;

public sealed class SubscriptionPlanPurchaseRow
{
    public Guid PlanId { get; set; }
    public string? PlanName { get; set; }
    public int PurchaseCount { get; set; }
    public decimal TotalAmount { get; set; }
}
