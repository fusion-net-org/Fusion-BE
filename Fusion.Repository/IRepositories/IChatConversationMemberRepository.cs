using Fusion.Repository.Entities;

namespace Fusion.Repository.IRepositories;

public interface IChatConversationMemberRepository
{
    Task<bool> IsMemberAsync(Guid conversationId, Guid userId, CancellationToken ct = default);
    Task<ChatConversationMember?> GetMemberAsync(Guid conversationId, Guid userId, CancellationToken ct = default);
    Task AddAsync(ChatConversationMember entity, CancellationToken ct = default);
    Task AddRangeAsync(IEnumerable<ChatConversationMember> entities, CancellationToken ct = default);
    void Remove(ChatConversationMember entity);
    Task<List<ChatConversationMember>> GetByConversationIdAsync(Guid conversationId, CancellationToken ct = default);
}
