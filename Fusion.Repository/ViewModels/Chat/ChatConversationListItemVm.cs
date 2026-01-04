

namespace Fusion.Repository.ViewModels.Chat;

public class ChatConversationListItemVm
{
    public Guid Id { get; set; }
    public int Type { get; set; }
    public string? Title { get; set; }
    public DateTime? LastMessageAt { get; set; }

    // Direct: peer info
    public Guid? PeerUserId { get; set; }
    public string? PeerEmail { get; set; }
    public string? PeerAvatar { get; set; }
    public string? PeerUserName { get; set; }
}