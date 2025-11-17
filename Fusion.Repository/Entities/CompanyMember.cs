using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Fusion.Repository.Entities;

public partial class CompanyMember
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("company_id")]
    public Guid? CompanyId { get; set; }

    [Column("user_id")]
    public Guid? UserId { get; set; }

    [Column("status")]
    public string? Status { get; set; }

    [Column("reason")]
    public string? Reason { get; set; }

    [Column("is_deleted")]
    public bool? IsDeleted { get; set; }

    [Column("joined_at")]
    [Precision(3)]
    public DateTime? JoinedAt { get; set; }

    [ForeignKey("CompanyId")]
    [InverseProperty("CompanyMembers")]
    public virtual Company? Company { get; set; }

    [ForeignKey("UserId")]
    [InverseProperty("CompanyMembers")]
    public virtual User? User { get; set; }

}
