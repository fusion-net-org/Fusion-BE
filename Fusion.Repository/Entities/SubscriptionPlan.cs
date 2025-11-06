
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Fusion.Repository.Entities;

[Table("subscriptionplans")]
public class SubscriptionPlan
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; }

    [Required, MaxLength(50)]
    [Column("code")]
    public string Code { get; set; } = string.Empty;

    [Required, MaxLength(200)]
    [Column("name")]
    public string Name { get; set; } = string.Empty;

    [Column("description")]
    public string? Description { get; set; }

    [Column("is_active")]
    public bool IsActive { get; set; } = true;

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public SubscriptionPlanPrice? Price { get; set; }

    [InverseProperty(nameof(SubscriptionPlanFeature.SubscriptionPlan))]
    public ICollection<SubscriptionPlanFeature>? Features { get; set; }
}
