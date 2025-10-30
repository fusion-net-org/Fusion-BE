

using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Fusion.Repository.Entities;

[Table("CompanySubscriptionAssignments")]
public partial class CompanySubscriptionAssignment
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; }

    [Column("company_id")]
    public Guid CompanyId { get; set; }

    [Column("code_transaction")]
    public string Code { get; set; }

    [Column("member_id")]
    public Guid MemberId { get; set; }

    [Column("is_enabled")]
    public bool IsEnabled { get; set; } = true;

    [Column("assigned_at")]
    [Precision(3)]
    public DateTime AssignedAt { get; set; } = DateTime.UtcNow;

    [Column("revoked_at")]
    [Precision(3)]
    public DateTime? RevokedAt { get; set; }

    [ForeignKey(nameof(MemberId))]
    public virtual User? Member { get; set; }
}
