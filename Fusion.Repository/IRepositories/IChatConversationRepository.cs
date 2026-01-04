using Fusion.Repository.Bases.Page;
using Fusion.Repository.Bases.Page.Chat;
using Fusion.Repository.Entities;
using Fusion.Repository.ViewModels.Chat;

namespace Fusion.Repository.IRepositories;

public interface IChatConversationRepository
{
    Task<ChatConversation?> GetByIdAsync(Guid id, CancellationToken ct = default);

    // DirectPairKey is used as ChatKey (Dr_/Gr_/oldPairKey)
    Task<ChatConversation?> GetByChatKeyAsync(string chatKey, CancellationToken ct = default);

    Task AddAsync(ChatConversation entity, CancellationToken ct = default);
    void Update(ChatConversation entity);

    Task<PagedResult<ChatConversationListItemVm>> GetMyConversationsPagedAsync(Guid userId, ChatConversationPagedRequest request, CancellationToken ct = default);

    Task<ChatConversationDetailVm?> GetConversationDetailAsync(Guid conversationId, CancellationToken ct = default);
}
