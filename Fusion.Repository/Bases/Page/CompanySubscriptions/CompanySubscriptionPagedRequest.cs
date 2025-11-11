

using Fusion.Repository.Entities;
using Fusion.Repository.Enums;

namespace Fusion.Repository.Bases.Page.CompanySubscriptions;

public class CompanySubscriptionPagedRequest : PagedRequest
{
    public string? PlanName { get; set; }
    public SubscriptionStatus? status { get; set; }
    public DateRange<DateTime> CreateAt { get; set; } = new();
    public DateRange<DateTime> ExpiredAt { get; set; } = new();
    public string? Keyword { get; set; }

    public static readonly Dictionary<string, string> SortMap = new(StringComparer.OrdinalIgnoreCase)
    {
        ["planName"] = nameof(CompanySubscription.NameSubscription),
        ["status"] = nameof(CompanySubscription.Status),
        ["createdAt"] = nameof(CompanySubscription.CreatedAt),
        ["expiredAt"] = nameof(CompanySubscription.ExpiredAt)
    };
}
