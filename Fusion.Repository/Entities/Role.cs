using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Fusion.Repository.Entities;

public partial class Role
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("company_id")]
    public Guid? CompanyId { get; set; }

    [Column("role_name")]
    [StringLength(100)]
    public string? RoleName { get; set; }

    [Column("description")]
    public string? Description { get; set; }

    [Column("status")]
    [StringLength(50)]
    public string Status { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; }

    [Column("updated_at")]
    public DateTime? UpdatedAt { get; set; }


    [Column("reason")]
    public string? Reason { get; set; }


    [ForeignKey("CompanyId")]
    [InverseProperty("Roles")]
    public virtual Company? Company { get; set; }

    [InverseProperty("Role")]
    public virtual ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();

    [InverseProperty("Role")]
    public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
}
