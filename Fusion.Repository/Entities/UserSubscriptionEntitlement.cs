
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Fusion.Repository.Entities;

[Table("UserSubscriptionEntitlements")]
public class UserSubscriptionEntitlement
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [Column("user_subscription_id")]
    public Guid UserSubscriptionId { get; set; }

    [Required]
    [Column("feature_id")]
    public Guid FeatureId { get; set; }

    [Column("enabled")]
    public bool Enabled { get; set; } = true;

    [ForeignKey(nameof(UserSubscriptionId))]
    public virtual UserSubscription UserSubscription { get; set; } = null!;

    [ForeignKey(nameof(FeatureId))]
    public virtual Feature Feature { get; set; } = null!;

}
