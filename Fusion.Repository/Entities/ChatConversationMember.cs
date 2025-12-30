using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fusion.Repository.Entities
{
    public class ChatConversationMember
    {
        [Key]
        public Guid Id { get; set; }

        [ForeignKey(nameof(Conversation))]
        public Guid? ConversationId { get; set; }

        [ForeignKey(nameof(User))]
        public Guid? UserId { get; set; }

        public int Role { get; set; } // 0 = Member | 1 = Owner

        [ForeignKey(nameof(AddedByUser))]
        public Guid? AddedBy { get; set; }

        public DateTime? JoinedAt { get; set; }

        [InverseProperty(nameof(ChatConversation.Members))]
        public ChatConversation? Conversation { get; set; } = null!;

        [InverseProperty(nameof(User.ChatConversationMembers))]
        public virtual User? User { get; set; } = null!;

        public virtual User? AddedByUser { get; set; } = null!;
    }
}
