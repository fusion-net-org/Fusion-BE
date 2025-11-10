

using Fusion.Repository.Enums;

namespace Fusion.Service.ViewModels.UserSubscription.Requests;

public class UpdateStatusRequest
{
    public SubscriptionStatus? Status { get; set; }
}
