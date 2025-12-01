
namespace Fusion.Repository.ViewModels.SubscriptionPlan;

public sealed class PlanMonthlyPurchaseCountRow
{
    public Guid PlanId { get; set; }
    public string PlanName { get; set; } = string.Empty;

    public int Year { get; set; }
    public int Month { get; set; }

    public int PurchaseCount { get; set; }
    public decimal TotalAmount { get; set; }
}
