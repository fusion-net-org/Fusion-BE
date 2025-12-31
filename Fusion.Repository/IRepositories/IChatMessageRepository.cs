using Fusion.Repository.Entities;

namespace Fusion.Repository.IRepositories;

public interface IChatMessageRepository
{
    Task<ChatMessage?> GetByClientMessageIdAsync(Guid conversationId, Guid senderId, string clientMessageId, CancellationToken ct = default);
    Task AddAsync(ChatMessage entity, CancellationToken ct = default);
}
