
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Fusion.Repository.Entities;

public class SubscriptionPlan
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; }

    [Required]
    [Column("name")]
    [StringLength(100)]
    public string Name { get; set; } = null!;

    [Required]
    [Column("price", TypeName = "decimal(18, 2)")]
    public decimal Price { get; set; }

    [Required]
    [Column("quota_company")]
    public int QuotaCompany { get; set; }

    [Required]
    [Column("quota_project")]
    public int QuotaProject { get; set; }

    [Required]
    [Column("description")]
    [StringLength(250)]
    public string Description { get; set; } = null!;

    [Column("create_at")]
    public DateTime CreatedAt { get; set; }

    [Column("update_at")]
    public DateTime UpdatedAt { get; set; }


    [InverseProperty("SubscriptionPackage")]
    public virtual ICollection<UserSubscription> UserSubscriptions { get; set; } = new List<UserSubscription>();

    [InverseProperty("SubscriptionPackage")]
    public virtual ICollection<TransactionPayment> TransactionPayments { get; set; } = new List<TransactionPayment>();
}
