using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

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
    public DateOnly? StartDate { get; set; }

    [Column("end_date")]
    public DateOnly? EndDate { get; set; }

    [Column("color")]
    [StringLength(20)]
    public string? Color { get; set; }

    [ForeignKey("ProjectId")]
    [InverseProperty("Sprints")]
    public virtual Project? Project { get; set; }

    [InverseProperty("Sprint")]
    public virtual ICollection<ProjectTask> ProjectTasks { get; set; } = new List<ProjectTask>();
}
