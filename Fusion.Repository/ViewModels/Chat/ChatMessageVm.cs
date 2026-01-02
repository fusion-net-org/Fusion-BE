

namespace Fusion.Repository.ViewModels.Chat;

public class ChatMessageVm
{
    public Guid Id { get; set; }
    public Guid ConversationId { get; set; }
    public Guid SenderId { get; set; }
    public string? Content { get; set; }
    public string? ClientMessageId { get; set; }
    public DateTime? CreatedAt { get; set; }
}
