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
    public interface ITaskChecklistRepository
    {
        Task<ProjectTaskChecklistItem> AddAsync(ProjectTaskChecklistItem entity, CancellationToken ct = default);
        Task<ProjectTaskChecklistItem?> FindByIdAsync(Guid id, CancellationToken ct = default);
        Task<List<ProjectTaskChecklistItem>> GetByTaskIdAsync(Guid taskId, CancellationToken ct = default);
        Task<ProjectTaskChecklistItem> UpdateAsync(ProjectTaskChecklistItem entity, CancellationToken ct = default);
        Task<bool> DeleteAsync(Guid id, CancellationToken ct = default);
        Task<int> GetNextOrderIndexAsync(Guid taskId, CancellationToken ct = default);
    }
    public class TaskChecklistRepository : ITaskChecklistRepository
    {
        private readonly FusionDbContext _db;
        public TaskChecklistRepository(FusionDbContext db) => _db = db;

        public async Task<ProjectTaskChecklistItem> AddAsync(ProjectTaskChecklistItem entity, CancellationToken ct = default)
        {
            if (entity.OrderIndex <= 0)
            {
                entity.OrderIndex = await GetNextOrderIndexAsync(entity.TaskId, ct);
            }

            _db.ProjectTaskChecklistItems.Add(entity);
            await _db.SaveChangesAsync(ct);
            return entity;
        }

        public async Task<ProjectTaskChecklistItem?> FindByIdAsync(Guid id, CancellationToken ct = default)
        {
            return await _db.ProjectTaskChecklistItems
                .FirstOrDefaultAsync(x => x.Id == id, ct);
        }

        public async Task<List<ProjectTaskChecklistItem>> GetByTaskIdAsync(Guid taskId, CancellationToken ct = default)
        {
            return await _db.ProjectTaskChecklistItems
                .Where(x => x.TaskId == taskId)
                .OrderBy(x => x.OrderIndex)
                .ThenBy(x => x.CreatedAt)
                .ToListAsync(ct);
        }

        public async Task<ProjectTaskChecklistItem> UpdateAsync(ProjectTaskChecklistItem entity, CancellationToken ct = default)
        {
            _db.ProjectTaskChecklistItems.Update(entity);
            await _db.SaveChangesAsync(ct);
            return entity;
        }

        public async Task<bool> DeleteAsync(Guid id, CancellationToken ct = default)
        {
            var e = await _db.ProjectTaskChecklistItems
                .FirstOrDefaultAsync(x => x.Id == id, ct);
            if (e == null) return false;

            _db.ProjectTaskChecklistItems.Remove(e);
            await _db.SaveChangesAsync(ct);
            return true;
        }

        public async Task<int> GetNextOrderIndexAsync(Guid taskId, CancellationToken ct = default)
        {
            var max = await _db.ProjectTaskChecklistItems
                .Where(x => x.TaskId == taskId)
                .MaxAsync(x => (int?)x.OrderIndex, ct);

            return (max ?? 0) + 1;
        }
    }
}
