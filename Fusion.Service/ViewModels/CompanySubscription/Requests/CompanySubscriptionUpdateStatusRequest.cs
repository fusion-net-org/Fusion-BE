

using Fusion.Repository.Enums;

namespace Fusion.Service.ViewModels.CompanySubscription.Requests;

public class CompanySubscriptionUpdateStatusRequest
{
    public SubscriptionStatus Status { get; set; } // Active/Paused
}