using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Fusion.Repository.Entities;

public partial class FunctionInPage
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("function_code")]
    [StringLength(100)]
    public string? FunctionCode { get; set; }

    [Column("function_name")]
    [StringLength(200)]
    public string? FunctionName { get; set; }

    [Column("sort_order")]
    public int? SortOrder { get; set; }

    [Column("page_code")]
    [StringLength(100)]
    public string? PageCode { get; set; }

    [InverseProperty("Function")]
    public virtual ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
}
