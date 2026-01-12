using Fusion.Repository.Bases.Page;
using Fusion.Repository.Bases.Page.Chat;
using Fusion.Repository.ViewModels.Chat;
using Fusion.Service.ViewModels.ChatMessage.Requests;
using Fusion.Service.ViewModels.ChatMessage.Responses;


namespace Fusion.Service.IServices;

public interface IChatService
{
    Task<ChatConversationResponse> OpenDirectChatAsync(Guid otherUserId, CancellationToken ct = default);
    Task<ChatConversationResponse> CreateGroupChatAsync(CreateGroupChatRequest request, CancellationToken ct = default);
    Task InviteMembersToGroupAsync(Guid conversationId, InviteGroupMembersRequest request, CancellationToken ct = default);

    Task EnsureCanJoinConversationAsync(Guid conversationId, CancellationToken ct = default);
    Task<ChatMessageResponse> SendMessageAsync(SendMessageRequest request, CancellationToken ct = default);

    // new APIs
    Task<PagedResult<ChatConversationListItemVm>> GetMyConversationsPagedAsync(ChatConversationPagedRequest request, CancellationToken ct = default);
    Task<ChatConversationDetailVm> GetConversationByIdAsync(Guid conversationId, CancellationToken ct = default);
    Task<PagedResult<ChatMessageVm>> GetMessagesPagedAsync(Guid conversationId, ChatMessagePagedRequest request, CancellationToken ct = default);

    // internal helper: used when Accept friend
    Task EnsureDirectConversationExistsAsync(Guid userA, Guid userB, CancellationToken ct = default);
    Task KickMemberAsync(Guid conversationId, Guid targetUserId, CancellationToken ct = default);
}
