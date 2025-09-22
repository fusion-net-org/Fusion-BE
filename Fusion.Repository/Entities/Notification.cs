using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Fusion.Repository.Entities;

[Index("UserId", "CreateAt", Name = "IX_Notifications_User_Time", IsDescending = new[] { false, true })]
public partial class Notification
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; }

    [Column("user_id")]
    public Guid? UserId { get; set; }

    [Column("event")]
    [StringLength(50)]
    public string? Event { get; set; }

    [Column("title")]
    [StringLength(200)]
    public string? Title { get; set; }

    [Column("body")]
    public string? Body { get; set; }

    [Column("context")]
    public string? Context { get; set; }

    [Column("link_url")]
    [StringLength(500)]
    public string? LinkUrl { get; set; }

    [Column("is_read")]
    public bool IsRead { get; set; }

    [Column("create_at")]
    [Precision(3)]
    public DateTime CreateAt { get; set; }

    [Column("read_at")]
    [Precision(3)]
    public DateTime? ReadAt { get; set; }

    [ForeignKey("UserId")]
    [InverseProperty("Notifications")]
    public virtual User? User { get; set; }
}
