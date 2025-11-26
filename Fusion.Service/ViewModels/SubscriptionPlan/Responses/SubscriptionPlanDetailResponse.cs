

using Fusion.Repository.Enums;

namespace Fusion.Service.ViewModels.SubscriptionPlan.Responses;

public class SubscriptionPlanDetailResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = "";
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public LicenseScope LicenseScope { get; set; }
    public bool IsFullPackage { get; set; }
    public int? CompanyShareLimit { get; set; }
    public int? SeatsPerCompanyLimit { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public SubscriptionPlanPriceResponse? Price { get; set; }
    public List<SubscriptionPlanFeatureResponse> Features { get; set; } = new();
}
