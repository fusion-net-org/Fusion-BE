

namespace Fusion.Repository.Bases.Page.Chat;

public class ChatMessagePagedRequest : PagedRequest
{
    public Guid ConversationId { get; set; }
}