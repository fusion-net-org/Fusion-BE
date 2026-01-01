using Fusion.Repository.Entities;

namespace Fusion.Repository.IRepositories;

public interface IChatConversationRepository
{
    Task<ChatConversation?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<ChatConversation?> GetDirectByPairKeyAsync(string pairKey, CancellationToken ct = default);

    Task AddAsync(ChatConversation entity, CancellationToken ct = default);
    void Update(ChatConversation entity);
}
