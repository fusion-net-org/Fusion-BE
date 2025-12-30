using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fusion.Repository.Entities
{
    public class UserFriendship
    {
        [Key]
        public Guid Id { get; set; }

        public string? PairKey { get; set; }

        [ForeignKey(nameof(Requester))]
        public Guid? RequesterId { get; set; }

        [ForeignKey(nameof(Addressee))]
        public Guid? AddresseeId { get; set; }

        public int? Status { get; set; } // Pending | Accepted | Rejected

        public DateTime? RequestedAt { get; set; }
        public DateTime? RespondedAt { get; set; }


        [InverseProperty(nameof(User.SentFriendRequests))]
        public virtual User? Requester { get; set; } = null!;

        [InverseProperty(nameof(User.ReceivedFriendRequests))]
        public virtual User? Addressee { get; set; } = null!;
    }
}
