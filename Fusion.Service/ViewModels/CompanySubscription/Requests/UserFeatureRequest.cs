
using System.ComponentModel.DataAnnotations;

namespace Fusion.Service.ViewModels.CompanySubscription.Requests;

public class UserFeatureRequest
{
    [Required]
    public Guid companySubscriptionId {  get; set; }
    [Required]
    public long companyMemberId { get; set; }
    [Required]
    public string featureName {  get; set; }
}
