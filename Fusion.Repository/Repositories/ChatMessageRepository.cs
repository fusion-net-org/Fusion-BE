
using Fusion.Repository.Data;
using Fusion.Repository.Entities;
using Fusion.Repository.IRepositories;
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
}
