

using Fusion.Repository.Entities;
using Fusion.Repository.Enums;

namespace Fusion.Repository.Bases.Page.CompanySubscriptions;

public class CompanySubscriptionPagedRequest : PagedRequest
{
    public SubscriptionStatus? Status { get; set; }
    public string? Keyword { get; set; }

    public static readonly Dictionary<string, string> SortMap = new(StringComparer.OrdinalIgnoreCase)
    {
        ["status"] = nameof(CompanySubscription.Status),
        ["createdAt"] = nameof(CompanySubscription.SharedOn),
        ["expiredAt"] = nameof(CompanySubscription.ExpiredAt)
    };
}
