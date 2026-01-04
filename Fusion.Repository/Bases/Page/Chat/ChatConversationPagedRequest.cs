

namespace Fusion.Repository.Bases.Page.Chat;

public class ChatConversationPagedRequest : PagedRequest
{
    public int? Type { get; set; }     // 1 direct, 2 group
    public string? Keyword { get; set; } // search title or peer email
}
