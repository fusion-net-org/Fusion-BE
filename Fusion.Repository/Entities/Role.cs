using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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

    [ForeignKey("CompanyId")]
    [InverseProperty("Roles")]
    public virtual Company? Company { get; set; }

    [InverseProperty("Role")]
    public virtual ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();

    [InverseProperty("Role")]
    public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
}
