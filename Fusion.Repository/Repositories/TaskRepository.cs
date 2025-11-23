using Fusion.Repository.Bases.Page;
using Fusion.Repository.Bases.Page.Task;
using Fusion.Repository.Data;
using Fusion.Repository.Entities;
using Fusion.Repository.IRepositories;
using Microsoft.EntityFrameworkCore;

namespace Fusion.Repository.Repositories
{

    public class TaskRepository : ITaskRepository
    {
        private readonly FusionDbContext _db;
        public TaskRepository(FusionDbContext db) => _db = db;

        public async Task<ProjectTask> AddAsync(ProjectTask entity, CancellationToken ct = default)
        {
            _db.ProjectTasks.Add(entity);
            await _db.SaveChangesAsync(ct);
            return entity;
        }
        public async Task<bool> ExistsAsync(Guid id, CancellationToken ct = default)
        {
            return await _db.ProjectTasks
                .AsNoTracking()
                .AnyAsync(t => t.Id == id && !t.IsDeleted, ct);
        }

        public async Task<ProjectTask?> FindByIdAsync(Guid id, CancellationToken ct = default)
        {
            return await _db.ProjectTasks
                .Include(t => t.Assignees)
                .Include(t => t.Project)
                .Include(t => t.Sprint)
                .FirstOrDefaultAsync(t => t.Id == id, ct);
        }

        public async Task<PagedResult<ProjectTask>> GetAllAsync(PagedRequest request, CancellationToken ct = default)
        {
            var q = _db.ProjectTasks
                .AsNoTracking()
                .Where(t => !t.IsDeleted);

            // (tuỳ ý: filter theo Project/Sprint/Status…)
            return await q.ToPagedResultAsync(request, ct);
        }

        public async Task<ProjectTask> UpdateAsync(ProjectTask entity, CancellationToken ct = default)
        {
            _db.ProjectTasks.Update(entity);
            await _db.SaveChangesAsync(ct);
            return entity;
        }

        public async Task<bool> SoftDeleteAsync(Guid id, CancellationToken ct = default)
        {
            var e = await _db.ProjectTasks.FirstOrDefaultAsync(x => x.Id == id, ct);
            if (e == null) return false;
            e.IsDeleted = true;
            e.UpdateAt = DateTime.UtcNow;
            _db.ProjectTasks.Update(e);
            await _db.SaveChangesAsync(ct);
            return true;
        }
        public async Task<PagedResult<ProjectTask>> GetTasksBySprintIdAsync(
               Guid sprintId,
               TaskBySprintRequest request,
               CancellationToken ct = default)
        {
            var query = _db.ProjectTasks
                .Include(t => t.Assignees)
                .Include(t => t.Project)
                .Include(t => t.Sprint)
                .Where(t => t.SprintId == sprintId && !t.IsDeleted)
                .AsQueryable();

            // filter Title
            if (!string.IsNullOrWhiteSpace(request.Title))
            {
                var keyword = request.Title.Trim();
                query = query.Where(t =>
                    t.Title.Contains(keyword) ||
                    t.Type.Contains(keyword));
            }


            // filter Status
            if (!string.IsNullOrWhiteSpace(request.Status))
                query = query.Where(t => t.Status == request.Status);

            // filter Priority
            if (!string.IsNullOrWhiteSpace(request.Priority))
                query = query.Where(t => t.Priority == request.Priority);

            // filter CreatedAt
            if (request.CreatedFrom.HasValue && request.CreatedTo.HasValue)
            {
                var from = request.CreatedFrom.Value.Date;
                var to = request.CreatedTo.Value.Date.AddDays(1).AddTicks(-1);

                query = query.Where(x => x.CreateAt >= from && x.CreateAt <= to);
            }
            else if (request.CreatedFrom.HasValue)
            {
                var from = request.CreatedFrom.Value.Date;
                query = query.Where(x => x.CreateAt >= from);
            }
            else if (request.CreatedTo.HasValue)
            {
                var to = request.CreatedTo.Value.Date.AddDays(1).AddTicks(-1);
                query = query.Where(x => x.CreateAt <= to);
            }

            return await query.ToPagedResultAsync(request, ct);
        }
    }
}
