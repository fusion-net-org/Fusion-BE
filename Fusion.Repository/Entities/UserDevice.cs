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
    [Index(nameof(UserId), nameof(DeviceToken), IsUnique = true)]
    public partial class UserDevice
    {
        [Key]
        [Column("id")]
        public Guid Id { get; set; }

        [Column("user_id")]
        public Guid UserId { get; set; }

        [Column("device_token")]
        [Required, MaxLength(2000)]
        public string DeviceToken { get; set; } = default!;

        [Column("platform")]
        [MaxLength(50)]
        public string? Platform { get; set; } = "Web"; // Web / Android / iOS

        [Column("device_name")]
        [MaxLength(100)]
        public string? DeviceName { get; set; }

        [Column("create_at")]
        [Precision(3)]
        public DateTime? CreatedAt { get; set; }

        [Column("is_active")]
        public bool? IsActive { get; set; }

        [ForeignKey("UserId")]
        [InverseProperty("UserDevices")]
        public virtual User? User { get; set; }
    }
}
