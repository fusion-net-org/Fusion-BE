using Fusion.Repository.Bases.Page;
using Fusion.Repository.Bases.Page.UserLog;
using Fusion.Repository.Data;
using Fusion.Repository.Entities;
using Fusion.Repository.IRepositories;
using Microsoft.EntityFrameworkCore;

namespace Fusion.Repository.Repositories;

public class UserLogRepository : GenericRepository<UserLog>, IUserLogRepository
{
    private readonly FusionDbContext _context;

    public UserLogRepository(FusionDbContext context) : base(context)
    {
        _context = context;
    }

    public async Task<PagedResult<UserLog>> GetAllUserLogAsync(
        UserLogSearchRequest request,
        CancellationToken cancellationToken = default)
    {
        request ??= new UserLogSearchRequest();

        // default sort (CreatedAt desc) nếu caller không truyền
        if (string.IsNullOrWhiteSpace(request.SortColumn))
        {
            request.SortColumn = nameof(UserLog.CreatedAt);
            request.SortDescending = true;
        }

        var query = _context.UserLogs
            .AsNoTracking()
            .Where(x => !x.IsDeleted);

        query = ApplyFilters(query, request);

        return await query.ToPagedResultAsync(request, cancellationToken);
    }

    public async Task<PagedResult<UserLog>> GetUserLogByIdAsync(
        Guid actorUserId,
        UserLogSearchRequest request,
        CancellationToken cancellationToken = default)
    {
        request ??= new UserLogSearchRequest();

        if (string.IsNullOrWhiteSpace(request.SortColumn))
        {
            request.SortColumn = nameof(UserLog.CreatedAt);
            request.SortDescending = true;
        }

        var query = _context.UserLogs
            .AsNoTracking()
            .Where(x => !x.IsDeleted && x.ActorUserId == actorUserId);

        query = ApplyFilters(query, request);

        return await query.ToPagedResultAsync(request, cancellationToken);
    }

    /* ==================== Helpers ==================== */
    private static IQueryable<UserLog> ApplyFilters(IQueryable<UserLog> query, UserLogSearchRequest request)
    {
        // Keyword: tìm trong Title/Description
        if (!string.IsNullOrWhiteSpace(request.Keyword))
        {
            var kw = request.Keyword.Trim().ToLower();
            query = query.Where(x =>
                ((x.Title ?? "").ToLower().Contains(kw)) ||
                ((x.Description ?? "").ToLower().Contains(kw)));
        }

        // Date range: DateOnly -> UTC DateTime, [from, to+1d)
        if (request.DateRange is not null)
        {
            if (request.DateRange.From.HasValue)
            {
                var fromUtc = DateTime.SpecifyKind(
                    request.DateRange.From.Value.ToDateTime(TimeOnly.MinValue),
                    DateTimeKind.Utc);
                query = query.Where(x => x.CreatedAt >= fromUtc);
            }

            if (request.DateRange.To.HasValue)
            {
                var toUtcExclusive = DateTime.SpecifyKind(
                    request.DateRange.To.Value.ToDateTime(TimeOnly.MinValue),
                    DateTimeKind.Utc).AddDays(1);
                query = query.Where(x => x.CreatedAt < toUtcExclusive);
            }
        }

        return query;
    }
}
