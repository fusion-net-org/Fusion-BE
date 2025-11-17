using Fusion.Repository.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Fusion.Repository.Entities;


[Table("CompanySubscriptionEntitlements")]
public class CompanySubscriptionEntitlement
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [Column("company_subscription_id")]
    public Guid CompanySubscriptionId { get; set; }

    [Required]
    [Column("feature_id")]
    public Guid FeatureId { get; set; }

    [Column("enabled")]
    public bool Enabled { get; set; } = true;

    [ForeignKey(nameof(CompanySubscriptionId))]
    public virtual CompanySubscription CompanySubscription { get; set; } = null!;

    [ForeignKey(nameof(FeatureId))]
    public virtual Feature Feature { get; set; } = null!;
}
