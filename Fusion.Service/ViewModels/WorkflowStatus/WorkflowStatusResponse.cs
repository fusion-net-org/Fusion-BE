using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fusion.Service.ViewModels.WorkflowStatus
{
    public class WorkflowStatusResponse
    {
        [Key]
        [Column("id")]
        public Guid Id { get; set; }

        [Column("workflow_id")]
        public Guid? WorkflowId { get; set; }

        [Column("name")]
        [StringLength(100)]
        public string? Name { get; set; }

        [Column("position")]
        public int Position { get; set; }

        [Column("is_start")]
        public bool IsStart { get; set; }

        [Column("is_end")]
        public bool IsEnd { get; set; }

        [Column("guard_name_key")]
        [StringLength(100)]
        public string? GuardNameKey { get; set; }
        public string? Color { get; set; }
    }
}
