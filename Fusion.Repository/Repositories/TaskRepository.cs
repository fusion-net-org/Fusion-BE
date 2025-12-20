using Fusion.Repository.Bases.Exceptions;
using Fusion.Repository.Bases.Page;
using Fusion.Repository.Bases.Page.Task;
using Fusion.Repository.Data;
using Fusion.Repository.Entities;
using Fusion.Repository.IRepositories;
using Fusion.Repository.ViewModels.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace Fusion.Repository.Repositories
{
    public class TaskRepository : ITaskRepository
    {
        private readonly FusionDbContext _db;
        public TaskRepository(FusionDbContext db) => _db = db;

        // ===================== EXISTING CRUD =====================
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
                .Include(t => t.TaskWorkflows).ThenInclude(x => x.AssignUser)
                .Include(t => t.CurrentStatus)
                .Include(t => t.CreatedByNavigation)
                .Include(t => t.Comments)
                .Include(t => t.ChecklistItems)
                .Include(t => t.Attachments)
                .Include(t => t.Dependencies).ThenInclude(d => d.DependsOnTask)
                .FirstOrDefaultAsync(t => t.Id == id && !t.IsDeleted, ct);
        }

        public async Task<PagedResult<ProjectTask>> GetAllAsync(PagedRequest request, CancellationToken ct = default)
        {
            var q = _db.ProjectTasks.AsNoTracking().Where(t => !t.IsDeleted);
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

        public async Task<PagedResult<ProjectTask>> GetTasksBySprintIdAsync(Guid sprintId, TaskBySprintRequest request, CancellationToken ct = default)
        {
            var query = _db.ProjectTasks
                .Include(t => t.TaskWorkflows)
                .Include(t => t.Project)
                .Include(t => t.Sprint)
                .Where(t => t.SprintId == sprintId && !t.IsDeleted)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(request.Title))
            {
                var keyword = request.Title.Trim();
                query = query.Where(t => t.Title.Contains(keyword) || t.Type.Contains(keyword));
            }

            if (!string.IsNullOrWhiteSpace(request.Status))
                query = query.Where(t => t.Status == request.Status);

            if (!string.IsNullOrWhiteSpace(request.Priority))
                query = query.Where(t => t.Priority == request.Priority);

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
            var user = await _db.Users.FindAsync(new object?[] { userId }, token);
            if (user == null) throw CustomExceptionFactory.CreateNotFoundError("User Not existed");

            var taskIds = await _db.TaskWorkflows
                .Where(a => a.AssignUserId == userId)
                .Select(a => a.TaskId)
                .ToListAsync(token);

            var query = _db.ProjectTasks
                .Where(t => !t.IsDeleted && taskIds.Contains(t.Id))
                .Include(t => t.Project).ThenInclude(t => t.Company)
                .Include(t => t.Project).ThenInclude(p => p.CompanyRequest)
                .Include(t => t.Project).ThenInclude(p => p.ProjectRequest)
                .Include(t => t.Sprint)
                .Include(t => t.CurrentStatus)
                .Include(t => t.CreatedByNavigation)
                .Include(t => t.TaskWorkflows).ThenInclude(a => a.AssignUser)
                .Include(t => t.Comments)
                .Include(t => t.ChecklistItems)
                .Include(t => t.Dependencies).ThenInclude(d => d.DependsOnTask)
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
                query = query.Where(x => x.Title.Contains(keyword) || x.Code.Contains(keyword));
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
            var user = await _db.Users.FindAsync(new object?[] { userId }, token);
            if (user == null) throw CustomExceptionFactory.CreateNotFoundError("User Not existed");

            var task = await _db.ProjectTasks
                .Where(t => !t.IsDeleted)
                .Include(t => t.Project).ThenInclude(t => t.Company)
                .Include(t => t.Project).ThenInclude(p => p.CompanyRequest)
                .Include(t => t.Project).ThenInclude(p => p.ProjectRequest)
                .Include(t => t.Sprint)
                .Include(t => t.CurrentStatus)
                .Include(t => t.CreatedByNavigation)
                .Include(t => t.TaskWorkflows).ThenInclude(a => a.AssignUser)
                .Include(t => t.Comments)
                .Include(t => t.ChecklistItems)
                .Include(t => t.Attachments)
                .Include(t => t.Dependencies).ThenInclude(d => d.DependsOnTask)
                .SingleOrDefaultAsync(t => t.Id == taskId, token);

            if (task == null) throw CustomExceptionFactory.CreateNotFoundError("Task is not existed");

            var projectMember = await _db.ProjectMembers
                .AsNoTracking()
                .SingleOrDefaultAsync(pm => pm.UserId == userId && pm.ProjectId == task.ProjectId, token);

            if (projectMember == null)
                throw CustomExceptionFactory.CreateNotFoundError("User is not belong to this project.");

            return task;
        }

        public async Task<ProjectTask> GetTaskDetailForAdminByTaskIdAsync(Guid userId, Guid taskId, CancellationToken token = default)
        {
            var user = await _db.Users.FindAsync(new object?[] { userId }, token);
            if (user == null) throw CustomExceptionFactory.CreateNotFoundError("User Not existed");
            if (!user.IsSystemAdmin) throw CustomExceptionFactory.CreateBadRequestError("User is not system admin");

            var task = await _db.ProjectTasks
                .Include(t => t.Project).ThenInclude(t => t.Company)
                .Include(t => t.Project).ThenInclude(p => p.CompanyRequest)
                .Include(t => t.Project).ThenInclude(p => p.ProjectRequest)
                .Include(t => t.Sprint)
                .Include(t => t.CurrentStatus)
                .Include(t => t.CreatedByNavigation)
                .Include(t => t.TaskWorkflows).ThenInclude(a => a.AssignUser)
                .Include(t => t.Comments)
                .Include(t => t.ChecklistItems)
                .Include(t => t.Attachments)
                .Include(t => t.Dependencies).ThenInclude(d => d.DependsOnTask)
                .SingleOrDefaultAsync(t => t.Id == taskId, token);

            if (task == null) throw CustomExceptionFactory.CreateNotFoundError("Task is not existed");

            //var projectMember = await _db.ProjectMembers
            //    .AsNoTracking()
            //    .SingleOrDefaultAsync(pm => pm.UserId == userId && pm.ProjectId == task.ProjectId, token);

            //if (projectMember == null)
            //    throw CustomExceptionFactory.CreateNotFoundError("User is not belong to this project.");

            return task;
        }

        public async Task<List<Guid>> GetMemberIdByTaskId(Guid taskId, CancellationToken token = default)
        {
            var task = await _db.ProjectTasks
                .Where(t => !t.IsDeleted)
                .Include(t => t.TaskWorkflows)
                .SingleOrDefaultAsync(t => t.Id == taskId, token);

            if (task == null) throw CustomExceptionFactory.CreateNotFoundError("Task Not Found");

            return task.TaskWorkflows
                .Where(w => w.AssignUserId != null)
                .Select(w => w.AssignUserId!.Value)
                .Distinct()
                .ToList();
        }

        public async Task<List<ProjectTask>> GetSubTasksByTaskIdAsync(Guid userId, Guid taskId, CancellationToken token = default)
        {
            var user = await _db.Users.FindAsync(new object?[] { userId }, token);
            if (user == null) throw CustomExceptionFactory.CreateNotFoundError("User Not existed");

            var parentTask = await _db.ProjectTasks
                .Include(t => t.TaskWorkflows)
                .SingleOrDefaultAsync(t => !t.IsDeleted && t.Id == taskId, token);

            if (parentTask == null)
                throw CustomExceptionFactory.CreateNotFoundError($"Task with Id {taskId} not found.");

            var projectMember = await _db.ProjectMembers
                .AsNoTracking()
                .SingleOrDefaultAsync(pm => pm.UserId == userId && pm.ProjectId == parentTask.ProjectId, token);

            if (projectMember == null)
                throw CustomExceptionFactory.CreateNotFoundError("User is not belong to this project.");

            return await _db.ProjectTasks
                .Where(t => !t.IsDeleted && t.ParentTaskId == taskId)
                .Include(t => t.Project)
                .Include(t => t.Sprint)
                .Include(t => t.TaskWorkflows).ThenInclude(wf => wf.AssignUser)
                .Include(t => t.ChecklistItems)
                .Include(t => t.Attachments)
                .Include(t => t.Dependencies).ThenInclude(d => d.DependsOnTask)
                .ToListAsync(token);
        }

        public async Task<List<ProjectTask>> GetTasksAssignedToUserAsync(Guid userId, CancellationToken token = default)
        {
            var user = await _db.Users.FindAsync(new object?[] { userId }, token);
            if (user == null) throw CustomExceptionFactory.CreateNotFoundError("User Not existed");

            var taskIds = await _db.TaskWorkflows
                .Where(a => a.AssignUserId == userId)
                .Select(a => a.TaskId)
                .ToListAsync(token);

            return await _db.ProjectTasks
                .Include(t => t.TaskWorkflows)
                .Where(t => !t.IsDeleted && taskIds.Contains(t.Id))
                .OrderByDescending(t => t.CreateAt)
                .ToListAsync(token);
        }

        public async Task<UserTaskDashBoard> GetUserTaskDashboardAsync(Guid userId, CancellationToken token = default)
        {
            var user = await _db.Users.FindAsync(new object?[] { userId }, token);
            if (user == null) throw CustomExceptionFactory.CreateNotFoundError("User Not existed");

            var taskIds = await _db.TaskWorkflows
                .Where(a => a.AssignUserId == userId)
                .Select(a => a.TaskId)
                .ToListAsync(token);

            var tasks = await _db.ProjectTasks
                .Include(t => t.TaskWorkflows)
                .Where(t => !t.IsDeleted && taskIds.Contains(t.Id))
                .ToListAsync(token);

            var total = tasks.Count;
            var totalTasks = total > 0 ? total : 1;

            var bugPercent = tasks.Count(t => t.Type == "Bug") * 100.0 / totalTasks;
            var featurePercent = tasks.Count(t => t.Type == "Feature") * 100.0 / totalTasks;
            var chorePercent = tasks.Count(t => t.Type == "Chore") * 100.0 / totalTasks;

            var overduePercent = tasks.Count(t => t.Status != "Done" && t.DueDate < DateTime.UtcNow.AddHours(7)) * 100.0 / totalTasks;
            var onTimePercent = tasks.Count(t => t.Status != "Done" && t.DueDate >= DateTime.UtcNow.AddHours(7)) * 100.0 / totalTasks;
            var earlyCompletedPercent = tasks.Count(t => t.Status == "Done" && t.UpdateAt <= t.DueDate) * 100.0 / totalTasks;

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

        public async Task<List<ProjectTask>> GetNonBacklogTasksByTicketIdAsync(Guid ticketId, CancellationToken ct = default)
        {
            return await _db.ProjectTasks
                .AsNoTracking()
                .Where(t => t.TicketId == ticketId && !t.IsBacklog && !t.IsDeleted)
                .Include(t => t.CurrentStatus)
                .ToListAsync(ct);
        }

        // ===================== UNIT OF WORK / TRANSACTION =====================
        public Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken ct = default)
            => _db.Database.BeginTransactionAsync(ct);

        public Task<int> SaveChangesAsync(CancellationToken ct = default)
            => _db.SaveChangesAsync(ct);

        // ===================== USERS =====================
        public Task<bool> UserExistsAsync(Guid userId, CancellationToken ct = default)
            => _db.Users.AsNoTracking().AnyAsync(u => u.Id == userId, ct);

        public async Task<string?> GetUserNameAsync(Guid userId, CancellationToken ct = default)
            => (await _db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId, ct))?.UserName;

        public async Task<Dictionary<Guid, (string? UserName, string? Avatar)>> GetUsersMiniAsync(IEnumerable<Guid> userIds, CancellationToken ct = default)
        {
            var ids = userIds?.Distinct().ToList() ?? new List<Guid>();
            if (ids.Count == 0) return new();

            var rows = await _db.Users.AsNoTracking()
                .Where(u => ids.Contains(u.Id))
                .Select(u => new { u.Id, u.UserName, u.Avatar })
                .ToListAsync(ct);

            return rows.ToDictionary(x => x.Id, x => ((string?)x.UserName, (string?)x.Avatar));
        }

        // ===================== PROJECTS =====================
        public Task<Project?> GetProjectByIdAsync(Guid projectId, CancellationToken ct = default)
            => _db.Projects.AsNoTracking().FirstOrDefaultAsync(p => p.Id == projectId, ct);

        public Task<bool> ProjectExistsAsync(Guid projectId, CancellationToken ct = default)
            => _db.Projects.AsNoTracking().AnyAsync(p => p.Id == projectId, ct);

        public async Task<Guid> GetCompanyIdOfProjectAsync(Guid? projectId, CancellationToken ct = default)
        {
            if (!projectId.HasValue || projectId.Value == Guid.Empty)
                throw CustomExceptionFactory.CreateBadRequestError("ProjectId is null.");

            return await _db.Projects.AsNoTracking()
                .Where(p => p.Id == projectId.Value)
                .Select(p => (Guid)p.CompanyId)
                .FirstAsync(ct);
        }

        public async Task<string?> GetProjectCodeAsync(Guid projectId, CancellationToken ct = default)
            => await _db.Projects.AsNoTracking().Where(p => p.Id == projectId).Select(p => p.Code).FirstOrDefaultAsync(ct);

        public async Task<Guid?> GetProjectWorkflowIdAsync(Guid projectId, CancellationToken ct = default)
            => await _db.Projects.AsNoTracking().Where(p => p.Id == projectId).Select(p => p.WorkflowId).FirstOrDefaultAsync(ct);

        // ===================== SPRINTS =====================
        public Task<Sprint?> GetSprintByIdAsync(Guid sprintId, CancellationToken ct = default)
            => _db.Sprints.AsNoTracking().FirstOrDefaultAsync(s => s.Id == sprintId, ct);

        public Task<bool> SprintExistsAsync(Guid sprintId, CancellationToken ct = default)
            => _db.Sprints.AsNoTracking().AnyAsync(s => s.Id == sprintId, ct);

        public Task<bool> SprintBelongsToProjectAsync(Guid sprintId, Guid projectId, CancellationToken ct = default)
            => _db.Sprints.AsNoTracking().AnyAsync(s => s.Id == sprintId && s.ProjectId == projectId, ct);

        public async Task<List<Sprint>> GetSprintsByProjectAsync(Guid projectId, CancellationToken ct = default)
        {
            return await _db.Sprints.AsNoTracking()
                .Where(s => s.ProjectId == projectId && !s.IsDeleted)
                .OrderBy(s => s.StartDate)
                .ToListAsync(ct);
        }

        // ===================== WORKFLOW STATUSES =====================
        public Task<WorkflowStatus?> GetWorkflowStatusByIdAsync(Guid statusId, CancellationToken ct = default)
            => _db.WorkflowStatuses.AsNoTracking().FirstOrDefaultAsync(x => x.Id == statusId, ct);

        public async Task<WorkflowStatus?> FindWorkflowStatusByCodeAsync(string codeOrName, CancellationToken ct = default)
        {
            var key = (codeOrName ?? "").Trim().ToLower();
            if (string.IsNullOrWhiteSpace(key)) return null;

            return await _db.WorkflowStatuses.AsNoTracking()
                .FirstOrDefaultAsync(x =>
                    (x.Code != null && x.Code.ToLower() == key) ||
                    (x.Name != null && x.Name.ToLower() == key),
                    ct);
        }

        public async Task<List<WorkflowStatus>> GetStatusesByWorkflowAsync(Guid workflowId, CancellationToken ct = default)
        {
            return await _db.WorkflowStatuses.AsNoTracking()
                .Where(x => x.WorkflowId == workflowId)
                .OrderBy(x => x.Position)
                .ToListAsync(ct);
        }

        public async Task<WorkflowStatus> ResolveStatusForWorkflowAsync(Guid? statusId, string? codeOrName, Guid workflowId, CancellationToken ct = default)
        {
            var all = await GetStatusesByWorkflowAsync(workflowId, ct);
            if (all.Count == 0)
                throw CustomExceptionFactory.CreateBadRequestError("Workflow has no statuses.");

            if (statusId.HasValue && all.Any(x => x.Id == statusId.Value))
                return all.First(x => x.Id == statusId.Value);

            if (!string.IsNullOrWhiteSpace(codeOrName))
            {
                var key = codeOrName.Trim().ToLower();
                var hit = all.FirstOrDefault(x => (x.Code ?? "").ToLower() == key || (x.Name ?? "").ToLower() == key);
                if (hit != null) return hit;
            }

            return all.FirstOrDefault(x => x.Category == "TODO") ?? all.First();
        }

        // ===================== TASK HELPERS =====================
        public async Task<int> GetNextOrderInSprintAsync(Guid sprintId, Guid statusId, CancellationToken ct = default)
        {
            var max = await _db.ProjectTasks.AsNoTracking()
                .Where(t => t.SprintId == sprintId && t.CurrentStatusId == statusId && !t.IsDeleted)
                .MaxAsync(t => (int?)t.OrderInSprint, ct);

            return (max ?? 0) + 1;
        }

        public async Task<long> CountTasksInProjectAsync(Guid projectId, CancellationToken ct = default)
            => await _db.ProjectTasks.AsNoTracking().LongCountAsync(t => t.ProjectId == projectId, ct);

        public async Task<int> CountChildrenTasksAsync(Guid parentTaskId, CancellationToken ct = default)
            => await _db.ProjectTasks.AsNoTracking().CountAsync(x => x.ParentTaskId == parentTaskId && !x.IsDeleted, ct);

        public async Task<string?> GetTaskTitleAsync(Guid taskId, CancellationToken ct = default)
            => await _db.ProjectTasks.AsNoTracking().Where(x => x.Id == taskId).Select(x => x.Title).FirstOrDefaultAsync(ct);

        public Task<ProjectTask?> GetTaskForUpdateAsync(Guid taskId, CancellationToken ct = default)
            => _db.ProjectTasks.FirstOrDefaultAsync(t => t.Id == taskId && !t.IsDeleted, ct);

        public async Task<List<ProjectTask>> GetTasksInSprintStatusForUpdateAsync(Guid sprintId, Guid statusId, Guid excludeTaskId, CancellationToken ct = default)
        {
            return await _db.ProjectTasks
                .Where(t => t.SprintId == sprintId && t.CurrentStatusId == statusId && !t.IsDeleted && t.Id != excludeTaskId)
                .OrderBy(t => t.OrderInSprint)
                .ToListAsync(ct);
        }

        public Task<bool> IsProjectMemberAsync(Guid userId, Guid projectId, CancellationToken ct = default)
            => _db.ProjectMembers.AsNoTracking().AnyAsync(pm => pm.UserId == userId && pm.ProjectId == projectId, ct);

        // ===================== ATTACHMENTS =====================
        public async Task AddAttachmentsAsync(IEnumerable<ProjectTaskAttachment> attachments, CancellationToken ct = default)
        {
            var list = attachments?.ToList() ?? new List<ProjectTaskAttachment>();
            if (list.Count == 0) return;

            _db.ProjectTaskAttachments.AddRange(list);
            await _db.SaveChangesAsync(ct);
        }

        public async Task<List<ProjectTaskAttachment>> GetTaskAttachmentsAsync(Guid taskId, CancellationToken ct = default)
        {
            return await _db.ProjectTaskAttachments.AsNoTracking()
                .Where(a => a.TaskId == taskId && a.CommentId == null)
                .OrderByDescending(a => a.UploadedAt)
                .ToListAsync(ct);
        }

        public Task<ProjectTaskAttachment?> FindAttachmentAsync(Guid taskId, Guid attachmentId, CancellationToken ct = default)
            => _db.ProjectTaskAttachments.FirstOrDefaultAsync(a => a.Id == attachmentId && a.TaskId == taskId, ct);

        public async Task RemoveAttachmentAsync(ProjectTaskAttachment entity, CancellationToken ct = default)
        {
            _db.ProjectTaskAttachments.Remove(entity);
            await _db.SaveChangesAsync(ct);
        }

        public async Task<List<ProjectTaskAttachment>> GetCommentAttachmentsAsync(IEnumerable<long> commentIds, CancellationToken ct = default)
        {
            var ids = commentIds?.Distinct().ToList() ?? new List<long>();
            if (ids.Count == 0) return new();

            return await _db.ProjectTaskAttachments.AsNoTracking()
                .Where(a => a.CommentId.HasValue && ids.Contains(a.CommentId.Value))
                .ToListAsync(ct);
        }

        // ===================== COMMENTS =====================
        public async Task<List<Comment>> GetCommentsByTaskIdAsync(Guid taskId, CancellationToken ct = default)
        {
            return await _db.Comments.AsNoTracking()
                .Where(c => c.TaskId == taskId)
                .OrderByDescending(c => c.CreateAt)
                .ToListAsync(ct);
        }

        public async Task AddCommentAsync(Comment comment, CancellationToken ct = default)
        {
            _db.Comments.Add(comment);
            await _db.SaveChangesAsync(ct);
        }

        // ===================== TICKETS =====================
        public Task<Ticket?> GetTicketByIdAsync(Guid ticketId, CancellationToken ct = default)
            => _db.Tickets.AsNoTracking()
                .FirstOrDefaultAsync(t => t.Id == ticketId && (!t.IsDeleted.HasValue || !t.IsDeleted.Value), ct);

        public Task<bool> TicketExistsAsync(Guid ticketId, CancellationToken ct = default)
            => _db.Tickets.AsNoTracking()
                .AnyAsync(t => t.Id == ticketId && (!t.IsDeleted.HasValue || !t.IsDeleted.Value), ct);
    }
}
