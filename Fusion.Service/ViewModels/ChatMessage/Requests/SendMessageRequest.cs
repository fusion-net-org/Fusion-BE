

namespace Fusion.Service.ViewModels.ChatMessage.Requests;

public class SendMessageRequest
{
    public Guid ConversationId { get; set; }
    public string? Content { get; set; }
    public string? ClientMessageId { get; set; }
}
