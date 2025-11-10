
using Fusion.Repository.Enums;

namespace Fusion.Service.ViewModels.UserSubscription.Responses;

public class UserSubscriptionListItem
{
    public Guid Id { get; set; }
    public string? NamePlan { get; set; }
    public decimal Price { get; set; }
    public string? Currency { get; set; }
    public SubscriptionStatus Status { get; set; }
    public DateTime ExpiredAt { get; set; }
}