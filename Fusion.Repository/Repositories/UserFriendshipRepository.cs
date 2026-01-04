using Fusion.Repository.Bases.Page;
using Fusion.Repository.Bases.Page.Friend;
using Fusion.Repository.Common;
using Fusion.Repository.Data;
using Fusion.Repository.Entities;
using Fusion.Repository.IRepositories;
using Fusion.Repository.ViewModels;
using Microsoft.EntityFrameworkCore;


namespace Fusion.Repository.Repositories;

public class UserFriendshipRepository : GenericRepository<UserFriendship>, IUserFriendshipRepository
{
    private readonly FusionDbContext _context;
    public UserFriendshipRepository(FusionDbContext context) : base(context)
    {
        _context = context;
    }


    public Task<UserFriendship?> GetByIdAsync(Guid id, CancellationToken ct = default)
     => _context.Set<UserFriendship>().FirstOrDefaultAsync(x => x.Id == id, ct);

    public Task<UserFriendship?> GetByPairKeyAsync(string pairKey, CancellationToken ct = default)
        => _context.Set<UserFriendship>().FirstOrDefaultAsync(x => x.PairKey == pairKey, ct);

    public Task<List<UserFriendship>> GetPendingSentAsync(Guid requesterId, CancellationToken ct = default)
        => _context.Set<UserFriendship>()
            .Where(x => x.RequesterId == requesterId && x.Status == FriendshipStatus.Pending)
            .OrderByDescending(x => x.RequestedAt)
            .ToListAsync(ct);

    public Task<List<UserFriendship>> GetPendingReceivedAsync(Guid addresseeId, CancellationToken ct = default)
        => _context.Set<UserFriendship>()
            .Where(x => x.AddresseeId == addresseeId && x.Status == FriendshipStatus.Pending)
            .OrderByDescending(x => x.RequestedAt)
            .ToListAsync(ct);

    public Task AddAsync(UserFriendship entity, CancellationToken ct = default)
        => _context.Set<UserFriendship>().AddAsync(entity, ct).AsTask();

    public void Update(UserFriendship entity)
        => _context.Set<UserFriendship>().Update(entity);

    public async Task<PagedResult<FriendLiteResponse>> GetPagedUserFriendsAsync(
    Guid userId,
    UserFriendPagedRequest request,
    CancellationToken ct = default)
    {
        const int Pending = 0;
        const int Accepted = 1;

        // QUAN TRỌNG: khai báo IQueryable để Where/OrderBy gán lại không lỗi
        IQueryable<UserFriendship> query = _dbSet
            .AsNoTracking()
            .Where(x =>
                (x.RequesterId == userId || x.AddresseeId == userId) &&
                x.Status.HasValue &&
                (x.Status == Pending || x.Status == Accepted));

        // Search theo email của "người bên kia"
        if (!string.IsNullOrWhiteSpace(request.Email))
        {
            var keyword = request.Email.Trim();

            query = query.Where(x =>
                (x.RequesterId == userId
                    ? (x.Addressee != null ? (x.Addressee.Email ?? "") : "")
                    : (x.Requester != null ? (x.Requester.Email ?? "") : ""))
                .Contains(keyword));
        }

        // Filter status nếu truyền
        if (request.Status.HasValue)
        {
            if (request.Status.Value == Pending || request.Status.Value == Accepted)
                query = query.Where(x => x.Status == request.Status.Value);
        }

        // Include sau cùng (hoặc thậm chí không cần nếu chỉ project)
        query = query
            .Include(x => x.Requester)
            .Include(x => x.Addressee)
            .OrderBy(x => x.Status)              // Pending trước
            .ThenByDescending(x => x.RequestedAt);

        var projected = query.Select(x => new FriendLiteResponse
        {
            Id = x.Id,
            FriendshipId = x.RequesterId.Value == userId ? x.AddresseeId.Value : x.RequesterId.Value,
            Status = x.Status ?? -1,
            Email = (x.RequesterId == userId ? x.Addressee!.Email : x.Requester!.Email),
            Avatar = (x.RequesterId == userId ? x.Addressee!.Avatar : x.Requester!.Avatar),
            UserName = (x.RequesterId == userId ? x.Addressee!.UserName : x.Requester!.UserName),
        });

        return await projected.ToPagedResultAsync(request, ct);
    }
}


