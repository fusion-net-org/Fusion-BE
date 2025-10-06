
namespace Fusion.Service.ViewModels.UserSubscription.Requests;

public class CreateUserSubscriptionRequest 
{

    public Guid PackageId { get; set; }

    public DateTime PurchaseDate { get; set; }

    public int QuotaCompanyAdded { get; set; }

    public int QuotaProjectAdded { get; set; }
}
