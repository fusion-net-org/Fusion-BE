

namespace Fusion.Service.ViewModels.CompanySubscription.Requests;

public class CompanySubscriptionCreateRequest
{
    public Guid UserSubscriptionId { get; set; }
    public Guid CompanyId { get; set; }
    public Guid OwnerUserId { get; set; }

}