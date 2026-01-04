

namespace Fusion.Service.ViewModels.ChatMessage.Responses;

public class ChatConversationDetailResponse
{
    public Guid Id { get; set; }
    public int Type { get; set; }
    public string? Title { get; set; }
    public string? DirectPairKey { get; set; }
    public DateTime? LastMessageAt { get; set; }
    public List<ChatMemberResponse> Members { get; set; } = new();
}
