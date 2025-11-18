

namespace Fusion.Service.ViewModels.CompanySubscription.Responses;

public class CompanySubscriptionListResponse
{
    public Guid Id { get; set; }
    public string? CompanyName { get; set; }
    public string? UserName { get; set; }
    public string? PlanName { get; set; }
    public string Status { get; set; } = default!;
    public DateTimeOffset SharedOn { get; set; }
    public DateTimeOffset? ExpiredAt { get; set; }
    public int? SeatsLimitSnapshot { get; set; }
    public int? SeatsLimitUnit { get; set; }
}
