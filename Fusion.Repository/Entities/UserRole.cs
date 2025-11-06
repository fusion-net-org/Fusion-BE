using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace Fusion.Repository.Entities;

public partial class UserRole
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; }

    [Column("user_id")]
    public Guid? UserId { get; set; }

    [Column("role_id")]
    public int? RoleId { get; set; }

    [ForeignKey("RoleId")]
    [InverseProperty("UserRoles")]
    public virtual Role? Role { get; set; }

    [ForeignKey("UserId")]
    [InverseProperty("UserRoles")]
    public virtual User? User { get; set; }
}
