using Fusion.Repository.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Fusion.Repository.Entities;


[Table("CompanySubscriptionEntitlements")]
public class CompanySubscriptionEntitlement
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; }

    [Column("company_subscription_id")]
    public Guid CompanySubscriptionId { get; set; }

    [Required]
    [Column("feature_key")]
    public FeatureKeys FeatureKey { get; set; }

    [Column("quantity")]
    public int Quantity { get; set; }

    [Column("remaining")]
    public int Remaining { get; set; }

    [ForeignKey(nameof(CompanySubscriptionId))]
    [InverseProperty(nameof(CompanySubscription.CompanySubscriptionEntitlements))]
    public CompanySubscription CompanySubscription { get; set; } = null!;
}
