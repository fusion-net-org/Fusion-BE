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
    [Table("ProjectTaskDependencies")]
    [PrimaryKey(nameof(TaskId), nameof(DependsOnTaskId))]
    public class ProjectTaskDependency
    {
        [Column("task_id")] public Guid TaskId { get; set; }
        [Column("depends_on_task_id")] public Guid DependsOnTaskId { get; set; }

        [Column("created_at"), Precision(3)]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ProjectTask Task { get; set; } = null!;

        public ProjectTask DependsOnTask { get; set; } = null!;
    }
}
