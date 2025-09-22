using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Fusion.Repository.Entities;

public partial class RolePermission
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("function_id")]
    public int? FunctionId { get; set; }

    [Column("company_id")]
    public Guid? CompanyId { get; set; }

    [Column("role_id")]
    public int? RoleId { get; set; }

    [Column("is_access")]
    public bool IsAccess { get; set; }

    [ForeignKey("CompanyId")]
    [InverseProperty("RolePermissions")]
    public virtual Company? Company { get; set; }

    [ForeignKey("FunctionId")]
    [InverseProperty("RolePermissions")]
    public virtual FunctionInPage? Function { get; set; }

    [ForeignKey("RoleId")]
    [InverseProperty("RolePermissions")]
    public virtual Role? Role { get; set; }
}
