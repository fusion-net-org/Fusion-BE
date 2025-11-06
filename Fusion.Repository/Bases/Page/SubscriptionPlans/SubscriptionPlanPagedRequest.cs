
using Fusion.Repository.Entities;
using Fusion.Repository.Enums;

namespace Fusion.Repository.Bases.Page.SubscriptionPlans;

public class SubscriptionPlanPagedRequest : PagedRequest
{
    public string? Keyword { get; set; }

    public bool? IsActive { get; set; }

    // Lọc theo billing period của Price (Week/Month/Year)
    public BillingPriod? BillingPeriod { get; set; }

    // Lọc theo khoảng thời gian tạo
    public DateRange<DateTime> CreatedAt { get; set; } = new();

    // (tuỳ chọn) whitelist cột sort
    public static readonly Dictionary<string, string> SortMap = new(StringComparer.OrdinalIgnoreCase)
    {
        ["code"] = nameof(SubscriptionPlan.Code),
        ["name"] = nameof(SubscriptionPlan.Name),
        ["createdAt"] = nameof(SubscriptionPlan.CreatedAt),
        ["updatedAt"] = nameof(SubscriptionPlan.UpdatedAt),
        ["isActive"] = nameof(SubscriptionPlan.IsActive)
    };
}
