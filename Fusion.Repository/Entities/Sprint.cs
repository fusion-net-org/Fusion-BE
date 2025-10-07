using Fusion.Repository.Enums;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Fusion.Repository.Entities;

public partial class Sprint
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; }

    [Column("project_id")]
    public Guid? ProjectId { get; set; }

    [Column("name")]
    [StringLength(200)]
    public string? Name { get; set; }

    [Column("start_date")]
    public DateTime? StartDate { get; set; }

    [Column("end_date")]
    public DateTime? EndDate { get; set; }

    [Column("color")]
    [StringLength(20)]
    public string? Color { get; set; }
    //============================================
    [Column("goal")]
    public string? Goal { get; set; }
    [Column("created_by")]
    public Guid? CreatedBy { get; set; }


    [Column("created_at"), Precision(3)]
    public DateTime? CreatedAt { get; set; }
    [Column("status")]
    public SprintStatus Status { get; set; }
    [Column("is_deleted")]
    public bool IsDeleted { get; set; }
    [Column("update_at"), Precision(3)]
    public DateTime? UpdateAt { get; set; }
    //============================================
    [ForeignKey("ProjectId")]
    [InverseProperty("Sprints")]
    public virtual Project? Project { get; set; }

    [InverseProperty("Sprint")]
    public virtual ICollection<ProjectTask> ProjectTasks { get; set; } = new List<ProjectTask>();
}
