

namespace Fusion.Service.ViewModels.ChatMessage.Responses
{
    public class ChatConversationLiteResponse
    {
        public Guid Id { get; set; }
        public int Type { get; set; }
        public string? Title { get; set; }
        public DateTime? LastMessageAt { get; set; }

        // direct: peer info
        public Guid? PeerUserId { get; set; }
        public string? PeerEmail { get; set; }
        public string? PeerAvatar { get; set; }
    }
}
