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
    [Table("ProjectTaskAssignees")]
    public class ProjectTaskAssignee
    {
        [Key, Column("task_id", Order = 0)] public Guid TaskId { get; set; }
        [Key, Column("user_id", Order = 1)] public Guid UserId { get; set; }
        [Column("assigned_at"), Precision(3)] public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
        [ForeignKey(nameof(TaskId))] public ProjectTask Task { get; set; } = null!;
        [ForeignKey(nameof(UserId))] public User User { get; set; } = null!;
    }
}
