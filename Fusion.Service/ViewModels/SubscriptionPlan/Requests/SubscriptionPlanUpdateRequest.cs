
using Fusion.Repository.Enums;
using System.ComponentModel.DataAnnotations;

namespace Fusion.Service.ViewModels.SubscriptionPlan.Requests;

public class SubscriptionPlanUpdateRequest
{

    [Required]
    public Guid Id { get; set; }

    [Required, MaxLength(50)]
    public string Code { get; set; } = null!;

    [Required, MaxLength(200)]
    public string Name { get; set; } = null!;

    public string? Description { get; set; }
    public bool IsActive { get; set; }

    public List<SubscriptionPlanFeatureUpsertRequest>? Features { get; set; }

    [Required]
    public SubscriptionPlanPriceUpdateRequest Price { get; set; } = null!;
}

public class SubscriptionPlanFeatureUpsertRequest
{
    public Guid? Id { get; set; }

    [Required]
    public FeatureKeys FeatureKey { get; set; } = FeatureKeys.Project;

    public int? LimitValue { get; set; }
}


public class SubscriptionPlanPriceUpdateRequest
{
    public BillingPeriod BillingPeriod { get; set; } // Week/Month/Year
    public int PeriodCount { get; set; } = 1;
    public decimal Price { get; set; }
    public string Currency { get; set; } = "VND";    // ISO 4217 (3-letter)
    public int RefundWindowDays { get; set; }
    public decimal RefundFeePercent { get; set; }    // 0..100
}

