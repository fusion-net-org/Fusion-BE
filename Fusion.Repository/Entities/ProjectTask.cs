using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Fusion.Repository.Entities;

public partial class ProjectTask
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; }

    [Column("project_id")]
    public Guid? ProjectId { get; set; }

    [Column("sprint_id")]
    public Guid? SprintId { get; set; }

    [Column("type")]
    [StringLength(20)]
    [Unicode(false)]
    public string? Type { get; set; }

    [Column("img")]
    [StringLength(500)]
    public string? Img { get; set; }

    [Column("title")]
    [StringLength(200)]
    public string? Title { get; set; }

    [Column("description")]
    public string? Description { get; set; }

    [Column("priority")]
    [StringLength(20)]
    [Unicode(false)]
    public string? Priority { get; set; }

    [Column("is_backlog")]
    public bool IsBacklog { get; set; }

    [Column("point")]
    public int? Point { get; set; }

    [Column("status")]
    public string? Status { get; set; }
   

    [Column("due_date")]
    [Precision(3)]
    public DateTime? DueDate { get; set; }

    [Column("source")]
    [StringLength(100)]
    public string? Source { get; set; }

    [Column("withdrawn_at")]
    [Precision(3)]
    public DateTime? WithdrawnAt { get; set; }

    [Column("created_by")]
    public Guid? CreatedBy { get; set; }

    [Column("create_at")]
    [Precision(3)]
    public DateTime? CreateAt { get; set; }

    [Column("update_at")]
    [Precision(3)]
    public DateTime? UpdateAt { get; set; }
    [Column("order_in_sprint")]
    public int? OrderInSprint { get; set; }
    [Column("is_deleted")]
    public bool IsDeleted { get; set; }
    [InverseProperty("Task")]
    public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();

    [ForeignKey("CreatedBy")]
    [InverseProperty("ProjectTasks")]
    public virtual User? CreatedByNavigation { get; set; }

    [ForeignKey("ProjectId")]
    [InverseProperty("ProjectTasks")]
    public virtual Project? Project { get; set; }

    [ForeignKey("SprintId")]
    [InverseProperty("ProjectTasks")]
    public virtual Sprint? Sprint { get; set; }

    [InverseProperty("Task")]
    public virtual ICollection<TaskLogEvent> TaskLogEvents { get; set; } = new List<TaskLogEvent>();

    [InverseProperty("Task")]
    public virtual ICollection<TaskWorkflow> TaskWorkflows { get; set; } = new List<TaskWorkflow>();
}
