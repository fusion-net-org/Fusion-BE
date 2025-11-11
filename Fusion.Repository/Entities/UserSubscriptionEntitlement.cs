

using Fusion.Repository.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Fusion.Repository.Entities;

[Table("UserSubscriptionEntitlements")]
public class UserSubscriptionEntitlement
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; }

    [Column("user_subscription_id")]
    public Guid UserSubscriptionId { get; set; }

    [Required, MaxLength(50)]
    [Column("feature_key")]
    public FeatureKeys FeatureKey { get; set; } = FeatureKeys.Project;

    [Column("quantity")]
    public int Quantity { get; set; } 

    [Column("remaining")]
    public int Remaining { get; set; }

    [ForeignKey(nameof(UserSubscriptionId))]
    [InverseProperty(nameof(UserSubscription.UserSubscriptionEntitlements))]
    public UserSubscription? UserSubscription { get; set; }

}
