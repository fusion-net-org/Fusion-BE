using Fusion.Repository.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Fusion.Repository.Entities;

[Table("CompanySubscriptions")]
public class CompanySubscription
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [Column("company_id")]
    public Guid CompanyId { get; set; }

    [Required]
    [Column("user_subscription_id")]
    public Guid UserSubscriptionId { get; set; }

    [Required]
    [Column("owner_user_id")]
    public Guid OwnerUserId { get; set; }

    [Column("status")]
    public SubscriptionStatus Status { get; set; } = SubscriptionStatus.Active;

    [Column("SharedOn", TypeName = "datetimeoffset")]
    public DateTimeOffset SharedOn { get; set; } = DateTimeOffset.UtcNow;

    [Column("updated_at", TypeName = "datetimeoffset")]
    public DateTimeOffset UpdatedAt { get; set; }

    [Column("expired_at", TypeName = "datetimeoffset")]
    public DateTimeOffset? ExpiredAt { get; set; }

    [Column("seats_limit_snapshot")]
    public int? SeatsLimitSnapshot { get; set; }

    [Column("seats_limit_unit")]
    public int? SeatsLimitUnit { get; set; }

    [ForeignKey(nameof(CompanyId))]
    public virtual Company Company { get; set; } = null!;

    [ForeignKey(nameof(UserSubscriptionId))]
    public virtual UserSubscription UserSubscription { get; set; } = null!;
    public virtual ICollection<CompanySubscriptionEntitlement> Entitlements { get; set; } = new List<CompanySubscriptionEntitlement>();

}
