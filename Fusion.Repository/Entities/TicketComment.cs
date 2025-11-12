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

    [Column("body")]
    public string? Body { get; set; }

    [Column("create_at")]
    [Precision(3)]
    public DateTime CreateAt { get; set; }

    [Column("update_at")]
    [Precision(3)]
    public DateTime UpdateAt { get; set; }

    [Column("is_deleted")]
    public bool? IsDeleted { get; set; }

    [ForeignKey("AuthorUserId")]
    [InverseProperty("TicketComments")]
    public virtual User? AuthorUser { get; set; }

    [ForeignKey("TicketId")]
    [InverseProperty("TicketComments")]
    public virtual Ticket? Ticket { get; set; }
}
