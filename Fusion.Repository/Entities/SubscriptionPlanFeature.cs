
using Fusion.Repository.Enums;
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

    //Mã khóa tính năng : Create_project, max_partner
    [Required, MaxLength(50)]
    [Column("feature_key")]
    public string FeatureKey { get; set; } = FeatureKeys.Project.ToString();

    //Giới hạn tương ứng
    [Column("limit_value")]
    public int LimitValue { get; set; } = 0;

    // Quan hệ ngược lại
    public SubscriptionPlan? SubscriptionPlan { get; set; }

}
