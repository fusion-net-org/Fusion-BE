using Fusion.Repository.Bases.Page;
using Fusion.Repository.Bases.Page.Chat;
using Fusion.Repository.Entities;
using Fusion.Repository.ViewModels.Chat;

namespace Fusion.Repository.IRepositories;

public interface IChatMessageRepository
{
    Task<ChatMessage?> GetByClientMessageIdAsync(Guid conversationId, Guid senderId, string clientMessageId, CancellationToken ct = default);
    Task AddAsync(ChatMessage entity, CancellationToken ct = default);

    Task<PagedResult<ChatMessageVm>> GetMessagesPagedAsync(Guid conversationId, ChatMessagePagedRequest request, CancellationToken ct = default);
}
