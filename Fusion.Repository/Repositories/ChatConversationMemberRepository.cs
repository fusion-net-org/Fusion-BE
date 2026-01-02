using Fusion.Repository.Data;
using Fusion.Repository.Entities;
using Fusion.Repository.IRepositories;
using Microsoft.EntityFrameworkCore;

namespace Fusion.Repository.Repositories;

public class ChatConversationMemberRepository : IChatConversationMemberRepository
{
    private readonly FusionDbContext _context;
    private readonly DbSet<ChatConversationMember> _dbSet;

    public ChatConversationMemberRepository(FusionDbContext context)
    {
        _context = context;
        _dbSet = _context.Set<ChatConversationMember>();
    }

    public Task<bool> IsMemberAsync(Guid conversationId, Guid userId, CancellationToken ct = default)
       => _dbSet.AnyAsync(x => x.ConversationId == conversationId && x.UserId == userId, ct);

    public Task<List<ChatConversationMember>> GetByConversationIdAsync(Guid conversationId, CancellationToken ct = default)
        => _dbSet.Where(x => x.ConversationId == conversationId).ToListAsync(ct);

    public Task AddAsync(ChatConversationMember entity, CancellationToken ct = default)
        => _dbSet.AddAsync(entity, ct).AsTask();

    public Task AddRangeAsync(IEnumerable<ChatConversationMember> entities, CancellationToken ct = default)
        => _dbSet.AddRangeAsync(entities, ct);
}
