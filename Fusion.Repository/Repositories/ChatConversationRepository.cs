using Fusion.Repository.Data;
using Fusion.Repository.Entities;
using Fusion.Repository.IRepositories;
using Microsoft.EntityFrameworkCore;

namespace Fusion.Repository.Repositories;

public class ChatConversationRepository : IChatConversationRepository
{
    private readonly FusionDbContext _context;
    private readonly DbSet<ChatConversation> _dbSet;

    public ChatConversationRepository(FusionDbContext context)
    {
        _context = context;
        _dbSet = _context.Set<ChatConversation>();
    }

    public Task<ChatConversation?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => _dbSet.FirstOrDefaultAsync(x => x.Id == id, ct);

    public Task<ChatConversation?> GetDirectByPairKeyAsync(string pairKey, CancellationToken ct = default)
        => _dbSet.FirstOrDefaultAsync(x => x.Type == 1 && x.DirectPairKey == pairKey, ct);

    public Task AddAsync(ChatConversation entity, CancellationToken ct = default)
        => _dbSet.AddAsync(entity, ct).AsTask();

    public void Update(ChatConversation entity)
        => _dbSet.Update(entity);
}
