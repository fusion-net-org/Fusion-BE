

using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Fusion.Repository.Entities
{
    [Table("UserLogs")]
    public class UserLog
    {
        [Key]
        [Column("id")]
        public Guid Id { get; set; }

        [Column("actor_user_id")]
        public Guid? ActorUserId { get; set; }

        [Column("title")]
        [StringLength(200)]
        public string? Title { get; set; }

        [Column("description")]
        public string? Description { get; set; }

        [Column("created_at")]
        [Precision(3)]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("update_at")]
        [Precision(3)]
        public DateTime? UpdateAt { get; set; }

        [Column("is_deleted")]
        public bool IsDeleted { get; set; }
    }
}
