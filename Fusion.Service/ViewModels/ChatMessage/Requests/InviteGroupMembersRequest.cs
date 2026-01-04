

namespace Fusion.Service.ViewModels.ChatMessage.Requests;

public class InviteGroupMembersRequest
{
    public List<Guid> MemberIds { get; set; } = new();
}
