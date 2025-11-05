
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Fusion.Repository.Entities;

public class UserSubscription
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; }

    [Required]
    [Column("user_id")]
    public Guid UserId { get; set; }

    [Required]
    [Column("package_id")]
    public Guid PackageId { get; set; }

    [Required]
    [Column("purchase_date")]
    public DateTime PurchaseDate { get; set; }

    [Required]
    [Column("quota_company_added")]
    public int QuotaCompanyAdded { get; set; }

    [Required]
    [Column("quota_project_added")]
    public int QuotaProjectAdded { get; set; }

    [Required]
    [Column("quota_company_remaining")]
    public int QuotaCompanyRemaining { get; set; }

    [Required]
    [Column("quota_project_remaining")]
    public int QuotaProjectRemaining { get; set; }

    [Column("expiry_date")]
    public DateTime? ExpiryDate { get; set; }

    [Column("is_active")]
    public bool IsActive { get; set; } = true;

    [ForeignKey("UserId")]
    [InverseProperty("UserSubscriptions")]
    public virtual User User { get; set; } = null!;

    [ForeignKey("PackageId")]
    [InverseProperty("UserSubscriptions")]
    public virtual SubscriptionPlan SubscriptionPackage { get; set; } = null!;

}
