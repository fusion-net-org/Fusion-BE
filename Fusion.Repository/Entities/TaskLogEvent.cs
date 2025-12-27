
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Fusion.Repository.Entities;

[Table("TaskLogEvent")]
public partial class TaskLogEvent
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("task_id")]
    public Guid? TaskId { get; set; }

    [Column("action")]
    [StringLength(30)]
    [Unicode(false)]
    public string? Action { get; set; }

    [Column("actor_id")]
    public Guid? ActorId { get; set; }

    [Column("changed_cols")]
    public string? ChangedCols { get; set; }

    [Column("old_row")]
    public string? OldRow { get; set; }

    [Column("new_row")]
    public string? NewRow { get; set; }

    [Column("metadata")]
    public string? Metadata { get; set; }

    [Column("created_at")]
    [Precision(3)]
    public DateTimeOffset CreatedAt { get; set; }

    [ForeignKey("ActorId")]
    [InverseProperty("TaskLogEvents")]
    public virtual User? Actor { get; set; }

    [ForeignKey("TaskId")]
    [InverseProperty("TaskLogEvents")]
    public virtual ProjectTask? Task { get; set; }
    [Column("is_view")]
    public bool IsView { get; set; }

    [Column("is_deleted")]
    public bool IsDeleted { get; set; }

    [Column("updated_at")]
    [Precision(3)]
    public DateTimeOffset? UpdatedAt { get; set; }
}
