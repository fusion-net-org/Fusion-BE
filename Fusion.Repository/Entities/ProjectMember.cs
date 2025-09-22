using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Fusion.Repository.Entities;

public partial class ProjectMember
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("project_id")]
    public Guid? ProjectId { get; set; }

    [Column("user_id")]
    public Guid? UserId { get; set; }

    [Column("isPartner")]
    public bool IsPartner { get; set; }

    [Column("isViewAll")]
    public bool IsViewAll { get; set; }

    [Column("joined_at")]
    [Precision(3)]
    public DateTime JoinedAt { get; set; }

    [ForeignKey("ProjectId")]
    [InverseProperty("ProjectMembers")]
    public virtual Project? Project { get; set; }

    [ForeignKey("UserId")]
    [InverseProperty("ProjectMembers")]
    public virtual User? User { get; set; }
}
