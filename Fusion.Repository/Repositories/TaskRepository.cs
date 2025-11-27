using Fusion.Repository.Bases.Exceptions;
using Fusion.Repository.Bases.Page;
using Fusion.Repository.Bases.Page.Task;
using Fusion.Repository.Data;
using Fusion.Repository.Entities;
using Fusion.Repository.IRepositories;
using Fusion.Repository.ViewModels.Users;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

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
                .Include(t => t.TaskWorkflows)
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
            var user = await _db.Users.FindAsync(userId);

            if (user == null)
                throw CustomExceptionFactory.CreateNotFoundError("User Not existed");

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

        public async Task<ProjectTask> GetTaskDetailByTaskIdAsync(Guid userId, Guid taskId, CancellationToken token = default)
        {
            var user = await _db.Users.FindAsync(userId);

            if (user == null)
                throw CustomExceptionFactory.CreateNotFoundError("User Not existed");

            var task = await _db.ProjectTasks
                .Where(t => !t.IsDeleted)
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
                .Include(t => t.Attachments)
                .Include(t => t.Dependencies)
                    .ThenInclude(d => d.DependsOnTask)
                .SingleOrDefaultAsync(t => t.Id == taskId, token);

            if (task == null)
                throw CustomExceptionFactory.CreateNotFoundError("Task is not existed");

            var projectMember = await _db.ProjectMembers.SingleOrDefaultAsync(pm => pm.UserId == userId && pm.ProjectId == task.ProjectId);

            if (projectMember == null)
                throw CustomExceptionFactory.CreateNotFoundError("User is not belong to this project.");

            return task;

        }

        public async Task<List<Guid>> GetMemberIdByTaskId(Guid taskId, CancellationToken token = default)
        {

            var task = await _db.ProjectTasks
                .Where(t => !t.IsDeleted)
                .Include(t => t.TaskWorkflows)
                .SingleOrDefaultAsync(t => t.Id == taskId, token);

            if (task == null)
                throw CustomExceptionFactory.CreateNotFoundError("Task Not Found");

            return task.TaskWorkflows
                .Where(w => w.AssignUserId != null)
                .Select(w => w.AssignUserId.Value)
                .Distinct()
                .ToList();

        }

        public async Task<List<ProjectTask>> GetSubTasksByTaskIdAsync(Guid userId, Guid taskId, CancellationToken token = default)
        {
            var user = await _db.Users.FindAsync(userId);

            if (user == null)
                throw CustomExceptionFactory.CreateNotFoundError("User Not existed");

            var parentTask = await _db.ProjectTasks
                .Include(t => t.TaskWorkflows)
                .SingleOrDefaultAsync(t => !t.IsDeleted && t.Id == taskId, token);

            if (parentTask == null)
                throw CustomExceptionFactory.CreateNotFoundError($"Task with Id {taskId} not found.");

            var projectMember = await _db.ProjectMembers.SingleOrDefaultAsync(pm => pm.UserId == userId && pm.ProjectId == parentTask.ProjectId);

            if (projectMember == null)
                throw CustomExceptionFactory.CreateNotFoundError("User is not belong to this project.");

            var subTasks = await _db.ProjectTasks
                .Where(t => !t.IsDeleted && t.ParentTaskId == taskId)
                .Include(t => t.Project)
                .Include(t => t.Sprint)
                .Include(t => t.TaskWorkflows)
                    .ThenInclude(wf => wf.AssignUser)
                .Include(t => t.ChecklistItems)
                .Include(t => t.Attachments)
                .Include(t => t.Dependencies)
                    .ThenInclude(d => d.DependsOnTask)
                .ToListAsync(token);

            return subTasks;
        }

        public async Task<List<ProjectTask>> GetTasksAssignedToUserAsync(Guid userId, CancellationToken token = default)
        {
            var user = await _db.Users.FindAsync(userId);

            if (user == null)
                throw CustomExceptionFactory.CreateNotFoundError("User Not existed");


            var taskIds = await _db.TaskWorkflows
                .Where(a => a.AssignUserId == userId)
                .Select(a => a.TaskId)
                .ToListAsync();

            return await _db.ProjectTasks
                .Include(t => t.TaskWorkflows)
                .Where(t => !t.IsDeleted && taskIds.Contains(t.Id))
                .OrderByDescending(t => t.CreateAt)
                .ToListAsync(token);
        }

        public async Task<UserTaskDashBoard> GetUserTaskDashboardAsync(Guid userId, CancellationToken token = default)
        {
            var user = await _db.Users.FindAsync(userId);

            if (user == null)
                throw CustomExceptionFactory.CreateNotFoundError("User Not existed");

            var taskIds = await _db.TaskWorkflows
                .Where(a => a.AssignUserId == userId)
                .Select(a => a.TaskId)
                .ToListAsync();

            var tasks = await _db.ProjectTasks
                .Include(t => t.TaskWorkflows)
                .Where(t => !t.IsDeleted && taskIds.Contains(t.Id))
                .ToListAsync(token);

            var total = tasks.Count;
            var totalTasks = total > 0 ? total : 1;

            // % Task type
            var bugPercent = tasks.Count(t => t.Type == "Bug") * 100.0 / totalTasks;
            var featurePercent = tasks.Count(t => t.Type == "Feature") * 100.0 / totalTasks;
            var chorePercent = tasks.Count(t => t.Type == "Chore") * 100.0 / totalTasks;

            // % Task trạng thái
            var overduePercent = tasks.Count(t => t.Status != "Done" && t.DueDate < DateTime.UtcNow.AddHours(7)) * 100.0 / totalTasks;
            var onTimePercent = tasks.Count(t => t.Status != "Done" && t.DueDate >= DateTime.UtcNow.AddHours(7)) * 100.0 / totalTasks;
            var earlyCompletedPercent = tasks.Count(t => t.Status == "Done" && t.UpdateAt <= t.DueDate)* 100.0 / totalTasks;

            return new UserTaskDashBoard
            {
                BugPercent = bugPercent,
                FeaturePercent = featurePercent,
                ChorePercent = chorePercent,
                OverduePercent = overduePercent,
                OnTimePercent = onTimePercent,
                EarlyCompletedPercent = earlyCompletedPercent
            };
        }

    }
}
