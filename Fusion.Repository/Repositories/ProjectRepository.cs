
using Fusion.Repository.Bases.Page;
using Fusion.Repository.Bases.Page.Project;
using Fusion.Repository.Data;
using Fusion.Repository.Entities;
using Fusion.Repository.IRepositories;
using Microsoft.EntityFrameworkCore;

namespace Fusion.Repository.Repositories
{
    public class ProjectRepository : GenericRepository<Project>, IProjectRepository
    {
        private readonly FusionDbContext _context;
        public ProjectRepository(FusionDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<Project> CreateProjectAsync(Guid userId, Project request, CancellationToken cancellationToken = default)
        {
            var now = DateTime.UtcNow;
            request.CreateAt = now;
            request.UpdateAt = now;
            request.CreatedBy = userId;

            await _context.Projects.AddAsync(request, cancellationToken);
            try
            {
                await _context.SaveChangesAsync(cancellationToken);
                return request;
            }
            catch (DbUpdateException ex)
            {
                var detail = ex.InnerException?.Message ?? ex.Message;
                throw;
            }
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

        public async Task<(int Todo, int Cancel, int Finish)> GetCountProjectByStatusAsync(CancellationToken ct = default)
        {
            var row = await _context.Projects
                 .AsNoTracking()
                 .GroupBy(_ => 1)
                 .Select(g => new
                 {
                     Todo = g.Count(p => (p.Status ?? "").Trim().ToLower() == "todo"),
                     Cancel = g.Count(p => (p.Status ?? "").Trim().ToLower() == "cancel"),
                     Finish = g.Count(p => (p.Status ?? "").Trim().ToLower() == "finish"),
                 })
                 .FirstOrDefaultAsync(ct);

            return (row?.Todo ?? 0, row?.Cancel ?? 0, row?.Finish ?? 0);
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
        public Task<int> GetAllProjectCountAsync(CancellationToken cancellationToken = default)
        {
            return _context.Projects
                  .AsNoTracking()
                  .CountAsync(cancellationToken);
        }
    }
}
