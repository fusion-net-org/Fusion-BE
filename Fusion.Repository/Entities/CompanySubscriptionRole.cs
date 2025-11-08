using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace Fusion.Repository.Entities;

[Table("CompanySubscriptionRoles")]
public class CompanySubscriptionRole
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; }

    [Column("company_subscription_id")]
    public Guid CompanySubscriptionId { get; set; }

    [Required, MaxLength(50)]
    [Column("name_role")]
    public string NameRole { get; set; } = null!;

    [ForeignKey(nameof(CompanySubscriptionId))]
    [InverseProperty(nameof(CompanySubscription.CompanySubscriptionRoles))]
    public CompanySubscription CompanySubscription { get; set; } = null!;
}
