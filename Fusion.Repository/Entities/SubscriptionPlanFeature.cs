
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Fusion.Repository.Entities;

[Table("subscriptionplanfeatures")]
public class SubscriptionPlanFeature
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; }

    [Required]
    [ForeignKey(nameof(SubscriptionPlan))]
    [Column("plan_id")]
    public Guid PlanId { get; set; }

    [Required]
    [Column("feature_id")]
    public Guid FeatureId { get; set; }

    [Column("enabled")]
    public bool Enabled { get; set; } = true;


    [ForeignKey(nameof(PlanId))]
    public virtual SubscriptionPlan SubscriptionPlans { get; set; } = null!;

    [ForeignKey(nameof(FeatureId))]
    public virtual Feature Feature { get; set; } = null!;
}
