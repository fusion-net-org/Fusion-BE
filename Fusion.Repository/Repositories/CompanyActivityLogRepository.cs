

using Fusion.Repository.Bases.Exceptions;
using Fusion.Repository.Bases.Page;
using Fusion.Repository.Bases.Page.CompanyActivityLog;
using Fusion.Repository.Data;
using Fusion.Repository.Entities;
using Fusion.Repository.IRepositories;
using Microsoft.EntityFrameworkCore;
using System.Linq.Dynamic.Core.Tokenizer;

namespace Fusion.Repository.Repositories
{
    public class CompanyActivityLogRepository : GenericRepository<CompanyActivityLog>, ICompanyActivityLogRepository
    {
        private readonly FusionDbContext _context;
        public CompanyActivityLogRepository(FusionDbContext context) : base(context)
        {
            _context = context;
        }


        public async Task<CompanyActivityLog?> GetLogByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.CompanyActivityLogs
          .AsNoTracking()
          .FirstOrDefaultAsync(l => l.Id == id && !l.IsDeleted, default);
        }

        public async Task<PagedResult<CompanyActivityLog>> GetPagedLogsByCompanyIdAsync(
     Guid companyId,
     Guid userId,
     CompanyActivityLogPagedSearchRequest request,
     CancellationToken token = default)
        {
            // 1) Company & User
            var company = await _context.Companies
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == companyId, token);
            if (company == null)
                throw CustomExceptionFactory.CreateNotFoundError("Company not found");

            var user = await _context.Users
                .AsNoTracking()
                .SingleOrDefaultAsync(u => u.Id == userId, token);
            if (user == null)
                throw CustomExceptionFactory.CreateNotFoundError("User not found");

            // 2) Các công ty user đang là member
            var myCompanyIds = await _context.CompanyMembers
                .AsNoTracking()
                .Where(cm => cm.UserId == user.Id && cm.CompanyId != null && cm.IsDeleted != true)
                .Select(cm => cm.CompanyId!.Value)
                .ToListAsync(token);

            var isMember = myCompanyIds.Contains(companyId);

            // 3) Nếu không phải member, kiểm tra friend-access 2 chiều
            bool hasFriendAccess = false;
            if (!isMember && myCompanyIds.Count > 0)
            {
                const string Accepted = "Accepted"; // đổi theo chuẩn trạng thái của bạn nếu khác
                hasFriendAccess = await _context.CompanyFriendships
                    .AsNoTracking()
                    .AnyAsync(cf =>
                        cf.Status == Accepted &&
                        (
                            (cf.CompanyAId == companyId && cf.CompanyBId != null && myCompanyIds.Contains(cf.CompanyBId.Value)) ||
                            (cf.CompanyBId == companyId && cf.CompanyAId != null && myCompanyIds.Contains(cf.CompanyAId.Value))
                        ),
                        token);
            }

            // 4) Không member và không friend => 403
            if (!isMember && !hasFriendAccess)
                throw CustomExceptionFactory.CreateForbiddenError();

            // 5) Base query
            var query = _context.CompanyActivityLogs
                .AsNoTracking()
                .Where(l => l.CompanyId == companyId && !l.IsDeleted);

            // 6) Friend (không phải member) => chỉ xem public (IsView = true)
            if (!isMember && hasFriendAccess)
            {

                query = query.Where(l => l.IsView);
            }

            // 7) Keyword
            if (!string.IsNullOrWhiteSpace(request?.KeyWord))
            {
                var keyword = request.KeyWord.Trim().ToLower();
                query = query.Where(l =>
                    (l.Title ?? string.Empty).ToLower().Contains(keyword) ||
                    (l.Description ?? string.Empty).ToLower().Contains(keyword));
            }

            // 8) Date range
            if (request?.DateRange != null)
            {
                if (request.DateRange.From.HasValue && request.DateRange.To.HasValue)
                {
                    var from = request.DateRange.From.Value.ToDateTime(TimeOnly.MinValue);
                    var to = request.DateRange.To.Value.ToDateTime(TimeOnly.MaxValue);
                    query = query.Where(l => l.CreatedAt >= from && l.CreatedAt <= to);
                }
                else if (request.DateRange.From.HasValue)
                {
                    var from = request.DateRange.From.Value.ToDateTime(TimeOnly.MinValue);
                    query = query.Where(l => l.CreatedAt >= from);
                }
                else if (request.DateRange.To.HasValue)
                {
                    var to = request.DateRange.To.Value.ToDateTime(TimeOnly.MaxValue);
                    query = query.Where(l => l.CreatedAt <= to);
                }
            }

            // 9) Sort
            query = query.OrderByDescending(l => l.CreatedAt);

            // 10) Return
            return await query.ToPagedResultAsync(request!, token);
        }

        public async Task<bool> UpdateIsViewLog(bool isView, Guid companyId, Guid userId, CancellationToken cancellationToken = default)
        {
            var company = await _context.Companies
           .AsNoTracking()
           .FirstOrDefaultAsync(c => c.Id == companyId, cancellationToken);

            if (company == null)
                throw CustomExceptionFactory.CreateNotFoundError("Company not found");

            var user = await _context.Users
             .AsNoTracking()
             .SingleOrDefaultAsync(u => u.Id == userId, cancellationToken);

            if (user == null)
                throw CustomExceptionFactory.CreateNotFoundError("User not found");

            var isMember = await _context.CompanyMembers
             .AsNoTracking()
             .AnyAsync(cm => cm.UserId == user.Id && cm.CompanyId == companyId, cancellationToken);

            if (!isMember)
                throw CustomExceptionFactory.CreateNotFoundError($"User can not access to {company.Name}");

            // ----Owner - only----
            //  var isOwner =
            //   await _context.Companies.AsNoTracking()
            //    .AnyAsync(c => c.Id == companyId && c.OwnerUserId == userId, cancellationToken)
            //|| await _context.CompanyMembers.AsNoTracking()
            //    .AnyAsync(m => m.CompanyId == companyId && m.UserId == userId && , cancellationToken);

            //    if (!isOwner)
            //        throw CustomExceptionFactory.CreateForbiddenError("Only the company owner can change all log visibility.");


            var affected = await _context.CompanyActivityLogs
                          .Where(l => l.CompanyId == companyId && !l.IsDeleted && l.IsView != isView)
                          .ExecuteUpdateAsync(setters => setters
                          .SetProperty(l => l.IsView, _ => isView)
                          .SetProperty(l => l.UpdateAt, _ => DateTime.UtcNow),
                          cancellationToken);
            return true;
        }
      
    }
}
