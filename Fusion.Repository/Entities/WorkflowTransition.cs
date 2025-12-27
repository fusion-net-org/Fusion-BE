using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Fusion.Repository.Entities;

public partial class WorkflowTransition
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("workflow_id")]
    public Guid? WorkflowId { get; set; }

    [Column("from_status_id")]
    public Guid? FromStatusId { get; set; }

    [Column("to_status_id")]
    public Guid? ToStatusId { get; set; }
    [Column("enforce_transitions")]
    public bool EnforceTransitions { get; set; } = false;


    [ForeignKey("FromStatusId")]
    [InverseProperty("WorkflowTransitionFromStatuses")]
    public virtual WorkflowStatus? FromStatus { get; set; }

    [ForeignKey("ToStatusId")]
    [InverseProperty("WorkflowTransitionToStatuses")]
    public virtual WorkflowStatus? ToStatus { get; set; }

    [ForeignKey("WorkflowId")]
    [InverseProperty("WorkflowTransitions")]
    public virtual Workflow? Workflow { get; set; }
    [Column("type")]
    [StringLength(30)]
    public string? Type { get; set; }
    [Column("label"), StringLength(100)] public string? Label { get; set; }
    [Column("rule"), StringLength(400)] public string? Rule { get; set; }
    [Column("role_names_json")] public string? RoleNamesJson { get; set; }
}
