

using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Fusion.Repository.Entities;

[Table("CompanySubscriptionAssignments")]
public class CompanySubscriptionAssignment
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; }

    [Required]
    [Column("company_member_id")]
    public long CompanyMemberId { get; set; }         

    [Required]
    [Column("user_subscription_id")]
    public Guid UserSubscriptionId { get; set; }   

    [Column("code_transaction")]
    [StringLength(100)]
    public string? CodeTransaction { get; set; }

    [Column("is_enabled")]
    public bool IsEnabled { get; set; } = true;

    [Precision(3)]
    [Column("assigned_at")]
    public DateTime AssignedAt { get; set; } = DateTime.UtcNow;

    [Precision(3)]
    [Column("revoked_at")]
    public DateTime? RevokedAt { get; set; }

    [ForeignKey(nameof(CompanyMemberId))]
    public virtual CompanyMember Member { get; set; } = null!;

    [ForeignKey(nameof(UserSubscriptionId))]
    public virtual UserSubscription OwnerSubscription { get; set; } = null!;
}