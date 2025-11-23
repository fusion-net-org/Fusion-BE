using Fusion.Repository.Bases.Page;
using Fusion.Repository.Bases.Page.Task;
using Fusion.Repository.Data;
using Fusion.Repository.Entities;
using Fusion.Repository.IRepositories;
using Microsoft.EntityFrameworkCore;
using System.Threading;

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

        public async Task<PagedResult<ProjectTask>> GetAllTaskByUserId(Guid userId, TaskFilterRequest request, CancellationToken token = default)
        {
            var taskIds = await _db.TaskWorkflows
                .Where(a => a.AssignUserId == userId)
                .Select(a => a.TaskId)
                .ToListAsync();

            var query = _db.ProjectTasks
                .Where(t => !t.IsDeleted && taskIds.Contains(t.Id))
                .Include(t => t.Project)
                    .ThenInclude(t => t.Company)
                .Include(t => t.Project)
                    .ThenInclude(p => p.CompanyRequest)
                .Include(t => t.Project)
                    .ThenInclude(p => p.ProjectRequest)
                .Include(t => t.Sprint)
                .Include(t => t.CurrentStatus)
                .Include(t => t.CreatedByNavigation)
                .Include(t => t.TaskWorkflows)
                    .ThenInclude(a => a.AssignUser)
                .Include(t => t.Comments)
                .Include(t => t.ChecklistItems)
                .Include(t => t.Dependencies)
                    .ThenInclude(d => d.DependsOnTask)
                .AsQueryable();


            if (request.DateRange != null)
            {
                if (request.DateRange.From != null)
                    query = query.Where(x => x.DueDate >= request.DateRange.From.Value.ToDateTime(TimeOnly.MinValue));

                if (request.DateRange.To != null)
                    query = query.Where(x => x.DueDate <= request.DateRange.To.Value.ToDateTime(TimeOnly.MaxValue));
            }

            if (request.Type.HasValue)
                query = query.Where(x => x.Type == request.Type.ToString());

            if (request.Priority.HasValue)
                query = query.Where(x => x.Priority == request.Priority.ToString());

            if (!string.IsNullOrWhiteSpace(request.Keyword))
            {
                var keyword = request.Keyword.Trim();
                query = query.Where(x =>
                    x.Title.Contains(keyword) ||
                    x.Code.Contains(keyword)
                );
            }

            if (request.ProjectId.HasValue)
                query = query.Where(x => x.ProjectId == request.ProjectId);

            if (request.SprintId.HasValue)
                query = query.Where(x => x.SprintId == request.SprintId);

            if (request.StatusId.HasValue)
                query = query.Where(x => x.CurrentStatusId == request.StatusId);

            if (request.OverDue == true)
                query = query.Where(x => x.DueDate < DateTime.UtcNow && !x.CurrentStatus.IsEnd);

            return await query.ToPagedResultAsync(request, token);

        }
    }
}
