using Fusion.Service.IServices;
using Fusion.Service.ViewModels.ChatMessage.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Fusion.API.Hubs;

[Authorize]
public class ChatHub : Hub
{
    private readonly IChatService _chatService;

    public ChatHub(IChatService chatService)
    {
        _chatService = chatService;
    }

    public async Task JoinConversation(Guid conversationId, CancellationToken ct)
    {
        await _chatService.EnsureCanJoinConversationAsync(conversationId, ct);

        await Groups.AddToGroupAsync(Context.ConnectionId, $"conv:{conversationId}", ct);
    }

    public async Task SendMessage(SendMessageRequest request, CancellationToken ct)
    {
        var saved = await _chatService.SendMessageAsync(request, ct);

        // Broadcast MessageCreated tới group conv:{conversationId}
        await Clients.Group($"conv:{request.ConversationId}")
            .SendAsync("MessageCreated", saved, ct);
    }
}
