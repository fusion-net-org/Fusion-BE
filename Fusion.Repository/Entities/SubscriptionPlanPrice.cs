using Fusion.Repository.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Fusion.Repository.Entities;

[Table("SubscriptionPlanPrices")]
public class SubscriptionPlanPrice
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [ForeignKey(nameof(SubscriptionPlan))]
    [Column("plan_id")]
    public Guid PlanId { get; set; }

    // Chu kỳ thanh toán (MONTH, YEAR)
    [Column("billing_period")]
    public BillingPeriod BillingPeriod { get; set; } = BillingPeriod.Month;

    // Số chu kỳ (ví dụ 1, 3, 12)
    [Column("period_count")]
    public int PeriodCount { get; set; } = 1;


    // PerSeat: giá cho MỖI seat (thường đi với SeatBased)
    [Column("charge_unit")]
    public ChargeUnit ChargeUnit { get; set; } = ChargeUnit.PerSubscription;

    [Column("price", TypeName = "decimal(18,2)")]
    public decimal Price { get; set; }

    // Mã tiền tệ (VND, USD...)
    [Required, MaxLength(10)]
    [Column("currency")]
    public string Currency { get; set; } = "VND";

    // --- Trả góp ---
    [Column("payment_mode")]
    public PaymentMode PaymentMode { get; set; } = PaymentMode.Prepaid;

    [Column("installment_count")]
    public int? InstallmentCount { get; set; } 

    [Column("installment_interval")]
    public BillingPeriod? InstallmentInterval { get; set; }

    [ForeignKey(nameof(PlanId))]
    public virtual SubscriptionPlan SubscriptionPlan { get; set; } = null!;
    [InverseProperty(nameof(SubscriptionPlanPriceDiscount.Price))]
    public virtual ICollection<SubscriptionPlanPriceDiscount> Discounts { get; set; } = new List<SubscriptionPlanPriceDiscount>();

}
