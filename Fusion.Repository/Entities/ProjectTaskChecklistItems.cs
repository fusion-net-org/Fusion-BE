using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fusion.Repository.Entities
{
    [Table("ProjectTaskChecklistItems")]
    [Index(nameof(TaskId))]
    public class ProjectTaskChecklistItem
    {
        [Key, Column("id")]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required, Column("task_id")]
        public Guid TaskId { get; set; }

        [Column("label"), StringLength(255)]
        public string Label { get; set; } = string.Empty;

        [Column("is_done")]
        public bool IsDone { get; set; } = false;

        [Column("order_index")]
        public int OrderIndex { get; set; } = 0;

        [Column("created_at"), Precision(3)]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey(nameof(TaskId))]
        public ProjectTask Task { get; set; } = null!;
    }
}
