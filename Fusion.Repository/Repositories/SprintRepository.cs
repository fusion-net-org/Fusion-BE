using Fusion.Repository.Bases.Exceptions;
using Fusion.Repository.Bases.Page.Sprint;


using Fusion.Repository.Data;
using Fusion.Repository.Entities;
using Fusion.Repository.Enums;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace Fusion.Repository.Repositories
{
    public interface ISprintRepository
    {
        Task<bool> IsOverlappedAsync(Guid projectId, DateTime start, DateTime end, Guid? ignoreId, CancellationToken ct);
        Task<Sprint> CreateAsync(Sprint sprint, IEnumerable<Guid>? initialTaskIds, Guid userId, CancellationToken ct);
        Task<Sprint?> GetAsync(Guid sprintId, Guid projectId, CancellationToken ct);
        Task<SprintVmData?> GetVmAsync(Guid sprintId, Guid projectId, CancellationToken ct);
        Task StartAsync(Guid sprintId, Guid projectId, CancellationToken ct);
        Task CompleteAsync(Guid sprintId, Guid projectId, bool carryBacklog, Guid? nextSprintId, CancellationToken ct);
        Task<int> AddTasksAsync(Guid sprintId, Guid projectId, IEnumerable<Guid> taskIds, Guid userId, CancellationToken ct);
        Task<int> RemoveTasksAsync(Guid sprintId, Guid projectId, IEnumerable<Guid> taskIds, CancellationToken ct);
        Task ReorderAsync(Guid sprintId, Guid projectId, IReadOnlyList<(Guid taskId, int order)> orders, CancellationToken ct);
    }
    public class SprintRepository : ISprintRepository
    {
        private readonly FusionDbContext _db;
        public SprintRepository(FusionDbContext db) => _db = db;


        public Task<Sprint?> GetAsync(Guid sprintId, Guid projectId, CancellationToken ct) =>
        _db.Set<Sprint>().FirstOrDefaultAsync(x => x.Id == sprintId && x.ProjectId == projectId, ct);


        public Task<SprintVmData?> GetVmAsync(Guid sprintId, Guid projectId, CancellationToken ct) =>
        _db.Set<Sprint>()
        .Where(x => x.Id == sprintId && x.ProjectId == projectId)
        .Select(x => new SprintVmData
        {
            Id = x.Id,
            ProjectId = x.ProjectId,
            Name = x.Name,
            StartDate = x.StartDate,
            EndDate = x.EndDate,
            Status = x.Status,
            TaskCount = x.ProjectTasks.Count(t => !t.IsDeleted && t.SprintId == x.Id)
        })
        .FirstOrDefaultAsync(ct);
        public async Task<bool> IsOverlappedAsync(Guid projectId, DateTime start, DateTime end, Guid? ignoreId, CancellationToken ct)
        {
            // Cancelled sprints are ignored for overlap; soft-deleted filtered by query filter
            var ignore = ignoreId ?? Guid.Empty;
            return await _db.Set<Sprint>()
            .AnyAsync(x => x.ProjectId == projectId
            && x.Id != ignore
            && x.Status != SprintStatus.Cancelled
            && start < x.EndDate && end > x.StartDate, ct);
        }


        public async Task<Sprint> CreateAsync(Sprint sprint, IEnumerable<Guid>? initialTaskIds, Guid userId, CancellationToken ct)
        {
            await _db.Set<Sprint>().AddAsync(sprint, ct);


            if (initialTaskIds?.Any() == true)
            {
                var tasks = await _db.Set<ProjectTask>()
                .Where(t => t.ProjectId == sprint.ProjectId && initialTaskIds.Contains(t.Id) && !t.IsDeleted)
                .ToListAsync(ct);


                var maxOrder = await _db.Set<ProjectTask>()
                .Where(t => t.SprintId == sprint.Id)
                .Select(t => (int?)t.OrderInSprint).MaxAsync(ct) ?? 0;


                foreach (var t in tasks)
                {
                    t.SprintId = sprint.Id;
                    t.IsBacklog = false;
                    t.OrderInSprint = ++maxOrder;
                    t.UpdateAt = DateTime.UtcNow;
                }
            }


            await _db.SaveChangesAsync(ct); // SaveChanges ở Repository
            return sprint;
        }
        public async Task StartAsync(Guid sprintId, Guid projectId, CancellationToken ct)
        {
            var s = await GetAsync(sprintId, projectId, ct)
                ?? throw CustomExceptionFactory.CreateNotFoundError("Sprint not found");

            if (s.Status != SprintStatus.Planning)
                throw CustomExceptionFactory.CreateBadRequestError("Sprint is not in Planning");

            s.Status = SprintStatus.Active;
            s.UpdateAt = DateTime.UtcNow;
            await _db.SaveChangesAsync(ct);
        }

        public async Task ReorderAsync(
    Guid sprintId,
    Guid projectId,
    IReadOnlyList<(Guid taskId, int order)> orders,
    CancellationToken ct)
        {
            if (orders == null || orders.Count == 0) return;

            var ids = orders.Select(o => o.taskId).Distinct().ToList();

            var tasks = await _db.Set<ProjectTask>()
                .Where(t => t.ProjectId == projectId
                         && t.SprintId == sprintId
                         && ids.Contains(t.Id))
                .ToListAsync(ct);

            var map = orders
                .GroupBy(x => x.taskId)
                .ToDictionary(g => g.Key, g => g.First().order);

            foreach (var t in tasks)
            {
                if (map.TryGetValue(t.Id, out var ord))
                {
                    t.OrderInSprint = ord;
                    t.UpdateAt = DateTime.UtcNow;
                }
            }

            await _db.SaveChangesAsync(ct);
        }

        public async Task CompleteAsync(Guid sprintId, Guid projectId, bool carryBacklog, Guid? nextSprintId, CancellationToken ct)
        {
            var s = await GetAsync(sprintId, projectId, ct) ?? throw CustomExceptionFactory.CreateNotFoundError("Sprint not found");
            if (s.Status != SprintStatus.Active)
                throw CustomExceptionFactory.CreateBadRequestError("Sprint is not Active");


            // các task chưa Done → trả về backlog hoặc chuyển sang sprint khác
            var undone = await _db.Set<ProjectTask>()
            .Where(t => t.ProjectId == projectId && t.SprintId == sprintId && t.Status != "Done" && !t.IsDeleted)
            .ToListAsync(ct);


            if (!carryBacklog && nextSprintId.HasValue)
            {
                var maxOrder = await _db.Set<ProjectTask>()
                .Where(t => t.SprintId == nextSprintId)
                .Select(t => (int?)t.OrderInSprint).MaxAsync(ct) ?? 0;
                foreach (var t in undone)
                {
                    t.SprintId = nextSprintId;
                    t.IsBacklog = false;
                    t.OrderInSprint = ++maxOrder;
                    t.UpdateAt = DateTime.UtcNow;
                }
            }
            else
            {
                foreach (var t in undone)
                {
                    t.SprintId = null;
                    t.IsBacklog = true;
                    t.OrderInSprint = null;
                    t.UpdateAt = DateTime.UtcNow;
                }
            }


            s.Status = SprintStatus.Completed;
            s.UpdateAt = DateTime.UtcNow;


            await _db.SaveChangesAsync(ct);
        }
        public async Task<int> AddTasksAsync(
    Guid sprintId,
    Guid projectId,
    IEnumerable<Guid> taskIds,
    Guid userId,
    CancellationToken ct)
        {
            var s = await GetAsync(sprintId, projectId, ct)
                ?? throw CustomExceptionFactory.CreateNotFoundError("Sprint not found");

            // Khi sprint đang Active và bị khoá thì không cho thêm/bớt task
            if (s.Status == SprintStatus.Active )
                throw CustomExceptionFactory.CreateBadRequestError("Sprint is locked");

            var ids = taskIds?.Distinct().ToList() ?? new List<Guid>();
            if (ids.Count == 0) return 0;

            var tasks = await _db.Set<ProjectTask>()
                .Where(t => t.ProjectId == projectId && ids.Contains(t.Id) && !t.IsDeleted)
                .ToListAsync(ct);

            var maxOrder = await _db.Set<ProjectTask>()
                .Where(t => t.SprintId == sprintId)
                .Select(t => (int?)t.OrderInSprint)
                .MaxAsync(ct) ?? 0;

            foreach (var t in tasks)
            {
                t.SprintId = sprintId;
                t.IsBacklog = false;
                t.OrderInSprint = ++maxOrder;
                t.UpdateAt = DateTime.UtcNow; // đổi thành UpdatedAt nếu entity dùng tên đó
            }

            return await _db.SaveChangesAsync(ct);
        }

        public async Task<int> RemoveTasksAsync(
            Guid sprintId,
            Guid projectId,
            IEnumerable<Guid> taskIds,
            CancellationToken ct)
        {
            var ids = taskIds?.Distinct().ToList() ?? new List<Guid>();
            if (ids.Count == 0) return 0;

            var tasks = await _db.Set<ProjectTask>()
                .Where(t => t.ProjectId == projectId
                         && t.SprintId == sprintId
                         && ids.Contains(t.Id)
                         && !t.IsDeleted)
                .ToListAsync(ct);

            foreach (var t in tasks)
            {
                t.SprintId = null;       // trả về backlog
                t.IsBacklog = true;
                t.OrderInSprint = null;
                t.UpdateAt = DateTime.UtcNow; // hoặc UpdatedAt
            }

            return await _db.SaveChangesAsync(ct);
        }

    }
}
