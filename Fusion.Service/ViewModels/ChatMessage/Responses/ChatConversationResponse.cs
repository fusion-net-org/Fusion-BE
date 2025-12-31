

namespace Fusion.Service.ViewModels.ChatMessage.Responses;

public class ChatConversationResponse
{
    public Guid Id { get; set; }
    public int Type { get; set; } // 1 Direct | 2 Group
    public string? Title { get; set; }
    public string? DirectPairKey { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? LastMessageAt { get; set; }
}