using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Fusion.Repository.Entities
{
    public class ChatMessage
    {
        [Key]
        public Guid Id { get; set; }

        [ForeignKey(nameof(Conversation))]
        public Guid? ConversationId { get; set; }

        [ForeignKey(nameof(Sender))]
        public Guid? SenderId { get; set; }

        public string? Content { get; set; } = null!;

        public string? ClientMessageId { get; set; } // idempotency

        public DateTime CreatedAt { get; set; }

        [InverseProperty(nameof(ChatConversation.Messages))]
        public ChatConversation? Conversation { get; set; } = null!;

        public virtual User? Sender { get; set; } = null!;
    }
}
