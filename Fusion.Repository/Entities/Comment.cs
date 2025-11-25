using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Fusion.Repository.Entities;

public partial class Comment
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("task_id")]
    public Guid? TaskId { get; set; }

    [Column("author_user_id")]
    public Guid? AuthorUserId { get; set; }

    [Column("body")]
    public string? Body { get; set; }

    [Column("create_at")]
    [Precision(3)]
    public DateTime CreateAt { get; set; }

    [Column("update_at")]
    [Precision(3)]
    public DateTime UpdateAt { get; set; }

    [Column("status")]
    public string? Status { get; set; }

    [ForeignKey("AuthorUserId")]
    [InverseProperty("Comments")]
    public virtual User? AuthorUser { get; set; }

    [ForeignKey("TaskId")]
    [InverseProperty("Comments")]
    public virtual ProjectTask? Task { get; set; }
    [InverseProperty(nameof(ProjectTaskAttachment.Comment))]
    public virtual ICollection<ProjectTaskAttachment> Attachments { get; set; } = new List<ProjectTaskAttachment>();
}
