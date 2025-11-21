using Fusion.Repository.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace Fusion.Repository.Entities;

[Table("TransactionPayments")]
public class TransactionPayment
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [Column("user_id")]
    public Guid UserId { get; set; }

    [Column("user_subscription_id")]
    public Guid? UserSubscriptionId { get; set; }

    [Required]
    [Column("plan_id")]
    public Guid PlanId { get; set; }


    [Column("order_code")]
    public long? OrderCode { get; set; }

    [MaxLength(100)]
    [Column("payment_link_id")]
    public string? PaymentLinkId { get; set; }


    [MaxLength(50)]
    [Column("payment_method")]
    public string? PaymentMethod { get; set; }

    [MaxLength(32)]
    [Column("provider")]
    public string? Provider { get; set; }

    // ===== Snapshot pricing tại thời điểm charge =====
    [Column("charge_unit_snapshot")]
    public ChargeUnit ChargeUnitSnapshot { get; set; } = ChargeUnit.PerSubscription;

    [Column("billing_period_snapshot")]
    public BillingPeriod BillingPeriodSnapshot { get; set; } = BillingPeriod.Month;

    [Column("period_count_snapshot")]
    public int PeriodCountSnapshot { get; set; } = 1;

    // Nếu PerSeat => số seat mua ở lần này
    [Column("seat_count_snapshot")]
    public int? SeatCountSnapshot { get; set; }

    [Column("payment_mode_snapshot")]
    public PaymentMode PaymentModeSnapshot { get; set; } = PaymentMode.Prepaid;


    // Installments: kỳ thứ mấy / tổng số kỳ
    [Column("installment_index")]
    public int? InstallmentIndex { get; set; } // 1-based

    [Column("installment_total")]
    public int? InstallmentTotal { get; set; }

    // Số tiền thực thu
    [Column("amount", TypeName = "decimal(18,2)")]
    public decimal Amount { get; set; }


    [MaxLength(3)]
    [Column("currency")]
    public string? Currency { get; set; } = "VND";

    [Column("description")]
    [StringLength(250)]
    public string? Description { get; set; }

    [Column("reference")]
    [StringLength(250)]
    public string? Reference { get; set; }

    [Column("account_number")]
    [StringLength(250)]
    public string? AccountNumber { get; set; } // stk receving

    [Column("counterAccountBankId")]
    [StringLength(250)]
    public string? CounterAccountBankId { get; set; } // Mã ngân hàng đối ứng

    [Column("counterAccountBankName")]
    [StringLength(250)]
    public string? CounterAccountBankName { get; set; }  // Tên ngân hàng đối ứng

    [Column("counterAccountName")]
    [StringLength(250)]
    public string? CounterAccountName { get; set; }   // Tên chủ tài khoản đối ứng

    [Column("counterAccountNumber")]
    [StringLength(250)]
    public string? CounterAccountNumber { get; set; }  // Số tài khoản đối ứng


    [Column("transaction_datetime", TypeName = "datetimeoffset")]
    public DateTimeOffset? TransactionDateTime { get; set; }


    // ===== Mốc thời gian & trạng thái =====
    [Column("due_at", TypeName = "datetimeoffset")]
    public DateTimeOffset? DueAt { get; set; }     // kỳ đến hạn (installment) hoặc thời điểm cần thanh toán

    [Column("paid_at", TypeName = "datetimeoffset")]
    public DateTimeOffset? PaidAt { get; set; }    // thanh toán thành công


    [Column("status")]
    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;

    [Column("type")]
    public TransactionType Type { get; set; } = TransactionType.Charge;

    [Column("created_at", TypeName = "datetimeoffset")]
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    [ForeignKey(nameof(UserId))]
    public virtual User User { get; set; } = null!;

    [ForeignKey(nameof(PlanId))]
    public virtual SubscriptionPlan SubscriptionPlan { get; set; } = null!;
    [ForeignKey(nameof(UserSubscriptionId))]
    public virtual UserSubscription? UserSubscription { get; set; }
}