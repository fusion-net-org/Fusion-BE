

using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Fusion.Repository.Entities;

[Table("CompanyActivityLogs")]
public partial class CompanyActivityLog
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; }

    [Column("is_view")]
    public bool IsView { get; set; }
    [Column("company_id")]
    public Guid CompanyId { get; set; }

    [Column("actor_user_id")]
    public Guid? ActorUserId { get; set; }

    [Column("title")]
    [StringLength(200)]
    public string? Title { get; set; }

    [Column("description")]
    public string? Description { get; set; }

    [Column("created_at")]
    [Precision(3)]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("update_at")]
    [Precision(3)]
    public DateTime? UpdateAt { get; set; }

    [Column("is_deleted")]
    public bool IsDeleted { get; set; }
}
