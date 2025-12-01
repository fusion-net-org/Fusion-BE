

namespace Fusion.Service.ViewModels.SubscriptionPlan.Responses;

public sealed class SubscriptionPlanPurchaseStatResponse
{
    public Guid PlanId { get; set; }
    public string PlanName { get; set; } = string.Empty;

    /// <summary> Tổng số lần gói này được mua (transaction thành công). </summary>
    public int PurchaseCount { get; set; }

    /// <summary> Tổng tiền đã thu từ gói này. </summary>
    public decimal TotalAmount { get; set; }

    /// <summary> % so với tổng số lần mua (để FE vẽ chart). </summary>
    public decimal Percentage { get; set; }
}
