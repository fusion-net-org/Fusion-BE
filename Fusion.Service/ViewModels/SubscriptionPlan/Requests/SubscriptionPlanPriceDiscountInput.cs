
namespace Fusion.Service.ViewModels.SubscriptionPlan.Requests;

public class SubscriptionPlanPriceDiscountInput
{
    /// <summary>
    /// Kỳ thứ mấy (1 = năm 1 / kỳ 1, 2 = năm 2 / kỳ 2, ...)
    /// </summary>
    public int InstallmentIndex { get; set; }
    /// <summary>
    /// Phần trăm giảm giá, 10 = 10%, 20 = 20%
    /// </summary>
    public decimal DiscountValue { get; set; }

    public string? Note { get; set; }
}
