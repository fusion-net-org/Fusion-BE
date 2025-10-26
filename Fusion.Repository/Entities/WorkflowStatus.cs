using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Fusion.Repository.Entities;

[Table("WorkflowStatus")]
public partial class WorkflowStatus
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; }

    [Column("workflow_id")]
    public Guid? WorkflowId { get; set; }

    [Column("name")]
    [StringLength(100)]
    public string? Name { get; set; }

    [Column("position")]
    public int Position { get; set; }

    [Column("is_start")]
    public bool IsStart { get; set; }

    [Column("is_end")]
    public bool IsEnd { get; set; }

    [Column("guard_name_key")]
    [StringLength(100)]
    public string? GuardNameKey { get; set; }

    [InverseProperty("WorkflowStatus")]
    public virtual ICollection<TaskWorkflow> TaskWorkflows { get; set; } = new List<TaskWorkflow>();

    [InverseProperty("Status")]
    public virtual ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();

    [ForeignKey("WorkflowId")]
    [InverseProperty("WorkflowStatuses")]
    public virtual Workflow? Workflow { get; set; }

    [InverseProperty("FromStatus")]
    public virtual ICollection<WorkflowTransition> WorkflowTransitionFromStatuses { get; set; } = new List<WorkflowTransition>();

    [InverseProperty("ToStatus")]
    public virtual ICollection<WorkflowTransition> WorkflowTransitionToStatuses { get; set; } = new List<WorkflowTransition>();
    [Column("x")] public int X { get; set; }           // default 200
    [Column("y")] public int Y { get; set; }           // default 320
    [Column("color"), StringLength(9)] public string? Color { get; set; } // "#RRGGBB" / "#RRGGBBAA"
    [Column("roles_json")] public string? RolesJson { get; set; }
}
