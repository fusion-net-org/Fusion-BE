using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Fusion.Repository.Entities;

[Table("TaskWorkflow")]
public partial class TaskWorkflow
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; }

    [Column("task_id")]
    public Guid? TaskId { get; set; }

    [Column("workflow_status_id")]
    public Guid? WorkflowStatusId { get; set; }

    [Column("assign_user_id")]
    public Guid? AssignUserId { get; set; }
    //------------------------------------------------
    [Column("created_at"), Precision(3)]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    //------------------------------------------------

    [ForeignKey("AssignUserId")]
    [InverseProperty("TaskWorkflows")]
    public virtual User? AssignUser { get; set; }

    [ForeignKey("TaskId")]
    [InverseProperty("TaskWorkflows")]
    public virtual ProjectTask? Task { get; set; }

    [ForeignKey("WorkflowStatusId")]
    [InverseProperty("TaskWorkflows")]
    public virtual WorkflowStatus? WorkflowStatus { get; set; }
}
