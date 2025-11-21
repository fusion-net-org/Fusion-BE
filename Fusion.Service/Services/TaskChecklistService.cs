using Fusion.Repository.Bases.Exceptions;
using Fusion.Repository.Bases.Responses;
using Fusion.Repository.Data;
using Fusion.Repository.Entities;
using Fusion.Repository.Repositories;
using Fusion.Service.Commons.Helpers;
using Fusion.Service.IServices;
using Fusion.Service.ViewModels.Task.Request;
using Fusion.Service.ViewModels.Task.Response;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fusion.Service.Services
{
    public interface ITaskChecklistService
    {
        Task<IReadOnlyList<TaskChecklistItemResponse>> GetByTaskIdAsync(Guid taskId, CancellationToken ct = default);
        Task<TaskChecklistItemResponse> AddAsync(TaskChecklistItemCreateRequest req, Guid userId, CancellationToken ct = default);
        Task<TaskChecklistItemResponse> UpdateAsync(TaskChecklistItemUpdateRequest req, Guid userId, CancellationToken ct = default);
        Task<bool> DeleteAsync(Guid id, Guid userId, CancellationToken ct = default);
        Task<TaskChecklistItemResponse> ToggleDoneAsync(Guid id, bool? isDone, Guid userId, CancellationToken ct = default);
    }
    public class TaskChecklistService : ITaskChecklistService
    {
        private readonly FusionDbContext _db;
        private readonly ITaskChecklistRepository _repo;
        private readonly ICompanyActivityService _log;
        private readonly ICurrentService _current;

        public TaskChecklistService(
            FusionDbContext db,
            ITaskChecklistRepository repo,
            ICompanyActivityService log,
            ICurrentService current)
        {
            _db = db;
            _repo = repo;
            _log = log;
            _current = current;
        }

        private static TaskChecklistItemResponse ToResponse(ProjectTaskChecklistItem e)
            => new TaskChecklistItemResponse
            {
                Id = e.Id,
                TaskId = e.TaskId,
                Label = e.Label,
                IsDone = e.IsDone,
                OrderIndex = e.OrderIndex,
                CreatedAt = e.CreatedAt
            };

        private async Task<string?> GetUserName(Guid userId)
            => (await _db.Users.AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == userId))?.UserName;

        public async Task<IReadOnlyList<TaskChecklistItemResponse>> GetByTaskIdAsync(Guid taskId, CancellationToken ct = default)
        {
            var exists = await _db.ProjectTasks.AsNoTracking()
                .AnyAsync(t => t.Id == taskId && !t.IsDeleted, ct);

            if (!exists)
                throw CustomExceptionFactory.CreateNotFoundError(
                    ResponseMessages.NOT_FOUND.FormatMessage("Task"));

            var items = await _repo.GetByTaskIdAsync(taskId, ct);
            return items.Select(ToResponse).ToList();
        }

        public async Task<TaskChecklistItemResponse> AddAsync(TaskChecklistItemCreateRequest req, Guid userId, CancellationToken ct = default)
        {
            var task = await _db.ProjectTasks.AsNoTracking()
                .FirstOrDefaultAsync(t => t.Id == req.TaskId && !t.IsDeleted, ct)
                ?? throw CustomExceptionFactory.CreateNotFoundError(
                    ResponseMessages.NOT_FOUND.FormatMessage("Task"));

            if (string.IsNullOrWhiteSpace(req.Label))
                throw CustomExceptionFactory.CreateBadRequestError("Checklist label is required.");

            var now = DateTime.UtcNow;

            var entity = new ProjectTaskChecklistItem
            {
                Id = Guid.NewGuid(),
                TaskId = req.TaskId,
                Label = req.Label.Trim(),
                IsDone = false,
                OrderIndex = await _repo.GetNextOrderIndexAsync(req.TaskId, ct),
                CreatedAt = now
            };

            await _repo.AddAsync(entity, ct);

            // log hoạt động
            var companyId = await _db.Projects
                .Where(p => p.Id == task.ProjectId)
                .Select(p => (Guid)p.CompanyId!)
                .FirstAsync(ct);

            await _log.CreateLog(new CompanyActivityLog
            {
                CompanyId = companyId,
                ActorUserId = _current.GetUserId(),
                Title = "Add checklist item",
                Description = $"User '{await GetUserName(_current.GetUserId())}' added checklist '{entity.Label}' to task '{task.Title}'"
            });

            return ToResponse(entity);
        }

        public async Task<TaskChecklistItemResponse> UpdateAsync(TaskChecklistItemUpdateRequest req, Guid userId, CancellationToken ct = default)
        {
            var entity = await _repo.FindByIdAsync(req.Id, ct)
                ?? throw CustomExceptionFactory.CreateNotFoundError(
                    ResponseMessages.NOT_FOUND.FormatMessage("Checklist item"));

            if (!string.IsNullOrWhiteSpace(req.Label))
                entity.Label = req.Label.Trim();

            entity.IsDone = req.IsDone;

            if (req.OrderIndex.HasValue && req.OrderIndex.Value >= 0)
                entity.OrderIndex = req.OrderIndex.Value;

            await _repo.UpdateAsync(entity, ct);
            return ToResponse(entity);
        }

        public async Task<bool> DeleteAsync(Guid id, Guid userId, CancellationToken ct = default)
        {
            var ok = await _repo.DeleteAsync(id, ct);
            return ok;
        }

        public async Task<TaskChecklistItemResponse> ToggleDoneAsync(Guid id, bool? isDone, Guid userId, CancellationToken ct = default)
        {
            var entity = await _repo.FindByIdAsync(id, ct)
                ?? throw CustomExceptionFactory.CreateNotFoundError(
                    ResponseMessages.NOT_FOUND.FormatMessage("Checklist item"));

            entity.IsDone = isDone ?? !entity.IsDone;
            await _repo.UpdateAsync(entity, ct);

            return ToResponse(entity);
        }
    }
}
