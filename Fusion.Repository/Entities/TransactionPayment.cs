

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Fusion.Repository.Entities;

public class TransactionPayment
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

    [Column("transaction_code")]
    [StringLength(50)]
    public string TransactionCode { get; set; } = null!;

    [Required]
    [Column("amount")]
    public decimal Amount { get; set; }

    [Column("payment_method")]
    [StringLength(50)]
    public string? PaymentMethod { get; set; }

    [Column("status")]
    [StringLength(50)]
    public string? Status { get; set; } // Pending, Success, Failed

    [Required]
    [Column("created_at")]
    public DateTime CreatedAt { get; set; }

    [Column("updated_at")]
    public DateTime? UpdatedAt { get; set; }

    [ForeignKey("UserId")]
    [InverseProperty("TransactionPayments")]
    public virtual User User { get; set; } = null!;

    [ForeignKey("PackageId")]
    [InverseProperty("TransactionPayments")]
    public virtual SubscriptionPlan SubscriptionPackage { get; set; } = null!;
}