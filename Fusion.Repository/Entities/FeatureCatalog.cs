
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Fusion.Repository.Entities;


[Table("FeaturesCatalogs")]
public class Feature
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; }

    [Required, MaxLength(64)]
    [Column("code")]
    public string Code { get; set; } = "";

    [Required, MaxLength(200)]
    [Column("name")]
    public string Name { get; set; } = "";

    [MaxLength(400)]
    [Column("description")]
    public string? Description { get; set; }

    [MaxLength(50)]
    [Column("category")]
    public string? Category { get; set; }

    [Column("is_active")]
    public bool IsActive { get; set; } = true;

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<SubscriptionPlanFeature> PlanFeatures { get; set; } = new List<SubscriptionPlanFeature>();

}