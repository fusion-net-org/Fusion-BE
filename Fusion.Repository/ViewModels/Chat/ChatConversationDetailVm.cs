

namespace Fusion.Repository.ViewModels.Chat;

public class ChatMemberVm
{
    public Guid UserId { get; set; }
    public int Role { get; set; }
    public string? Email { get; set; }
    public string? Avatar { get; set; }
    public string? UserName { get; set; }
}

public class ChatConversationDetailVm
{
    public Guid Id { get; set; }
    public int Type { get; set; }
    public string? Title { get; set; }
    public string? DirectPairKey { get; set; } // stores Dr_/Gr_
    public DateTime? LastMessageAt { get; set; }

    public List<ChatMemberVm> Members { get; set; } = new();
}