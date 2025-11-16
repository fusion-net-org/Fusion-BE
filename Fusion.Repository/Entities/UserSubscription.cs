

using Fusion.Repository.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Fusion.Repository.Entities;
[Table("UserSubscriptions")]
public class UserSubscription
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; } = Guid.NewGuid();

    // --- Who & What plan ---
    [Required]
    [Column("user_id")]
    public Guid UserId { get; set; }

    [Required]
    [Column("plan_id")]
    public Guid PlanId { get; set; }

    // Chỉ audit – KHÔNG map FK để tránh multipath
    [Column("created_by_transaction_id")]
    public Guid? CreatedByTransactionId { get; set; }

    // --- Status & timeline ---
    [Column("status")]
    public SubscriptionStatus Status { get; set; } = SubscriptionStatus.Pending;

    [Column("term_start", TypeName = "datetimeoffset")]
    public DateTimeOffset? TermStart { get; set; }

    [Column("term_end", TypeName = "datetimeoffset")]
    public DateTimeOffset? TermEnd { get; set; }

    // Kỳ thanh toán tiếp theo (đối với installments)
    [Column("next_payment_due_at", TypeName = "datetimeoffset")]
    public DateTimeOffset? NextPaymentDueAt { get; set; }

    [Column("created_at", TypeName = "datetimeoffset")]
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    [Column("updated_at", TypeName = "datetimeoffset")]
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;

    [Column("canceled_at", TypeName = "datetimeoffset")]
    public DateTimeOffset? CanceledAt { get; set; }

    // --- Snapshots of PLAN at purchase ---
    [Column("license_scope_snapshot")]
    public LicenseScope LicenseScopeSnapshot { get; set; }

    [Column("is_full_package_snapshot")]
    public bool IsFullPackageSnapshot { get; set; }

    [Column("company_share_limit_snapshot")]
    public int? CompanyShareLimitSnapshot { get; set; }

    [Column("seats_per_company_limit_snapshot")]
    public int? SeatsPerCompanyLimitSnapshot { get; set; }

    // --- Snapshots of PRICE / billing at purchase (nguồn: TransactionPayment snapshot) ---
    [Column("charge_unit_snapshot")]
    public ChargeUnit ChargeUnitSnapshot { get; set; }

    [Column("billing_period_snapshot")]
    public BillingPeriod BillingPeriodSnapshot { get; set; }

    [Column("period_count_snapshot")]
    public int PeriodCountSnapshot { get; set; }

    [Column("payment_mode_snapshot")]
    public PaymentMode PaymentModeSnapshot { get; set; }

    [Column("installment_count_snapshot")]
    public int? InstallmentCountSnapshot { get; set; }

    [Column("installment_interval_snapshot")]
    public BillingPeriod? InstallmentIntervalSnapshot { get; set; }

    [MaxLength(3)]
    [Column("currency_snapshot")]
    public string CurrencySnapshot { get; set; } = "VND";

    // Giá gói snapshot tại thời điểm mua (tổng giá gói)
    [Column("unit_price_snapshot", TypeName = "decimal(18,2)")]
    public decimal UnitPriceSnapshot { get; set; }

    // -------- Navigations --------
    [ForeignKey(nameof(UserId))]
    public virtual User User { get; set; } = null!;

    [ForeignKey(nameof(PlanId))]
    public virtual SubscriptionPlan Plan { get; set; } = null!;

    public virtual ICollection<TransactionPayment> TransactionPayments { get; set; } = new List<TransactionPayment>();
    public virtual ICollection<UserSubscriptionEntitlement> Entitlements { get; set; } = new List<UserSubscriptionEntitlement>();
    //public virtual ICollection<CompanySubscription> CompanySubscriptions { get; set; } = new List<CompanySubscription>();
}
