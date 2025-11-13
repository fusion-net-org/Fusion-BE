//using Fusion.Repository.Enums;
//using System.ComponentModel.DataAnnotations;
//using System.ComponentModel.DataAnnotations.Schema;


//namespace Fusion.Repository.Entities;

//[Table("TransactionPayments")]
//public class TransactionPayment
//{
//    [Key]
//    [Column("id")]
//    public Guid Id { get; set; } = Guid.NewGuid();

//    [Required]
//    [Column("user_id")]
//    public Guid UserId { get; set; }

//    [Column("plan_id")]
//    public Guid PlanId { get; set; }


//    [Column("order_code")]
//    public long? OrderCode { get; set; }

//    [MaxLength(100)]
//    [Column("payment_link_id")]
//    public string? PaymentLinkId { get; set; }


//    [Column("amount", TypeName = "decimal(18,2)")]
//    public decimal Amount { get; set; }


//    [Column("description")]
//    [StringLength(250)]
//    public string? Description { get; set; }


//    [Column("account_number")]
//    [StringLength(250)]
//    public string? AccountNumber { get; set; } // stk receving


//    [Column("reference")]
//    [StringLength(250)]
//    public string? Reference { get; set; }


//    [Column("transaction_datetime", TypeName = "datetimeoffset")]
//    public DateTimeOffset? TransactionDateTime { get; set; }

//    [MaxLength(3)]
//    [Column("currency")]
//    public string? Currency { get; set; } = "VND";


//    [Column("counterAccountBankId")]
//    [StringLength(250)]
//    public string? CounterAccountBankId { get; set; } // Mã ngân hàng đối ứng

//    [Column("counterAccountBankName")]
//    [StringLength(250)]
//    public string? CounterAccountBankName { get; set; }  // Tên ngân hàng đối ứng

//    [Column("counterAccountName")]
//    [StringLength(250)]
//    public string? CounterAccountName { get; set; }   // Tên chủ tài khoản đối ứng

//    [Column("counterAccountNumber")]
//    [StringLength(250)]
//    public string? CounterAccountNumber { get; set; }  // Số tài khoản đối ứng


//    [Column("payment_method")]
//    [StringLength(50)]
//    public string? PaymentMethod { get; set; }

//    [Column("status")]
//    [StringLength(50)]
//    public string Status { get; set; } = PaymentStatus.Pending.ToString();

//    [Column("created_at")]
//    public DateTime CreatedAt { get; set; } = DateTime.Now;

//    [ForeignKey(nameof(UserId))]
//    [InverseProperty(nameof(User.TransactionPayments))]
//    public virtual User User { get; set; } = null!;

//    [ForeignKey(nameof(PlanId))]
//    [InverseProperty(nameof(SubscriptionPlan.TransactionPayments))]
//    public virtual SubscriptionPlan SubscriptionPlan { get; set; } = null!;

//    //[InverseProperty(nameof(UserSubscription.TransactionPayment))]
//    //public UserSubscription? UserSubscription { get; set; }
//}