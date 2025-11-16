

namespace Fusion.Service.ViewModels.UserSubscription.Requests;

public sealed class UpdateNextDueRequest
{
    public DateTimeOffset? NextPaymentDueAt { get; set; }
}
