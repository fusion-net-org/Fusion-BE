

namespace Fusion.Service.ViewModels.SubscriptionPlan.Responses;

public class SubscriptionPlanMonthlyPurchaseItemDto
{
    public Guid PlanId { get; set; }
    public string PlanName { get; set; } = string.Empty;
    public int Month { get; set; }
    public int PurchaseCount { get; set; }
}

public class SubscriptionPlanMonthlyPurchaseResponse
{
    public int Year { get; set; }
    public List<SubscriptionPlanMonthlyPurchaseItemDto> Items { get; set; } = new();
}
