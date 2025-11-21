
namespace Fusion.Service.ViewModels.SubscriptionPlan.Responses;

public class SubscriptionPlanPriceDiscountResponse
{
    public int InstallmentIndex { get; set; }
    public decimal DiscountValue { get; set; }
    public string? Note { get; set; }
}
