using Fusion.Repository.Bases.Exceptions;
using Fusion.Repository.Bases.Page;
using Fusion.Repository.Bases.Page.TaskLogEvent;
using Fusion.Repository.Bases.Responses;
using Fusion.Repository.Data;
using Fusion.Repository.Entities;
using Fusion.Repository.Repositories;
using Fusion.Service.Commons.Helpers;
using Fusion.Service.ViewModels.TaskLogEventQuery;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fusion.Service.Services
{
    public interface ITaskLogEventService
    {
        Task<TaskLogEvent> CreateAsync(TaskLogEvent log, CancellationToken ct = default);
        Task<TaskLogEvent?> GetByIdAsync(long id, CancellationToken ct = default);

        Task<PagedResult<TaskLogEvent>> GetPagedByTaskIdAsync(
            Guid taskId,
            TaskLogEventPagedSearchRequest? request,
            CancellationToken ct = default);
        Task<PagedResult<TaskLogEvent>> GetPagedByProjectIdAsync(
      Guid projectId,
      TaskLogEventPagedSearchRequest req,
      CancellationToken ct = default);
        Task<bool> UpdateIsViewForTaskAsync(Guid taskId, bool isView, CancellationToken ct = default);
        Task<PagedResult<ProjectActivityVm>> GetPagedProjectActivitiesAsync(
      Guid projectId,
      TaskLogEventPagedSearchRequest req,
      CancellationToken ct = default);
        Task<PagedResult<ProjectActivityVm>> GetPagedTaskLogsVmByTaskIdAsync(Guid taskId, TaskLogEventPagedSearchRequest? request, CancellationToken ct = default);
        Task<ProjectActivityVm?> GetTaskLogVmByIdAsync(long id, CancellationToken ct = default);

    }
    public sealed class TaskLogEventService : ITaskLogEventService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ITaskLogEventRepository _repo;
        private readonly ICurrentService _current;

        public TaskLogEventService(IUnitOfWork unitOfWork, ITaskLogEventRepository repo, ICurrentService current)
        {
            _unitOfWork = unitOfWork;
            _repo = repo;
            _current = current;
        }
        public async Task<PagedResult<ProjectActivityVm>> GetPagedTaskLogsVmByTaskIdAsync(
    Guid taskId,
    TaskLogEventPagedSearchRequest? request,
    CancellationToken ct = default)
        {
            var userId = _current.GetUserId();
            if (userId == Guid.Empty)
                throw CustomExceptionFactory.CreateUnauthorizedError("Unauthorized");

            request ??= new TaskLogEventPagedSearchRequest();
            if (string.IsNullOrWhiteSpace(request.SortColumn))
            {
                request.SortColumn = "CreatedAt";
                request.SortDescending = true;
            }

            return await _repo.GetPagedTaskLogsVmByTaskIdAsync(taskId, userId, request, ct);
        }

        public async Task<ProjectActivityVm?> GetTaskLogVmByIdAsync(long id, CancellationToken ct = default)
        {
            var userId = _current.GetUserId();
            if (userId == Guid.Empty)
                throw CustomExceptionFactory.CreateUnauthorizedError("Unauthorized");

            return await _repo.GetTaskLogVmByIdAsync(id, userId, ct);
        }

        public async Task<PagedResult<ProjectActivityVm>> GetPagedProjectActivitiesAsync(
        Guid projectId,
        TaskLogEventPagedSearchRequest req,
        CancellationToken ct = default)
        {
            var userId = _current.GetUserId();
            if (userId == Guid.Empty)
                throw CustomExceptionFactory.CreateUnauthorizedError("Unauthorized");

            req ??= new TaskLogEventPagedSearchRequest();
            if (string.IsNullOrWhiteSpace(req.SortColumn))
            {
                req.SortColumn = "CreatedAt";
                req.SortDescending = true;
            }

            return await _repo.GetPagedProjectActivitiesAsync(projectId, userId, req, ct);
        }
        public async Task<PagedResult<TaskLogEvent>> GetPagedByProjectIdAsync(
      Guid projectId,
      TaskLogEventPagedSearchRequest req,
      CancellationToken ct = default)
        {
            var userId = _current.GetUserId();
            if (userId == Guid.Empty)
                throw CustomExceptionFactory.CreateUnauthorizedError("Unauthorized");

            req ??= new TaskLogEventPagedSearchRequest();

            if (string.IsNullOrWhiteSpace(req.SortColumn))
            {
                req.SortColumn = "CreatedAt";
                req.SortDescending = true;
            }

            return await _repo.GetPagedByProjectIdAsync(projectId, userId, req, ct);
        }
        public async Task<TaskLogEvent> CreateAsync(TaskLogEvent log, CancellationToken ct = default)
        {
            if (log == null)
                throw CustomExceptionFactory.CreateBadRequestError(ResponseMessages.INVALID_INPUT.FormatMessage("log"));

            if (log.TaskId == null || log.TaskId == Guid.Empty)
                throw CustomExceptionFactory.CreateBadRequestError("TaskId is required.");

            var userId = _current.GetUserId();
            if (userId == Guid.Empty)
                throw CustomExceptionFactory.CreateUnauthorizedError("Unauthorized");

            try
            {
                log.ActorId = log.ActorId == null || log.ActorId == Guid.Empty ? userId : log.ActorId;
                log.CreatedAt = DateTimeOffset.UtcNow;
                log.IsDeleted = false;
                log.IsView = false;
                log.UpdatedAt = null;

                await _unitOfWork.Repository<TaskLogEvent>().AddAsync(log);
                await _unitOfWork.SaveChangesAsync(ct);
                return log;
            }
            catch (DbUpdateException dbEx)
            {
                throw new Exception("Database update failed when creating TaskLogEvent.", dbEx);
            }
        }

        public Task<TaskLogEvent?> GetByIdAsync(long id, CancellationToken ct = default)
            => _repo.GetByIdAsync(id, ct);

        public async Task<PagedResult<TaskLogEvent>> GetPagedByTaskIdAsync(Guid taskId, TaskLogEventPagedSearchRequest? request, CancellationToken ct = default)
        {
            var userId = _current.GetUserId();
            if (userId == Guid.Empty)
                throw CustomExceptionFactory.CreateUnauthorizedError("Unauthorized");

            request ??= new TaskLogEventPagedSearchRequest();

            if (string.IsNullOrWhiteSpace(request.SortColumn))
            {
                request.SortColumn = "CreatedAt";
                request.SortDescending = true;
            }

            return await _repo.GetPagedByTaskIdAsync(taskId, userId, request, ct);
        }

        public async Task<bool> UpdateIsViewForTaskAsync(Guid taskId, bool isView, CancellationToken ct = default)
        {
            var userId = _current.GetUserId();
            if (userId == Guid.Empty)
                throw CustomExceptionFactory.CreateUnauthorizedError("Unauthorized");

            return await _repo.UpdateIsViewForTaskAsync(taskId, isView, userId, ct);
        }

    }

}
