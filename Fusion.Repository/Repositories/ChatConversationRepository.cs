using Fusion.Repository.Bases.Page;
using Fusion.Repository.Bases.Page.Chat;
using Fusion.Repository.Data;
using Fusion.Repository.Entities;
using Fusion.Repository.IRepositories;
using Fusion.Repository.ViewModels.Chat;
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

    /// <summary>
    /// Lấy conversation theo Id.
    /// </summary>
    public Task<ChatConversation?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => _dbSet.FirstOrDefaultAsync(x => x.Id == id, ct);

    /// <summary>
    /// (Legacy) Lấy direct conversation theo pairKey (khi DirectPairKey còn lưu raw pairKey).
    /// </summary>
    public Task<ChatConversation?> GetDirectByPairKeyAsync(string pairKey, CancellationToken ct = default)
        => _dbSet.FirstOrDefaultAsync(x => x.Type == 1 && x.DirectPairKey == pairKey, ct);

    /// <summary>
    /// Thêm mới conversation.
    /// </summary>
    public Task AddAsync(ChatConversation entity, CancellationToken ct = default)
        => _dbSet.AddAsync(entity, ct).AsTask();

    /// <summary>
    /// Update conversation (vd: LastMessageAt, Title...).
    /// </summary>
    public void Update(ChatConversation entity)
        => _dbSet.Update(entity);

    /// <summary>
    /// Lấy conversation theo chatKey đang lưu trong DirectPairKey.
    /// - Direct: "Dr_{pairKey}"
    /// - Group : "Gr_{conversationId}"
    /// </summary>
    public Task<ChatConversation?> GetByChatKeyAsync(string chatKey, CancellationToken ct = default)
        => _dbSet.FirstOrDefaultAsync(x => x.DirectPairKey == chatKey, ct);

    /// <summary>
    /// Lấy danh sách conversation của user (sidebar chat), có phân trang + filter/search.
    /// - Direct: trả thêm thông tin "peer" (người chat cùng)
    /// - Group : trả Title
    /// </summary>
    public async Task<PagedResult<ChatConversationListItemVm>> GetMyConversationsPagedAsync(
        Guid userId,
        ChatConversationPagedRequest request,
        CancellationToken ct = default)
    {
        var members = _context.Set<ChatConversationMember>().AsNoTracking();
        var users = _context.Set<User>().AsNoTracking();
        var convs = _dbSet.AsNoTracking();

        // Direct conversations: tìm peer bằng member khác userId trong cùng conversation
        var directQ =
            from my in members
            join c in convs on my.ConversationId equals c.Id
            join peer in members on c.Id equals peer.ConversationId
            join pu in users on peer.UserId equals pu.Id
            where my.UserId == userId
                  && (c.Type ?? 0) == 1
                  && peer.UserId != userId
            select new ChatConversationListItemVm
            {
                Id = c.Id,
                Type = c.Type ?? 0,
                Title = null,
                LastMessageAt = c.LastMessageAt,

                // các field peer (nếu Vm bạn khác tên thì đổi lại)
                PeerUserId = peer.UserId,
                PeerEmail = pu.Email,
                PeerUserName = pu.UserName,
                PeerAvatar = pu.Avatar, // nếu là AvatarUrl => đổi pu.AvatarUrl
            };

        // Group conversations
        var groupQ =
            from my in members
            join c in convs on my.ConversationId equals c.Id
            where my.UserId == userId
                  && (c.Type ?? 0) == 2
            select new ChatConversationListItemVm
            {
                Id = c.Id,
                Type = c.Type ?? 0,
                Title = c.Title,
                LastMessageAt = c.LastMessageAt,

                PeerUserId = null,
                PeerEmail = null,
                PeerAvatar = null,
                PeerUserName = null,
            };

        IQueryable<ChatConversationListItemVm> q;

        if (request.Type == 1) q = directQ;
        else if (request.Type == 2) q = groupQ;
        else q = directQ.Concat(groupQ);

        // Search keyword:
        // - group: search title
        // - direct: search peer email
        if (!string.IsNullOrWhiteSpace(request.Keyword))
        {
            var kw = request.Keyword.Trim();
            q = q.Where(x =>
                ((x.Type == 2) && (x.Title ?? "").Contains(kw)) ||
                ((x.Type == 1) && (x.PeerEmail ?? "").Contains(kw)));
        }

        // Sort: mới nhắn lên trên
        q = q.OrderByDescending(x => x.LastMessageAt);

        // (tùy bạn) đảm bảo không trùng
        q = q.Distinct();

        return await q.ToPagedResultAsync(request, ct);
    }

    /// <summary>
    /// Lấy chi tiết 1 conversation + danh sách members (kèm email/avatar/role).
    /// </summary>
    public async Task<ChatConversationDetailVm?> GetConversationDetailAsync(Guid conversationId, CancellationToken ct = default)
    {
        var conv = await _dbSet.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == conversationId, ct);

        if (conv == null) return null;

        var members = await (
            from m in _context.Set<ChatConversationMember>().AsNoTracking()
            join u in _context.Set<User>().AsNoTracking()
                on m.UserId equals u.Id
            where m.ConversationId == conversationId
            select new ChatMemberVm
            {
                UserId = m.UserId ?? Guid.Empty,
                Role = m.Role,
                Email = u.Email,
                UserName = u.UserName,
                Avatar = u.Avatar, // nếu AvatarUrl => đổi u.AvatarUrl
            }
        ).ToListAsync(ct);

        return new ChatConversationDetailVm
        {
            Id = conv.Id,
            Type = conv.Type ?? 0,
            Title = conv.Title,
            DirectPairKey = conv.DirectPairKey,
            LastMessageAt = conv.LastMessageAt,
            Members = members
        };
    }
}
