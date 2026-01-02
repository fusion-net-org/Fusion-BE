
using Fusion.Repository.Bases.Page;
using Fusion.Repository.Bases.Page.Chat;
using Fusion.Repository.Data;
using Fusion.Repository.Entities;
using Fusion.Repository.IRepositories;
using Fusion.Repository.ViewModels.Chat;
using Microsoft.EntityFrameworkCore;

namespace Fusion.Repository.Repositories;

public class ChatMessageRepository : IChatMessageRepository
{
    private readonly FusionDbContext _context;
    private readonly DbSet<ChatMessage> _dbSet;

    public ChatMessageRepository(FusionDbContext context)
    {
        _context = context;
        _dbSet = _context.Set<ChatMessage>();
    }

    public Task<ChatMessage?> GetByClientMessageIdAsync(Guid conversationId, Guid senderId, string clientMessageId, CancellationToken ct = default)
        => _dbSet.FirstOrDefaultAsync(x =>
            x.ConversationId == conversationId &&
            x.SenderId == senderId &&
            x.ClientMessageId == clientMessageId, ct);

    public Task AddAsync(ChatMessage entity, CancellationToken ct = default)
        => _dbSet.AddAsync(entity, ct).AsTask();

    public async Task<PagedResult<ChatMessageVm>> GetMessagesPagedAsync(Guid conversationId, ChatMessagePagedRequest request, CancellationToken ct = default)
    {
        var q = _dbSet.AsNoTracking()
            .Where(x => x.ConversationId == conversationId)
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new ChatMessageVm
            {
                Id = x.Id,
                ConversationId = x.ConversationId ?? Guid.Empty,
                SenderId = x.SenderId ?? Guid.Empty,
                Content = x.Content,
                ClientMessageId = x.ClientMessageId,
                CreatedAt = x.CreatedAt
            });

        return await q.ToPagedResultAsync(request, ct);
    }
}
