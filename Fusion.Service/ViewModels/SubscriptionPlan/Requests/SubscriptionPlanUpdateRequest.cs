

using System.ComponentModel.DataAnnotations;

namespace Fusion.Service.ViewModels.SubscriptionPlan.Requests;

public class SubscriptionPlanUpdateRequest : SubscriptionPlanCreateRequest
{
    [Required]
    public Guid Id { get; set; }
}

