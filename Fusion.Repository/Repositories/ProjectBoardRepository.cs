using Fusion.Repository.Data;
using Fusion.Repository.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Fusion.Repository.Repositories
{
    // Fusion.Repository/Repositories/ProjectBoardRepository.cs
    public interface IProjectBoardRepository
    {
        Task<Project?> GetProjectWithWorkflowAsync(Guid projectId, CancellationToken ct = default);

        Task<List<Sprint>> GetSprintsAsync(Guid projectId, Guid? sprintId, bool includeClosed, DateOnly? from, DateOnly? to, CancellationToken ct = default);
        Task<List<WorkflowStatus>> GetStatusesAsync(Guid workflowId, CancellationToken ct = default);

        Task<List<ProjectTask>> GetTasksForSprintsAsync(Guid projectId, IEnumerable<Guid> sprintIds, CancellationToken ct = default);
        Task<List<TaskWorkflow>> GetAssigneesForTasksAsync(IEnumerable<Guid> taskIds, CancellationToken ct = default);

        // ---- write ops ----
        Task<ProjectTask?> GetTaskForMoveAsync(Guid taskId, CancellationToken ct = default);
        Task<bool> HasTransitionAsync(Guid workflowId, Guid fromStatusId, Guid toStatusId, CancellationToken ct = default);
        Task AddTaskWorkflowAsync(TaskWorkflow entity, CancellationToken ct = default);
        Task SaveAsync(CancellationToken ct = default);
        Task<List<WorkflowTransition>> GetTransitionsAsync(Guid workflowId, CancellationToken ct = default);
        Task<List<ProjectComponent>> GetComponentsAsync(Guid projectId, CancellationToken ct = default);


    }


    public class ProjectBoardRepository : IProjectBoardRepository
    {
        private readonly FusionDbContext _db;
        public ProjectBoardRepository(FusionDbContext db) => _db = db;

        public Task<Project?> GetProjectWithWorkflowAsync(Guid projectId, CancellationToken ct = default)
            => _db.Projects.AsNoTracking().Include(p => p.Workflow).SingleOrDefaultAsync(p => p.Id == projectId, ct);
        public Task<List<ProjectComponent>> GetComponentsAsync(Guid projectId, CancellationToken ct = default)
        {
            return _db.ProjectComponents
                .AsNoTracking()
                .Where(c => c.ProjectId == projectId)
                .OrderBy(c => c.Name)
                .ToListAsync(ct);
        }
        public async Task<List<Sprint>> GetSprintsAsync(Guid projectId, Guid? sprintId, bool includeClosed, DateOnly? from, DateOnly? to, CancellationToken ct = default)
        {
            var q = _db.Sprints.AsNoTracking()
                .Where(s => s.ProjectId == projectId && !s.IsDeleted);

            if (sprintId.HasValue) q = q.Where(s => s.Id == sprintId.Value);
            if (!includeClosed) q = q.Where(s => s.Status != Enums.SprintStatus.Closed);
            if (from.HasValue) q = q.Where(s => s.StartDate >= from.Value.ToDateTime(TimeOnly.MinValue));
            if (to.HasValue) q = q.Where(s => s.StartDate <= to.Value.ToDateTime(TimeOnly.MaxValue));

            return await q.OrderBy(s => s.StartDate).ToListAsync(ct);
        }

        public Task<List<WorkflowStatus>> GetStatusesAsync(Guid workflowId, CancellationToken ct = default)
            => _db.WorkflowStatuses.AsNoTracking()
                .Where(s => s.WorkflowId == workflowId)
                .OrderBy(s => s.Position)
                .ToListAsync(ct);
        public Task<List<WorkflowTransition>> GetTransitionsAsync(Guid workflowId, CancellationToken ct = default)
        {
            return _db.WorkflowTransitions
                     .Where(x => x.WorkflowId == workflowId)
                     .ToListAsync(ct);
        }

        public Task<List<ProjectTask>> GetTasksForSprintsAsync(Guid projectId, IEnumerable<Guid> sprintIds, CancellationToken ct = default)
        {
            var set = sprintIds.Distinct().ToList();
            if (set.Count == 0) return Task.FromResult(new List<ProjectTask>());

            return _db.ProjectTasks
        .AsNoTracking()
        .Include(t => t.Ticket).Include(t => t.Component)
        .Where(t =>
            !t.IsDeleted &&
            t.ProjectId == projectId &&
            t.SprintId != null &&
            set.Contains(t.SprintId.Value))
        .ToListAsync(ct);
        }

        public Task<List<TaskWorkflow>> GetAssigneesForTasksAsync(
    IEnumerable<Guid> taskIds,
    CancellationToken ct = default)
        {
            var set = taskIds.Distinct().ToList();
            if (set.Count == 0)
                return Task.FromResult(new List<TaskWorkflow>());

            return _db.Set<TaskWorkflow>()
                .AsNoTracking()
                .Include(a => a.AssignUser) // cần User để map name/avatar
                .Where(a => a.TaskId.HasValue && set.Contains(a.TaskId.Value))
                .ToListAsync(ct);
        }


        // write ops giữ nguyên
        public Task<ProjectTask?> GetTaskForMoveAsync(Guid taskId, CancellationToken ct = default)
            => _db.ProjectTasks.Include(t => t.Ticket).Include(t => t.Component).SingleOrDefaultAsync(t => t.Id == taskId && !t.IsDeleted, ct);

        public Task<bool> HasTransitionAsync(Guid workflowId, Guid fromStatusId, Guid toStatusId, CancellationToken ct = default)
            => _db.WorkflowTransitions.AsNoTracking()
                .AnyAsync(t => t.WorkflowId == workflowId && t.FromStatusId == fromStatusId && t.ToStatusId == toStatusId, ct);

        public Task AddTaskWorkflowAsync(TaskWorkflow entity, CancellationToken ct = default)
            => _db.TaskWorkflows.AddAsync(entity, ct).AsTask();

        public Task SaveAsync(CancellationToken ct = default) => _db.SaveChangesAsync(ct);
    }
}
