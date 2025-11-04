

using Fusion.Repository.Bases.Exceptions;
using Fusion.Repository.Data;
using Fusion.Repository.Entities;
using Fusion.Repository.IRepositories;
using Microsoft.EntityFrameworkCore;

namespace Fusion.Repository.Repositories
{

    public class ProjectRepository : GenericRepository<Project>, IProjectRepository
    {
        private readonly FusionDbContext _ctx;
        public ProjectRepository(FusionDbContext ctx) : base(ctx) => _ctx = ctx;

        public Task<bool> IsCodeExistedAsync(Guid companyId, string code, CancellationToken ct = default)
            => _ctx.Projects.AnyAsync(p => p.CompanyId == companyId && p.Code == code, ct);

        public Task<Project?> GetByIdWithSprintsAsync(Guid projectId, CancellationToken ct = default)
            => _ctx.Projects
                    .Include(p => p.Sprints)
                    .FirstOrDefaultAsync(p => p.Id == projectId, ct);
        public Task<int> GetAllProjectCountAsync(CancellationToken cancellationToken = default)
        {
            return _context.Projects
                .AsNoTracking()
                .CountAsync(cancellationToken);
        }
        public async Task<(List<Project> Items, int TotalCount)> GetProjectsForCompanyAsync(
       Guid companyId,
       string? q,
       IEnumerable<string>? statuses,
       string? sort,
       int pageNumber,
       int pageSize,
       CancellationToken ct = default)
        {
            var query = _ctx.Projects
                .AsNoTracking()
                .Include(p => p.Company)
                .Include(p => p.CompanyHired)
                .Include(p => p.Workflow)
                .Where(p => p.CompanyId == companyId);

            // Search
            if (!string.IsNullOrWhiteSpace(q))
            {
                var key = q.Trim().ToLower();
                query = query.Where(p =>
                    (p.Code ?? "").ToLower().Contains(key) ||
                    (p.Name ?? "").ToLower().Contains(key));
            }

            // Status filter
            if (statuses != null && statuses.Any())
            {
                var bag = statuses.Where(s => !string.IsNullOrWhiteSpace(s))
                                  .Select(s => s.Trim())
                                  .ToHashSet(StringComparer.OrdinalIgnoreCase);
                query = query.Where(p => p.Status != null && bag.Contains(p.Status));
            }

            // Sort
            query = (sort ?? "recent").ToLower() switch
            {
                "name" => query.OrderBy(p => p.Name).ThenByDescending(p => p.CreateAt),
                "start" => query.OrderBy(p => p.StartDate).ThenBy(p => p.Name),
                _ => query.OrderByDescending(p => p.UpdateAt).ThenByDescending(p => p.CreateAt)
            };

            var total = await query.CountAsync(ct);

            var page = Math.Max(1, pageNumber);
            var size = Math.Max(1, pageSize);

            var items = await query
                .Skip((page - 1) * size)
                .Take(size)
                .ToListAsync(ct);

            return (items, total);
        }
    }
}
