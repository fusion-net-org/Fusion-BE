
using Fusion.Repository.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Fusion.Repository.Entities;

[Table("SubscriptionPlans")]
public class SubscriptionPlan
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required, MaxLength(200)]
    [Column("name")]
    public string Name { get; set; } = string.Empty;

    [Column("description")]
    public string? Description { get; set; }

    [Column("is_active")]
    public bool IsActive { get; set; } = true;

    /// <summary>SeatBased: giới hạn theo user; CompanyWide: áp dụng toàn công ty</summary>
    [Column("license_scope")]
    public LicenseScope LicenseScope { get; set; } = LicenseScope.Userlimits;

    /// <summary>True = bật toàn bộ features</summary>
    [Column("is_full_package")]
    public bool IsFullPackage { get; set; } = false;

    /// <summary> true: đây là gói được hệ thống tự cấp hàng tháng</summary>
    [Column("auto_grant_monthly")]
    public bool AutoGrantMonthly { get; set; } = false;

    /// <summary>Số công ty tối đa có thể share; NULL = không giới hạn</summary>

    [Column("company_share_limit")]
    public int? CompanyShareLimit { get; set; }

    /// <summary>(SeatBased) Số user tối đa trong MỖI công ty; NULL = không giới hạn/không áp dụng</summary>

    [Column("seats_per_company_limit")]
    public int? SeatsPerCompanyLimit { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // 1–n: Plan có nhiều Prices
    public virtual SubscriptionPlanPrice? Price { get; set; }

    // 1–n: Plan có nhiều Feature toggles
    [InverseProperty(nameof(SubscriptionPlanFeature.SubscriptionPlan))]
    public ICollection<SubscriptionPlanFeature>? Features { get; set; }

    [InverseProperty(nameof(TransactionPayment.SubscriptionPlan))]
    public virtual ICollection<TransactionPayment> TransactionPayments { get; set; } = new List<TransactionPayment>();

    [InverseProperty(nameof(UserSubscription.Plan))]
    public virtual ICollection<UserSubscription> UserSubscriptions { get; set; } = new List<UserSubscription>();
}
