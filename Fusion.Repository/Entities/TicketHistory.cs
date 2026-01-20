using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Fusion.Repository.Entities
{
    [Table("ticket_histories")]
    public class TicketHistory
    {
        [Key]
        [Column("id")]
        public long Id { get; set; }

        [Column("ticket_id")]
        public Guid TicketId { get; set; }

        [Column("action")]
        [StringLength(50)]
        public string Action { get; set; } = null!;

        [Column("description")]
        public string? Description { get; set; }

        [Column("performed_by")]
        public Guid? PerformedBy { get; set; }

        [Column("created_at")]
        [Precision(3)]
        public DateTime CreatedAt { get; set; }

        // Navigation
        [ForeignKey(nameof(TicketId))]
        public virtual Ticket Ticket { get; set; } = null!;

        [ForeignKey(nameof(PerformedBy))]
        public virtual User? PerformedByUser { get; set; }
    }
}
