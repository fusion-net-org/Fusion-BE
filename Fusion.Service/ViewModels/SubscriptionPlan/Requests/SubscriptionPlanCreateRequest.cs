
using Fusion.Repository.Enums;
using System.ComponentModel.DataAnnotations;

namespace Fusion.Service.ViewModels.SubscriptionPlan.Requests;

public class SubscriptionPlanFeatureRequest
{
    [Required]
    public FeatureKeys FeatureKey { get; set; } = FeatureKeys.Project;
    public int? LimitValue { get; set; }
}

public class SubscriptionPlanPriceRequest
{
    public BillingPriod BillingPeriod { get; set; } // Week/Month/Year
    public int PeriodCount { get; set; } = 1;        // >= 1
    public decimal Price { get; set; }               // >= 0
    public string Currency { get; set; } = "VND";    // ISO 4217, 3-letter, UPPER
    public int RefundWindowDays { get; set; }        // >= 0
    public decimal RefundFeePercent { get; set; }    // [0..100]
}
public class SubscriptionPlanCreateRequest
{
    [Required]
    public string Code { get; set; } = null!;
    [Required]
    public string Name { get; set; } = null!;
    public string? Description { get; set; }

    public List<SubscriptionPlanFeatureRequest>? Features { get; set; }

    [Required]
    public SubscriptionPlanPriceRequest Price { get; set; } = new();
}
