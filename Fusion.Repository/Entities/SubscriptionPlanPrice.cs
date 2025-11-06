using Fusion.Repository.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace Fusion.Repository.Entities;

[Table("subscriptionplanprices")]
public class SubscriptionPlanPrice
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; }

    [Required]
    [ForeignKey(nameof(SubscriptionPlan))]
    [Column("plan_id")]
    public Guid PlanId { get; set; }

    //Chu kỳ thanh toán (WEEK, MONTH, YEAR)
    [Required, MaxLength(20)]
    [Column("billing_period")]
    public BillingPriod BillingPeriod { get; set; } = BillingPriod.Month;

    //Số chu kỳ (ví dụ 1, 3, 12)
    [Column("period_count")]
    public int PeriodCount { get; set; } = 1;

    [Column("price", TypeName = "decimal(18,2)")]

    //Mã tiền tệ(VND, USD...)
    public decimal Price { get; set; }

    [Required, MaxLength(10)]
    [Column("currency")]
    public string Currency { get; set; } = "VND";

    //Thời gian cho phép hoàn tiền
    [Column("refund_window_days")]
    public int RefundWindowDays { get; set; } = 7;

    //% khấu trừ khi refund
    [Column("refund_fee_percent", TypeName = "decimal(5,2)")]
    public decimal RefundFeePercent { get; set; } = 0;

    // Quan hệ ngược lại
    public SubscriptionPlan? SubscriptionPlan { get; set; }
}
