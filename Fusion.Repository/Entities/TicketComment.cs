using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Fusion.Repository.Entities;

public partial class TicketComment
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("ticket_id")]
    public Guid? TicketId { get; set; }

    [Column("author_user_id")]
    public Guid? AuthorUserId { get; set; }

    [Column("visibility")]
    [StringLength(10)]
    [Unicode(false)]
    public string? Visibility { get; set; }

    [Column("body")]
    public string? Body { get; set; }

    [Column("create_at")]
    [Precision(3)]
    public DateTime CreateAt { get; set; }

    [ForeignKey("AuthorUserId")]
    [InverseProperty("TicketComments")]
    public virtual User? AuthorUser { get; set; }

    [ForeignKey("TicketId")]
    [InverseProperty("TicketComments")]
    public virtual Ticket? Ticket { get; set; }
}
