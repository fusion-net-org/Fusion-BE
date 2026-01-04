

namespace Fusion.Service.ViewModels.ChatMessage.Responses;

public class ChatConversationResponse
{
    public Guid Id { get; set; }
    public int Type { get; set; }
    public string? Title { get; set; }
    public string? DirectPairKey { get; set; } //  stores Dr_/Gr_
    public DateTime? LastMessageAt { get; set; }
}