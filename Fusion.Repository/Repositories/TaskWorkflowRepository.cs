using Fusion.Repository.Data;
using Fusion.Repository.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fusion.Repository.Repositories
{
    public interface ITaskWorkflowRepository
    {
        Task<List<TaskWorkflow>> GetByTaskIdAsync(Guid taskId, CancellationToken ct = default);
        Task UpsertAssignmentsAsync(Guid taskId, Dictionary<Guid, Guid?> assignments, CancellationToken ct = default);
    }
    public class TaskWorkflowRepository : ITaskWorkflowRepository
    {
        private readonly FusionDbContext _db;

        public TaskWorkflowRepository(FusionDbContext db)
        {
            _db = db;
        }

        public async Task<List<TaskWorkflow>> GetByTaskIdAsync(Guid taskId, CancellationToken ct = default)
        {
            return await _db.Set<TaskWorkflow>()
                .AsNoTracking()
                .Include(x => x.AssignUser)
                .Include(x => x.WorkflowStatus)
                .Where(x => x.TaskId == taskId)
                .ToListAsync(ct);
        }

        /// <summary>
        /// Upsert tất cả assign user của các workflow_status thuộc task này
        /// </summary>
        public async Task UpsertAssignmentsAsync(
     Guid taskId,
     Dictionary<Guid, Guid?> assignments,
     CancellationToken ct = default)
        {
            var set = _db.Set<TaskWorkflow>();

            var existing = await set
                .Where(x => x.TaskId == taskId)
                .ToListAsync(ct);

            foreach (var kv in assignments)
            {
                var statusId = kv.Key;
                var userId = kv.Value;

                var row = existing.FirstOrDefault(x => x.WorkflowStatusId == statusId);

                // Null = unassign → xóa record nếu có
                if (!userId.HasValue)
                {
                    if (row != null)
                    {
                        set.Remove(row);
                    }
                    continue;
                }

                // Có userId → insert / update
                if (row != null)
                {
                    row.AssignUserId = userId.Value;
                    row.CreatedAt = DateTime.UtcNow;
                    set.Update(row);
                }
                else
                {
                    var newItem = new TaskWorkflow
                    {
                        Id = Guid.NewGuid(),
                        TaskId = taskId,
                        WorkflowStatusId = statusId,
                        AssignUserId = userId.Value,
                        CreatedAt = DateTime.UtcNow
                    };
                    await set.AddAsync(newItem, ct);
                }
            }

            await _db.SaveChangesAsync(ct);
        }

    }
}
