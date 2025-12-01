
namespace Fusion.Service.ViewModels.SubscriptionPlan.Responses;

public class SubscriptionPlanFeatureResponse
{
    public Guid FeatureId { get; set; }
    public string? FeatureCode { get; set; }
    public string? FeatureName { get; set; }
    public bool Enabled { get; set; }
    public int? MonthlyLimit { get; set; }
}
