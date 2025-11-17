

//using System.ComponentModel.DataAnnotations;
//using System.ComponentModel.DataAnnotations.Schema;

//namespace Fusion.Repository.Entities;

//[Table("CompanySubscriptionSeats")]
//public class CompanySubscriptionSeat
//{
//    [Key]
//    [Column("id")]
//    public Guid Id { get; set; } = Guid.NewGuid();

//    [Required]
//    [Column("company_subscription_id")]
//    public Guid CompanySubscriptionId { get; set; }

//    [Required]
//    [Column("company_member_id")]
//    public long CompanyMemberId { get; set; }

//    [Column("created_at", TypeName = "datetimeoffset")]
//    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

//    [ForeignKey(nameof(CompanySubscriptionId))]
//    public virtual CompanySubscription CompanySubscription { get; set; } = null!;

//    [ForeignKey(nameof(CompanyMemberId))]
//    public virtual CompanyMember CompanyMember { get; set; } = null!;
//}
