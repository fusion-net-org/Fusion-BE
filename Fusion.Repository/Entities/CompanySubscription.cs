using Fusion.Repository.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Fusion.Repository.Entities;

[Table("CompanySubscriptions")]
public class CompanySubscription
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; }

    [Required]
    [Column("company_id")]
    public Guid CompanyId { get; set; }

    [Column("user_subscription_id")]
    public Guid UserSubscriptionId { get; set; }

    [Column("name_subscription")]
    [MaxLength(255)]
    public string? NameSubscription { get; set; }

    [Column("status")]
    public SubscriptionStatus Status { get; set; } = SubscriptionStatus.Active;

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    [Column("expired_at")]
    public DateTime ExpiredAt { get; set; }

    [Column("updated_at")]
    public DateTime? UpdatedAt { get; set; }

    [ForeignKey(nameof(CompanyId))]
    [InverseProperty(nameof(Company.CompanySubscriptions))]
    public Company Company { get; set; } = null!;

    [ForeignKey(nameof(UserSubscriptionId))]
    [InverseProperty(nameof(UserSubscription.CompanySubscriptions))]
    public UserSubscription UserSubscription { get; set; } = null!;

    [InverseProperty(nameof(CompanySubscriptionEntitlement.CompanySubscription))]
    public ICollection<CompanySubscriptionEntitlement> CompanySubscriptionEntitlements { get; set; } = new List<CompanySubscriptionEntitlement>();
}
