using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Fusion.Repository.Entities;

[Index("ProjectRequestId", Name = "UQ__Projects__B79D8DD66555382B", IsUnique = true)]
public partial class Project
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; }

    [Column("company_id")]
    public Guid? CompanyId { get; set; }

    [Column("isHired")]
    public bool IsHired { get; set; }

    [Column("company_request_id")]
    public Guid? CompanyRequestId { get; set; }

    [Column("project_request_id")]
    public Guid? ProjectRequestId { get; set; }

    [Column("code")]
    [StringLength(50)]
    public string? Code { get; set; }

    [Column("name")]
    [StringLength(200)]
    public string? Name { get; set; }

    [Column("description")]
    public string? Description { get; set; }

    [Column("status")]
    [StringLength(50)]
    public string? Status { get; set; }

    [Column("workflow_id")]
    public Guid? WorkflowId { get; set; }

    [Column("start_date")]
    public DateOnly? StartDate { get; set; }

    [Column("end_date")]
    public DateOnly? EndDate { get; set; }

    [Column("created_by")]
    public Guid? CreatedBy { get; set; }

    [Column("create_at")]
    [Precision(3)]
    public DateTime CreateAt { get; set; }

    [Column("update_at")]
    [Precision(3)]
    public DateTime UpdateAt { get; set; }
    [Column("is_closed")]
    public bool IsClosed { get; set; } = false;
    [Column("is_maintenance")]
    public bool IsMaintenance { get; set; } = false;
    [Column("maintenance_for_project_id")]
    public Guid? MaintenanceForProjectId { get; set; }
    [Column("closed_by")]
    public Guid? ClosedBy { get; set; }
    [Column("sprint_length_weeks")]
    public int? SprintLengthWeeks { get; set; }

    [ForeignKey("CompanyId")]
    [InverseProperty("ProjectCompanies")]
    public virtual Company? Company { get; set; }


    [ForeignKey("CompanyRequestId")]
    [InverseProperty("ProjectCompanyRequests")]
    public virtual Company? CompanyRequest { get; set; }

    [ForeignKey("CreatedBy")]
    [InverseProperty("Projects")]
    public virtual User? CreatedByNavigation { get; set; }

    [InverseProperty("Project")]
    public virtual ICollection<ProjectMember> ProjectMembers { get; set; } = new List<ProjectMember>();

    [ForeignKey("ProjectRequestId")]
    [InverseProperty("Project")]
    public virtual ProjectRequest? ProjectRequest { get; set; }

    [InverseProperty("Project")]
    public virtual ICollection<ProjectTask> ProjectTasks { get; set; } = new List<ProjectTask>();

    [InverseProperty("Project")]
    public virtual ICollection<Sprint> Sprints { get; set; } = new List<Sprint>();

    [InverseProperty("Project")]
    public virtual ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
   
    [ForeignKey("WorkflowId")]
    [InverseProperty("Projects")]
    public virtual Workflow? Workflow { get; set; }
    public virtual ICollection<ProjectComponent> Components { get; set; } = new List<ProjectComponent>();

}
