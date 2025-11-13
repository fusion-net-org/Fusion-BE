

//using Fusion.Repository.Enums;
//using System.ComponentModel.DataAnnotations;
//using System.ComponentModel.DataAnnotations.Schema;

//namespace Fusion.Repository.Entities;
//[Table("UserSubscriptions")]
//public class UserSubscription
//{
//    [Key]
//    [Column("id")]
//    public Guid Id { get; set; }

//    [Column("transaction_id")]
//    public Guid TransactionId { get; set; }

//    [Column("name_plan")]
//    public string? NamePlan { get; set; }

//    [Column("price")]
//    public decimal Price { get; set; }

//    [Column("currency")]
//    public string? Currency { get; set; }

//    [Column("status")]
//    public SubscriptionStatus Status { get; set; } = SubscriptionStatus.Active;

//    [Column("create_at")]
//    public DateTime CreatAt { get; set; } = DateTime.Now;

//    [Column("expired_at")]
//    public DateTime ExpiredAt { get; set; }

//    [Column("update_at")]
//    public DateTime? UpdateAt { get; set; }

//    [ForeignKey(nameof(TransactionId))]
//    [InverseProperty(nameof(TransactionPayment.UserSubscription))]
//    public TransactionPayment TransactionPayment { get; set; } = null!;

//    [InverseProperty(nameof(UserSubscriptionEntitlement.UserSubscription))]
//    public ICollection<UserSubscriptionEntitlement>? UserSubscriptionEntitlements { get; set; }


//    [InverseProperty(nameof(CompanySubscription.UserSubscription))]
//    public ICollection<CompanySubscription> CompanySubscriptions { get; set; } = new List<CompanySubscription>();
//}
