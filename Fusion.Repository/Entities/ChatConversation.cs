using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fusion.Repository.Entities
{
    public class ChatConversation
    {
        [Key]
        public Guid Id { get; set; }

        public int? Type { get; set; } // 1 = Direct | 2 = Group

        public string? Title { get; set; } // nullable (Group)

        public string? DirectPairKey { get; set; } // chỉ dùng khi type = Direct

        [ForeignKey(nameof(CreatedByUser))]
        public Guid? CreatedBy { get; set; }

        public DateTime? CreatedAt { get; set; }
        public DateTime? LastMessageAt { get; set; }


        [InverseProperty(nameof(ChatConversationMember.Conversation))]
        public ICollection<ChatConversationMember> Members { get; set; } = new List<ChatConversationMember>();

        [InverseProperty(nameof(ChatMessage.Conversation))]
        public ICollection<ChatMessage> Messages { get; set; } = new List<ChatMessage>();

        public virtual User? CreatedByUser { get; set; } = null!;
    }
}
