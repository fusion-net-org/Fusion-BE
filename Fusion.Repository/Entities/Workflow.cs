using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Fusion.Repository.Entities;

public partial class Workflow
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; }

    [Column("company_id")]
    public Guid? CompanyId { get; set; }

    [Column("name")]
    [StringLength(200)]
    public string? Name { get; set; }

    [Column("is_default")]
    public bool IsDefault { get; set; }

    [ForeignKey("CompanyId")]
    [InverseProperty("Workflows")]
    public virtual Company? Company { get; set; }

    [InverseProperty("Workflow")]
    public virtual ICollection<Project> Projects { get; set; } = new List<Project>();

    [InverseProperty("Workflow")]
    public virtual ICollection<WorkflowStatus> WorkflowStatuses { get; set; } = new List<WorkflowStatus>();

    [InverseProperty("Workflow")]
    public virtual ICollection<WorkflowTransition> WorkflowTransitions { get; set; } = new List<WorkflowTransition>();
}
