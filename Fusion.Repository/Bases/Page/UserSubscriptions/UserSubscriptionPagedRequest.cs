

namespace Fusion.Repository.Bases.Page.UserSubscriptions;

public class UserSubscriptionPagedRequest : PagedRequest
{
    public string? Keyword { get; set; }
    public string? Status { get; set; }
}
