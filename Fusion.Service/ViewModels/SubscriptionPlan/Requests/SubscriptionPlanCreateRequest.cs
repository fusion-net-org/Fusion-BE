
using Fusion.Repository.Enums;

namespace Fusion.Service.ViewModels.SubscriptionPlan.Requests;

public class SubscriptionPlanCreateRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    public bool IsActive { get; set; } = true;
    public LicenseScope LicenseScope { get; set; } = LicenseScope.SeatBased;
    public bool IsFullPackage { get; set; } = false;

    /// <summary>Số công ty share tối đa; null = không giới hạn</summary>
    public int? CompanyShareLimit { get; set; }

    /// <summary>Số seat tối đa mỗi công ty (chỉ áp cho SeatBased); null = không giới hạn/không áp dụng</summary>
    public int? SeatsPerCompanyLimit { get; set; }

    public SubscriptionPlanPriceInput Price { get; set; } = new();

    /// <summary>Danh sách FeatureId được bật cho gói</summary>
    public List<Guid>? FeatureIds { get; set; }
}
