
using System.ComponentModel.DataAnnotations;

namespace Fusion.Service.ViewModels.CompanySubscription.Requests;

public class UserFeatureRequest
{
    [Required]
    public Guid CompanySubscriptionId {  get; set; }
    [Required]
    public Guid ActorUserId { get; set; }
    [Required]
    public Guid CompanyId { get; set; }
    [Required]
    public string FeatureName {  get; set; }
}
