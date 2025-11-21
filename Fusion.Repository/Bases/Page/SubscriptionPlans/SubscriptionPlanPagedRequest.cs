
using Fusion.Repository.Entities;
using Fusion.Repository.Enums;

namespace Fusion.Repository.Bases.Page.SubscriptionPlans;

public class SubscriptionPlanPagedRequest : PagedRequest
{
    public string? Keyword { get; set; }
    public bool? IsActive { get; set; }
    public BillingPeriod? BillingPeriod { get; set; }

    public static readonly Dictionary<string, string> SortMap = new(StringComparer.OrdinalIgnoreCase)
    {
        ["createdAt"] = nameof(SubscriptionPlan.CreatedAt),
    };
}
