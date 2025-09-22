using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Fusion.Repository.Entities;

public partial class Company
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; }

    [Column("name")]
    [StringLength(200)]
    public string? Name { get; set; }

    [Column("owner_user_id")]
    public Guid? OwnerUserId { get; set; }

    [Column("tax_code")]
    [StringLength(50)]
    public string? TaxCode { get; set; }

    [Column("detail")]
    public string? Detail { get; set; }

    [Column("image_company")]
    [StringLength(500)]
    public string? ImageCompany { get; set; }

    [Column("create_at")]
    [Precision(3)]
    public DateTime CreateAt { get; set; }

    [Column("update_at")]
    [Precision(3)]
    public DateTime UpdateAt { get; set; }

    [InverseProperty("CompanyA")]
    public virtual ICollection<CompanyFriendship> CompanyFriendshipCompanyAs { get; set; } = new List<CompanyFriendship>();

    [InverseProperty("CompanyB")]
    public virtual ICollection<CompanyFriendship> CompanyFriendshipCompanyBs { get; set; } = new List<CompanyFriendship>();

    [InverseProperty("Company")]
    public virtual ICollection<CompanyMember> CompanyMembers { get; set; } = new List<CompanyMember>();

    [ForeignKey("OwnerUserId")]
    [InverseProperty("Companies")]
    public virtual User? OwnerUser { get; set; }

    [InverseProperty("Company")]
    public virtual ICollection<Project> ProjectCompanies { get; set; } = new List<Project>();

    [InverseProperty("CompanyHired")]
    public virtual ICollection<Project> ProjectCompanyHireds { get; set; } = new List<Project>();

    [InverseProperty("ExecutorCompany")]
    public virtual ICollection<ProjectRequest> ProjectRequestExecutorCompanies { get; set; } = new List<ProjectRequest>();

    [InverseProperty("RequesterCompany")]
    public virtual ICollection<ProjectRequest> ProjectRequestRequesterCompanies { get; set; } = new List<ProjectRequest>();

    [InverseProperty("Company")]
    public virtual ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();

    [InverseProperty("Company")]
    public virtual ICollection<Role> Roles { get; set; } = new List<Role>();

    [InverseProperty("Company")]
    public virtual ICollection<Workflow> Workflows { get; set; } = new List<Workflow>();
}
