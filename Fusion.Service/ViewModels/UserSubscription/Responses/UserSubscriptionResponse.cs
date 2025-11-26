

namespace Fusion.Service.ViewModels.UserSubscription.Responses;

public class  UserSubscriptionResponse
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid PlanId { get; set; }
    public string PlanName { get; set; } = "";
    public string Status { get; set; } = "";
    public DateTimeOffset? TermStart { get; set; }
    public DateTimeOffset? TermEnd { get; set; }
    public DateTimeOffset? NextPaymentDueAt { get; set; }
    public decimal UnitPrice { get; set; }
    public string Currency { get; set; } = "VND";
    public DateTimeOffset CreatedAt { get; set; }
}