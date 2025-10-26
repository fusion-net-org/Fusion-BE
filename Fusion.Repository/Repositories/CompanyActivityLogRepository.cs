

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

        public async Task<PagedResult<CompanyActivityLog>> GetPagedLogsByCompanyIdAsync(Guid companyId, Guid userId, CompanyActivityLogPagedSearchRequest request, CancellationToken token = default)
        {
            var company = await _context.Companies
          .AsNoTracking()
          .FirstOrDefaultAsync(c => c.Id == companyId, token);

            if (company == null)
                throw CustomExceptionFactory.CreateNotFoundError("Company not found");

            // 2) Validate user by email
            var user = await _context.Users
                .AsNoTracking()
                .SingleOrDefaultAsync(u => u.Id == userId, token);

            if (user == null)
                throw CustomExceptionFactory.CreateNotFoundError("User not found");

            var isMember = await _context.CompanyMembers
                .AsNoTracking()
                .AnyAsync(cm => cm.UserId == user.Id && cm.CompanyId == companyId, token);

            if (!isMember)
                throw CustomExceptionFactory.CreateNotFoundError($"User can not access to {company.Name}");

            // 4) Base query
            var query = _context.CompanyActivityLogs
                .AsNoTracking()
                .Where(l => l.CompanyId == companyId && !l.IsDeleted);

            if (!string.IsNullOrWhiteSpace(request?.KeyWord))
            {
                var keyword = request.KeyWord.Trim().ToLower();
                query = query.Where(l =>
                    (l.Title ?? "").ToLower().Contains(keyword) ||
                    (l.Description ?? "").ToLower().Contains(keyword));
            }

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

            query = query.OrderByDescending(l => l.CreatedAt);

            return await query.ToPagedResultAsync(request!, token);
        }
    }
}
