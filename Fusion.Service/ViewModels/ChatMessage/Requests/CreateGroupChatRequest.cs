
namespace Fusion.Service.ViewModels.ChatMessage.Requests;

public class CreateGroupChatRequest
{
    public string? Title { get; set; }
    public List<Guid> MemberIds { get; set; } = new();
}