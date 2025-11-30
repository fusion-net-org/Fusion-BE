using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Fusion.Repository.Entities;

public partial class Ticket
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; }

    [Column("project_id")]
    public Guid? ProjectId { get; set; }

    [Column("priority")]
    [StringLength(20)]
    [Unicode(false)]
    public string? Priority { get; set; }

    [Column("is_highest_urgen")]
    public bool IsHighestUrgen { get; set; }

    [Column("ticket_name")]
    [StringLength(200)]
    public string? TicketName { get; set; }

    [Column("description")]
    public string? Description { get; set; }

    [Column("status_id")]
    public Guid? StatusId { get; set; }

    [Column("submitted_by")]
    public Guid? SubmittedBy { get; set; }

    [Column("is_billable")]
    public bool IsBillable { get; set; }

    [Column("budget", TypeName = "decimal(18, 2)")]
    public decimal? Budget { get; set; }

	[Column("is_deleted")]
	public bool? IsDeleted { get; set; }

    [Column("reason")]

    public string? reason { get; set; }

    [Column("status")]

    public string? status { get; set; }

    [Column("resolved_at")]
    [Precision(3)]
    public DateTime? ResolvedAt { get; set; }

    [Column("closed_at")]
    [Precision(3)]
    public DateTime? ClosedAt { get; set; }

    [Column("created_at")]
    [Precision(3)]
    public DateTime CreatedAt { get; set; }

    [Column("updated_at")]
    [Precision(3)]
    public DateTime UpdatedAt { get; set; }

    [ForeignKey("ProjectId")]
    [InverseProperty("Tickets")]
    public virtual Project? Project { get; set; }

    [ForeignKey("StatusId")]
    [InverseProperty("Tickets")]
    public virtual WorkflowStatus? WorkflowStatus { get; set; }


    [ForeignKey("SubmittedBy")]
    [InverseProperty("Tickets")]
    public virtual User? SubmittedByNavigation { get; set; }

    [InverseProperty("Ticket")]
    public virtual ICollection<TicketComment> TicketComments { get; set; } = new List<TicketComment>();
    [InverseProperty(nameof(ProjectTask.Ticket))]
    public virtual ICollection<ProjectTask> Tasks { get; set; } = new List<ProjectTask>();
}
