
using Fusion.Repository.Bases.Page;
using Fusion.Repository.Bases.Page.Project;
using Fusion.Repository.Data;
using Fusion.Repository.Entities;
using Fusion.Repository.IRepositories;
using Fusion.Repository.ViewModels;
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
                .Include(p => p.CompanyRequest)
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
        public async Task<PagedResult<Project>> GetAllProjectAsync(ProjectSearchRequest req, CancellationToken ct = default)
        {
            req ??= new ProjectSearchRequest();
            if (string.IsNullOrWhiteSpace(req.SortColumn))
            {
                req.SortColumn = nameof(Project.CreateAt);
                req.SortDescending = true;
            }

            var q = _context.Projects.AsNoTracking();

            // Filter
            if (!string.IsNullOrWhiteSpace(req.Keyword))
            {
                var kw = req.Keyword.Trim().ToLower();
                q = q.Where(p =>
                    ((p.Name ?? "").ToLower().Contains(kw)) ||
                    ((p.Code ?? "").ToLower().Contains(kw)) ||
                    ((p.Description ?? "").ToLower().Contains(kw)));
            }

            if (req.CompanyId.HasValue)
                q = q.Where(p => p.CompanyId == req.CompanyId.Value);

            if (!string.IsNullOrWhiteSpace(req.Status))
            {
                q = q.Where(p => (p.Status ?? "").Trim().ToLower() == req.Status);
            }

            // Date range on CreateAt (DateOnly -> UTC range)
            if (req.DateRange is { } dr)
            {
                if (dr.From.HasValue)
                {
                    var from = DateTime.SpecifyKind(
                        dr.From.Value.ToDateTime(TimeOnly.MinValue), DateTimeKind.Utc);
                    q = q.Where(p => p.CreateAt >= from);
                }

                if (dr.To.HasValue)
                {
                    var toExclusive = DateTime.SpecifyKind(
                        dr.To.Value.ToDateTime(TimeOnly.MinValue), DateTimeKind.Utc).AddDays(1);
                    q = q.Where(p => p.CreateAt < toExclusive);
                }
            }

            return await q.ToPagedResultAsync(req, ct);

        }
        public async Task<List<StatusCountResponse>> GetCountProjectByStatusAsync(CancellationToken ct = default)
        {
            var rows = await _context.Projects
                        .AsNoTracking()
                        .GroupBy(p => (p.Status ?? "").Trim().ToLower())
                        .Select(g => new StatusCountResponse
                         {
                               Status = string.IsNullOrWhiteSpace(g.Key) ? "(none)" : g.Key,
                               Count = g.Count()
                         })
                         .OrderByDescending(x => x.Count)  
                         .ToListAsync(ct);

            return rows;
        }
        public async Task<PagedResult<Project>> GetProjectByMemberIdAsync(Guid userId, ProjectSearchRequest req, CancellationToken ct = default)
        {
            req ??= new ProjectSearchRequest();
            if (string.IsNullOrWhiteSpace(req.SortColumn))
            {
                req.SortColumn = nameof(Project.CreateAt);
                req.SortDescending = true;
            }

            var q = _context.Projects
                .AsNoTracking()
                .Where(x => x.ProjectMembers.Any(m => m.UserId == userId));

            if (!string.IsNullOrWhiteSpace(req.Keyword))
            {
                var kw = req.Keyword.Trim().ToLower();
                q = q.Where(p =>
                ((p.Name ?? "").ToLower().Contains(kw)) ||
                ((p.Code ?? "").ToLower().Contains(kw)) ||
                ((p.Description ?? "").ToLower().Contains(kw))
                );
            }

            if (req.CompanyId.HasValue)
                q = q.Where(p => p.CompanyId == req.CompanyId.Value);

            if (!string.IsNullOrWhiteSpace(req.Status))
            {
                q = q.Where(p => (p.Status ?? "").Trim().ToLower() == req.Status);
            }

            if (req.DateRange is { } dr)
            {
                if (dr.From.HasValue)
                {
                    var from = DateTime.SpecifyKind(
                        dr.From.Value.ToDateTime(TimeOnly.MinValue), DateTimeKind.Utc);
                    q = q.Where(p => p.CreateAt >= from);
                }

                if (dr.To.HasValue)
                {
                    var toExclusive = DateTime.SpecifyKind(
                        dr.To.Value.ToDateTime(TimeOnly.MinValue), DateTimeKind.Utc).AddDays(1);
                    q = q.Where(p => p.CreateAt < toExclusive);
                }
            }
            return await q.ToPagedResultAsync(req, ct);
        }
        public async Task<PagedResult<Project>> GetProjectByActorIdAsync(Guid userId, ProjectSearchRequest req, CancellationToken ct = default)
        {
            req ??= new ProjectSearchRequest();
            if (string.IsNullOrWhiteSpace(req.SortColumn))
            {
                req.SortColumn = nameof(Project.CreateAt);
                req.SortDescending = true;
            }

            var q = _context.Projects
                .AsNoTracking()
                .Where(x => x.CreatedBy == userId);

            if (!string.IsNullOrWhiteSpace(req.Keyword))
            {
                var kw = req.Keyword.Trim().ToLower();
                q = q.Where(p =>
                ((p.Name ?? "").ToLower().Contains(kw)) ||
                ((p.Code ?? "").ToLower().Contains(kw)) ||
                ((p.Description ?? "").ToLower().Contains(kw))
                );
            }

            if (req.CompanyId.HasValue)
                q = q.Where(p => p.CompanyId == req.CompanyId.Value);

            if (!string.IsNullOrWhiteSpace(req.Status))
            {
                q = q.Where(p => (p.Status ?? "").Trim().ToLower() == req.Status);
            }

            if (req.DateRange is { } dr)
            {
                if (dr.From.HasValue)
                {
                    var from = DateTime.SpecifyKind(
                        dr.From.Value.ToDateTime(TimeOnly.MinValue), DateTimeKind.Utc);
                    q = q.Where(p => p.CreateAt >= from);
                }

                if (dr.To.HasValue)
                {
                    var toExclusive = DateTime.SpecifyKind(
                        dr.To.Value.ToDateTime(TimeOnly.MinValue), DateTimeKind.Utc).AddDays(1);
                    q = q.Where(p => p.CreateAt < toExclusive);
                }
            }
            return await q.ToPagedResultAsync(req, ct);
        }
        public Task<Project?> GetProjectDetailAsync(Guid id, CancellationToken ct = default)
        {
            return _context.Projects
                .AsNoTracking()
                .Where(p => p.Id == id)
                .Include(p => p.Sprints.Where(s => !s.IsDeleted))
                .ThenInclude(s => s.ProjectTasks.Where(s => !s.IsDeleted))
                .Include(p => p.ProjectTasks.Where(s => !s.IsDeleted))
                .Include(c => c.Company.Name)
                .FirstOrDefaultAsync(ct);
        }

        public async Task<PagedResult<Project>> GetProjectsForAdminAsync(ProjectSummarySearchRequest request, CancellationToken cancellationToken = default)
        {
            var query = _context.Projects
        .Include(p => p.CreatedByNavigation)
        .Include(p => p.Company)
        .Include(p => p.CompanyRequest)
        .Include(p => p.Workflow)
        .Include(p => p.ProjectMembers)
        .Include(p => p.Sprints).ThenInclude(s => s.ProjectTasks)
        .AsQueryable();

            // FILTER: companyName
            if (!string.IsNullOrEmpty(request.CompanyName))
            {
                query = query.Where(p =>
                        (p.Company != null && p.Company.Name.Contains(request.CompanyName)) ||
                        (p.CompanyRequest != null && p.CompanyRequest.Name.Contains(request.CompanyName)));
            }

            return await query.ToPagedResultAsync(request, cancellationToken);
        }

        public async Task<Project?> GetProjectsByIdForAdminAsync(Guid projectId, CancellationToken cancellationToken = default)
        {
            var query = await _context.Projects
        .Include(p => p.CreatedByNavigation)
        .Include(p => p.Company)
        .Include(p => p.CompanyRequest)
        .Include(p => p.Workflow)
        .Include(p => p.ProjectMembers)
        .Include(p => p.Sprints).ThenInclude(s => s.ProjectTasks)
        .SingleOrDefaultAsync(x => x.Id == projectId);

            return query;
        }

        public async Task<Project> GetProjectById(Guid projectId, CancellationToken cancellationToken = default)
        {
            var query = await _context.Projects
                        .Include(p => p.Company)
                        .Include(p => p.CompanyRequest)
                        .Include(p => p.CreatedByNavigation)
                        .SingleOrDefaultAsync(x => x.Id == projectId);
            return query;
        }
    }
}
