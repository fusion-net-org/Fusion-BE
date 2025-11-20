
using System.ComponentModel.DataAnnotations;

namespace Fusion.Service.ViewModels.UserSubscription.Requests;

public class UserSubscriptionCreateRequest
{
    [Required]
    public Guid TransactionId { get; set; }
}