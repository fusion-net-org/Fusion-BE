using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Fusion.Repository.Entities;

public partial class CompanyFriendship
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("company_a_id")]
    public Guid? CompanyAId { get; set; }

    [Column("company_b_id")]
    public Guid? CompanyBId { get; set; }

    [Column("requester_id")]
    public Guid? RequesterId { get; set; }

    [Column("status")]
    [StringLength(20)]
    [Unicode(false)]
    public string? Status { get; set; }

    [Column("responded_at")]
    [Precision(3)]
    public DateTime? RespondedAt { get; set; }

    [Column("last_action_by")]
    public Guid? LastActionBy { get; set; }

    [Column("created_at")]
    [Precision(3)]
    public DateTime CreatedAt { get; set; }

    [Column("updated_at")]
    [Precision(3)]
    public DateTime UpdatedAt { get; set; }

    [ForeignKey("CompanyAId")]
    [InverseProperty("CompanyFriendshipCompanyAs")]
    public virtual Company? CompanyA { get; set; }

    [ForeignKey("CompanyBId")]
    [InverseProperty("CompanyFriendshipCompanyBs")]
    public virtual Company? CompanyB { get; set; }

    [ForeignKey("LastActionBy")]
    [InverseProperty("CompanyFriendships")]
    public virtual User? LastActionByNavigation { get; set; }
}
