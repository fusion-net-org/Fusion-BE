using Fusion.Service.ViewModels.ChatMessage.Requests;
using Fusion.Service.ViewModels.ChatMessage.Responses;


namespace Fusion.Service.IServices;

public interface IChatService
{
    Task<ChatConversationResponse> OpenDirectChatAsync(Guid otherUserId, CancellationToken ct = default);
    Task<ChatConversationResponse> CreateGroupChatAsync(CreateGroupChatRequest request, CancellationToken ct = default);

    Task EnsureCanJoinConversationAsync(Guid conversationId, CancellationToken ct = default);
    Task<ChatMessageResponse> SendMessageAsync(SendMessageRequest request, CancellationToken ct = default);
}
