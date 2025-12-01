
using Fusion.Repository.Enums;

namespace Fusion.Service.ViewModels.SubscriptionPlan.Requests;

public class SubscriptionPlanCreateRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    public bool IsActive { get; set; } = true;
    public LicenseScope LicenseScope { get; set; } = LicenseScope.Userlimits;

    /// <summary>True = bật toàn bộ features</summary>
    public bool IsFullPackage { get; set; } = false;

    /// <summary>
    /// True: đây là gói free được hệ thống tự cấp hàng tháng (auto-grant monthly).
    /// Khi bật, FE sẽ cho chọn từng feature và nhập monthly limit cho từng cái.
    /// </summary>
    public bool AutoGrantMonthly { get; set; } = false;

    /// <summary>Số công ty share tối đa; null = không giới hạn</summary>
    public int? CompanyShareLimit { get; set; }

    /// <summary>Số seat tối đa mỗi công ty (chỉ áp cho SeatBased); null = không giới hạn/không áp dụng</summary>
    public int? SeatsPerCompanyLimit { get; set; }

    public SubscriptionPlanPriceInput Price { get; set; } = new();

    /// <summary>Danh sách FeatureId được bật cho gói</summary>
    public List<Guid>? FeatureIds { get; set; }
    public List<SubscriptionPlanFeatureLimitInput>? FeatureMonthlyLimits { get; set; }
}

/// <summary>Input per-feature limit cho plan.</summary>
public class SubscriptionPlanFeatureLimitInput
{
    public Guid FeatureId { get; set; }
    public int? MonthlyLimit { get; set; }
}