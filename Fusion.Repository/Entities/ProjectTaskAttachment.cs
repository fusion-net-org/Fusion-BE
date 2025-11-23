// Fusion.Repository/Entities/ProjectTaskAttachment.cs
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Fusion.Repository.Entities
{
    [Table("project_task_attachments")]
    public class ProjectTaskAttachment
    {
        [Key]
        [Column("id")]
        public Guid Id { get; set; }

        [Column("task_id")]
        public Guid TaskId { get; set; }

        [Column("file_name")]
        [StringLength(255)]
        public string FileName { get; set; } = default!;

        [Column("content_type")]
        [StringLength(100)]
        public string? ContentType { get; set; }

        [Column("size_bytes")]
        public long SizeBytes { get; set; }

        [Column("url")]
        [StringLength(500)]
        public string Url { get; set; } = default!;

        [Column("public_id")]
        [StringLength(200)]
        public string PublicId { get; set; } = default!;

        [Column("is_image")]
        public bool IsImage { get; set; }

        [Column("uploaded_by")]
        public Guid UploadedBy { get; set; }

        [Column("uploaded_at")]
        [Precision(3)]
        public DateTime UploadedAt { get; set; }

        [Column("description")]
        public string? Description { get; set; }

        [ForeignKey(nameof(TaskId))]
        [InverseProperty(nameof(ProjectTask.Attachments))]
        public ProjectTask Task { get; set; } = default!;
    }
}
