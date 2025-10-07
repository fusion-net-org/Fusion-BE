using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Fusion.Repository.Entities;

[Index("ConvertedProjectId", Name = "UQ__ProjectR__FDFB014B54F36625", IsUnique = true)]
public partial class ProjectRequest
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; }

    [Column("requester_company_id")]
    public Guid? RequesterCompanyId { get; set; }

    [Column("executor_company_id")]
    public Guid? ExecutorCompanyId { get; set; }

    [Column("created_by")]
    public Guid? CreatedBy { get; set; }

    [Column("code")]
    [StringLength(50)]
    public string? Code { get; set; }

    [Column("name")]
    [StringLength(200)]
    public string? Name { get; set; }

    [Column("description")]
    public string? Description { get; set; }

    [Column("status")]
    [StringLength(20)]
    [Unicode(false)]
    public string? Status { get; set; }

    [Column("start_date")]
    public DateOnly? StartDate { get; set; }

    [Column("end_date")]
    public DateOnly? EndDate { get; set; }

    [Column("create_at")]
    [Precision(3)]
    public DateTime CreateAt { get; set; }

    [Column("update_at")]
    [Precision(3)]
    public DateTime UpdateAt { get; set; }

    [Column("is_deleted")]
    public bool? IsDeleted { get; set; }

    [Column("converted_project_id")]
    public Guid? ConvertedProjectId { get; set; }

    [ForeignKey("CreatedBy")]
    [InverseProperty("ProjectRequests")]
    public virtual User? CreatedByNavigation { get; set; }

    [ForeignKey("ExecutorCompanyId")]
    [InverseProperty("ProjectRequestExecutorCompanies")]
    public virtual Company? ExecutorCompany { get; set; }

    [InverseProperty("ProjectRequest")]
    public virtual Project? Project { get; set; }

    [ForeignKey("RequesterCompanyId")]
    [InverseProperty("ProjectRequestRequesterCompanies")]
    public virtual Company? RequesterCompany { get; set; }
}
