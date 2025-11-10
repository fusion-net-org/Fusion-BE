

using Fusion.Repository.Enums;

namespace Fusion.Service.ViewModels.CompanySubscription.Responses;

public class CompanySubscriptionListResponse
{
    public Guid Id { get; set; }
    public string? NameSubscription { get; set; }
    public SubscriptionStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime ExpiredAt { get; set; }
}
