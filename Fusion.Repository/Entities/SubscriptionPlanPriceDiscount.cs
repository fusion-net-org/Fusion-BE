using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fusion.Repository.Entities;

[Table("SubscriptionPlanPriceDiscounts")]
public class SubscriptionPlanPriceDiscount
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [ForeignKey(nameof(Price))]
    [Column("price_id")]
    public Guid PriceId { get; set; }

    /// <summary>
    /// Kỳ thứ mấy (1 = năm 1 / kỳ 1, 2 = năm 2 / kỳ 2, ...)
    /// </summary>
    [Column("installment_index")]
    public int InstallmentIndex { get; set; }  // 1-based

    /// <summary>
    /// Giá trị discount:
    /// - Nếu Percent: 10 = 10%
    /// - Nếu Amount: 100000 = giảm 100.000
    /// </summary>
    [Column("discount_value", TypeName = "decimal(18,2)")]
    public decimal DiscountValue { get; set; }

    [Column("note")]
    [MaxLength(250)]
    public string? Note { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public virtual SubscriptionPlanPrice Price { get; set; } = null!;
}