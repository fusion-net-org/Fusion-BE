

namespace Fusion.Service.ViewModels.ChatMessage.Responses;

public class ChatMemberResponse
{
    public Guid UserId { get; set; }
    public int Role { get; set; }
    public string? Email { get; set; }
    public string? Avatar { get; set; }
}
