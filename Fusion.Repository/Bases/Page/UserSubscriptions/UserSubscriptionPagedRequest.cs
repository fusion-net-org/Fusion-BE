
using Fusion.Repository.Entities;
using Fusion.Repository.Enums;

namespace Fusion.Repository.Bases.Page.UserSubscriptions;

public class UserSubscriptionPagedRequest : PagedRequest
{
    public string? PlanName { get; set; }
    public SubscriptionStatus? status { get; set; }
    public DateRange<DateTime> CreateAt { get; set; } = new();
    public DateRange<DateTime> ExpiredAt { get; set; } = new();
    public string? Keyword { get; set; }

    public static readonly Dictionary<string, string> SortMap = new(StringComparer.OrdinalIgnoreCase)
    {
        ["planName"] = nameof(UserSubscription.NamePlan),
        ["price"] = nameof(UserSubscription.Price),
        ["currency"] = nameof(UserSubscription.Currency),
        ["status"] = nameof(UserSubscription.Status),
        ["createdAt"] = nameof(UserSubscription.CreatAt),
        ["expiredAt"] = nameof(UserSubscription.ExpiredAt)
    };
}
