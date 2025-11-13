using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fusion.Repository.Bases.Exceptions;
using Fusion.Repository.Bases.Page;
using Fusion.Repository.Bases.Responses;
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
    }
}
