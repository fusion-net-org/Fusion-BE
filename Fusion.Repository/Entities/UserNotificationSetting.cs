using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fusion.Repository.Entities
{
    public class UserNotificationSetting
    {
        [Key]
        [Column("id")]
        public Guid Id { get; set; }

        [Column("user_id")]
        public Guid UserId { get; set; }

        [Column("notification_type")]
        [StringLength(50)]
        public string? NotificationType { get; set; }

        [Column("is_enabled")]
        public bool? IsEnabled { get; set; }

        [ForeignKey("UserId")]
        [InverseProperty("UserNotificationSettings")]
        public virtual User User { get; set; } = null!;
    }
}
