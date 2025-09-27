using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Fusion.Repository.Entities;

[Index("GoogleSub", Name = "UQ__Users__74328F6E0433F7F6", IsUnique = true)]
public partial class User
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; }

    [Column("userName")]
    public string? UserName { get; set; }

    [Column("avatar")]
    public string? Avatar { get; set; }

    [Column("email")]
    [StringLength(320)]
    public string? Email { get; set; }

    [Column("phone")]
    [StringLength(20)]
    public string? Phone { get; set; }

    [Column("address")]
    public string? Address { get; set; }

    [Column("gender")]
    [StringLength(10)]
    [Unicode(false)]
    public string? Gender { get; set; }

    [Column("password_hash")]
    public byte[] PasswordHash { get; set; } = Array.Empty<byte>();

    [Column("password_salt")]
    public byte[] PasswordSalt { get; set; } = Array.Empty<byte>();

    [Column("google_sub")]
    [StringLength(128)]
    public string? GoogleSub { get; set; }

    [Column("is_system_admin")]
    public bool IsSystemAdmin { get; set; }

    [Column("status")]
    public bool Status { get; set; }

    [Column("create_at")]
    [Precision(3)]
    public DateTime CreateAt { get; set; }

    [Column("update_at")]
    [Precision(3)]
    public DateTime UpdateAt { get; set; }

    [InverseProperty("AuthorUser")]
    public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();

    [InverseProperty("OwnerUser")]
    public virtual ICollection<Company> Companies { get; set; } = new List<Company>();

    [InverseProperty("LastActionByNavigation")]
    public virtual ICollection<CompanyFriendship> CompanyFriendships { get; set; } = new List<CompanyFriendship>();

    [InverseProperty("User")]
    public virtual ICollection<CompanyMember> CompanyMembers { get; set; } = new List<CompanyMember>();

    [InverseProperty("User")]
    public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();

    [InverseProperty("User")]
    public virtual ICollection<ProjectMember> ProjectMembers { get; set; } = new List<ProjectMember>();

    [InverseProperty("CreatedByNavigation")]
    public virtual ICollection<ProjectRequest> ProjectRequests { get; set; } = new List<ProjectRequest>();

    [InverseProperty("CreatedByNavigation")]
    public virtual ICollection<ProjectTask> ProjectTasks { get; set; } = new List<ProjectTask>();

    [InverseProperty("CreatedByNavigation")]
    public virtual ICollection<Project> Projects { get; set; } = new List<Project>();

    [InverseProperty("Actor")]
    public virtual ICollection<TaskLogEvent> TaskLogEvents { get; set; } = new List<TaskLogEvent>();

    [InverseProperty("AssignUser")]
    public virtual ICollection<TaskWorkflow> TaskWorkflows { get; set; } = new List<TaskWorkflow>();

    [InverseProperty("AuthorUser")]
    public virtual ICollection<TicketComment> TicketComments { get; set; } = new List<TicketComment>();

    [InverseProperty("SubmittedByNavigation")]
    public virtual ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();

    [InverseProperty("User")]
    public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();

    [InverseProperty("User")]
    public virtual ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
}
