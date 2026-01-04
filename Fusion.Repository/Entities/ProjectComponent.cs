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
    [Table("ProjectComponents")]
    public class ProjectComponent
    {
        [Key]
        [Column("id")]
        public Guid Id { get; set; }

        [Column("project_id")]
        public Guid? ProjectId { get; set; }

        [Column("project_request_id")]
        public Guid? ProjectRequestId { get; set; }

        [Column("name")]
        [StringLength(200)]
        public string Name { get; set; } = null!;

        [Column("description")]
        public string? Description { get; set; }

        [Column("created_at")]
        [Precision(3)]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("created_by")]
        public Guid? CreatedBy { get; set; }
        [ForeignKey(nameof(ProjectId))]
        public virtual Project? Project { get; set; }
        [ForeignKey(nameof(ProjectRequestId))]
        public virtual ProjectRequest? ProjectRequest { get; set; }
        [InverseProperty(nameof(ProjectTask.Component))]
        public virtual ICollection<ProjectTask> Tasks { get; set; } = new List<ProjectTask>();
        [InverseProperty(nameof(Ticket.Component))]
        public virtual ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
    }

}
