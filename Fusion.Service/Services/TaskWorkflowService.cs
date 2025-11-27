using AutoMapper;
using Fusion.Repository.Bases.Exceptions;
using Fusion.Repository.Bases.Responses;
using Fusion.Repository.Data;
using Fusion.Repository.Entities;
using Fusion.Repository.IRepositories;
using Fusion.Repository.Repositories;
using Fusion.Service.Commons.Helpers;
using Fusion.Service.IServices;
using Fusion.Service.ViewModels.Notifications.Requests;
using Fusion.Service.ViewModels.Task.Request;
using Fusion.Service.ViewModels.Task.Response;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Fusion.Service.Services
{
    public interface ITaskWorkflowService
    {
        /// <summary>
        /// Lấy danh sách status trong workflow + người được assign cho từng status
        /// </summary>
        Task<TaskWorkflowAssignmentsResponse> GetAssignmentsForTaskAsync(
            Guid taskId,
            CancellationToken ct = default);

        /// <summary>
        /// Cập nhật ma trận status → user cho 1 task (upsert)
        /// </summary>
        Task<TaskWorkflowAssignmentsResponse> UpsertAssignmentsForTaskAsync(
            TaskWorkflowAssignmentsRequest request,
            Guid actorUserId,
            CancellationToken ct = default);
        Task UpsertAssignmentsAsync(
    Guid taskId,
    IDictionary<Guid, Guid?> assignments,
    CancellationToken ct = default);
    }
    public class TaskWorkflowService : ITaskWorkflowService
    {
        private readonly FusionDbContext _db;
        private readonly ITaskWorkflowRepository _taskWorkflowRepo;
        private readonly ITaskRepository _taskRepo;
        private readonly ICompanyActivityService _log;
        private readonly ICurrentService _current;
        private readonly IMapper _mapper;
        private readonly INotificationService _notificationService;
        public TaskWorkflowService(
            FusionDbContext db,
            ITaskWorkflowRepository taskWorkflowRepo,
            ITaskRepository taskRepo,
            ICompanyActivityService log,
            ICurrentService current,
            IMapper mapper,
            INotificationService notificationService)
        {
            _db = db;
            _taskWorkflowRepo = taskWorkflowRepo;
            _taskRepo = taskRepo;
            _log = log;
            _current = current;
            _mapper = mapper;
            _notificationService = notificationService;
        }

        #region Helpers

        private async Task<string?> GetUserName(Guid userId)
            => (await _db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId))
                ?.UserName;
        public async Task UpsertAssignmentsAsync(
    Guid taskId,
    IDictionary<Guid, Guid?> assignments,
    CancellationToken ct = default)
        {
            // chỉ lấy những item có userId thật (không null, không Guid.Empty)
            var validAssignments = assignments
                .Where(kv => kv.Value.HasValue && kv.Value != Guid.Empty)
                .ToDictionary(kv => kv.Key, kv => kv.Value!.Value);

            // load record hiện có
            var existing = await _db.TaskWorkflows
                .Where(x => x.TaskId == taskId)
                .ToListAsync(ct);

            var now = DateTime.UtcNow;

            // 1️⃣ cập nhật hoặc thêm mới cho những cái có userId
            foreach (var kv in validAssignments)
            {
                var statusId = kv.Key;
                var userId = kv.Value;

                var row = existing.FirstOrDefault(x => x.WorkflowStatusId == statusId);

                if (row == null)
                {
                    _db.TaskWorkflows.Add(new TaskWorkflow
                    {
                        Id = Guid.NewGuid(),
                        TaskId = taskId,
                        WorkflowStatusId = statusId,
                        AssignUserId = userId,
                        CreatedAt = now,
                        UpdateAt = now,
                    });
                }
                else
                {
                    row.AssignUserId = userId;
                    row.UpdateAt = now;
                }
            }

            // 2️⃣ Với những statusId không có trong validAssignments → mark delete (nếu trước đó có)
            foreach (var row in existing)
            {
                if (!row.WorkflowStatusId.HasValue || !validAssignments.ContainsKey(row.WorkflowStatusId.Value))
                {
                    row.AssignUserId = null;
                    row.UpdateAt = now;
                }
            }

            await _db.SaveChangesAsync(ct);
        }

        /// <summary>
        /// Xác định workflowId của Task giống logic trong TaskService.GetWorkflowIdForTask
        /// </summary>
        private async Task<Guid> GetWorkflowIdForTaskAsync(ProjectTask task, CancellationToken ct)
        {
            // 1) Ưu tiên từ status hiện tại
            if (task.CurrentStatusId.HasValue)
            {
                Guid? wfFromStatus = await _db.WorkflowStatuses.AsNoTracking()
                    .Where(ws => ws.Id == task.CurrentStatusId.Value)
                    .Select(ws => (Guid?)ws.WorkflowId)
                    .FirstOrDefaultAsync(ct);

                if (wfFromStatus.HasValue && wfFromStatus.Value != Guid.Empty)
                    return wfFromStatus.Value;
            }

            // 2) Fallback từ Project
            if (!task.ProjectId.HasValue)
                throw CustomExceptionFactory.CreateBadRequestError("Task has no ProjectId.");

            Guid? wfFromProject = await _db.Projects.AsNoTracking()
                .Where(p => p.Id == task.ProjectId.Value)
                .Select(p => p.WorkflowId)
                .FirstOrDefaultAsync(ct);

            if (wfFromProject.HasValue && wfFromProject.Value != Guid.Empty)
                return wfFromProject.Value;

            // 3) Fallback từ các task khác trong project
            Guid? wfAny = await _db.ProjectTasks.AsNoTracking()
                .Where(t => t.ProjectId == task.ProjectId.Value && t.CurrentStatusId != null)
                .Join(_db.WorkflowStatuses.AsNoTracking(),
                      t => t.CurrentStatusId,
                      ws => ws.Id,
                      (t, ws) => (Guid?)ws.WorkflowId)
                .FirstOrDefaultAsync(ct);

            if (wfAny.HasValue && wfAny.Value != Guid.Empty)
                return wfAny.Value;

            throw CustomExceptionFactory.CreateBadRequestError("Cannot determine workflow for this task.");
        }

        #endregion

        #region Queries

        public async Task<TaskWorkflowAssignmentsResponse> GetAssignmentsForTaskAsync(
            Guid taskId,
            CancellationToken ct = default)
        {
            // 1) Kiểm tra task tồn tại
            var task = await _db.ProjectTasks
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Id == taskId && !t.IsDeleted, ct)
                ?? throw CustomExceptionFactory.CreateNotFoundError(
                    ResponseMessages.NOT_FOUND.FormatMessage("Task"));

            // 2) Lấy workflowId đang áp dụng cho task
            var workflowId = await GetWorkflowIdForTaskAsync(task, ct);

            // 3) Lấy tất cả status trong workflow đó theo thứ tự
            var statuses = await _db.WorkflowStatuses.AsNoTracking()
                .Where(s => s.WorkflowId == workflowId)
                .OrderBy(s => s.Position)
                .ToListAsync(ct);

            // 4) Lấy các dòng TaskWorkflow của task
            var rows = await _taskWorkflowRepo.GetByTaskIdAsync(taskId, ct);

            // Chỉ quan tâm tới các dòng có WorkflowStatusId (assignee theo bước)
            var map = rows
                .Where(x => x.WorkflowStatusId.HasValue)
                .GroupBy(x => x.WorkflowStatusId!.Value)
                .ToDictionary(
                    g => g.Key,
                    g => g
                        .OrderByDescending(r => r.CreatedAt) // nếu lỡ có nhiều dòng thì lấy dòng mới nhất
                        .First());

            // 5) Build response: đảm bảo FE luôn nhận được full list status
            var items = new List<TaskWorkflowAssignmentItemResponse>();
            foreach (var st in statuses)
            {
                map.TryGetValue(st.Id, out var row);

                items.Add(new TaskWorkflowAssignmentItemResponse
                {
                    WorkflowStatusId = st.Id,
                    StatusCode = st.Code ?? "",
                    StatusName = st.Name ?? "",
                    Category = st.Category ?? "",
                    Position = st.Position,

                    AssignUserId = row?.AssignUserId,
                    AssignUserName = row?.AssignUser?.UserName ?? row?.AssignUser?.UserName,
                    AssignUserEmail = row?.AssignUser?.Email,
                    AssignUserAvatarUrl = row?.AssignUser?.Avatar
                });
            }

            return new TaskWorkflowAssignmentsResponse
            {
                TaskId = taskId,
                WorkflowId = workflowId,
                Items = items
            };
        }

        #endregion

        #region Commands

        public async Task<TaskWorkflowAssignmentsResponse> UpsertAssignmentsForTaskAsync(
      TaskWorkflowAssignmentsRequest request,
      Guid actorUserId,
      CancellationToken ct = default)
        {
            // 1) Validate task
            var task = await _taskRepo.FindByIdAsync(request.TaskId, ct)
                ?? throw CustomExceptionFactory.CreateNotFoundError(
                    ResponseMessages.NOT_FOUND.FormatMessage("Task"));

            // 2) Get workflowId for this task
            var workflowId = await GetWorkflowIdForTaskAsync(task, ct);

            // 3) Validate statuses + get status names
            var statusIds = request.Items
                .Select(x => x.WorkflowStatusId)
                .Distinct()
                .ToList();

            var statusList = await _db.WorkflowStatuses.AsNoTracking()
                .Where(ws => ws.WorkflowId == workflowId && statusIds.Contains(ws.Id))
                .Select(ws => new { ws.Id, ws.Name })
                .ToListAsync(ct);

            var validStatusIds = statusList.Select(s => s.Id).ToList();
            var invalidStatusIds = statusIds.Except(validStatusIds).ToList();
            if (invalidStatusIds.Any())
            {
                throw CustomExceptionFactory.CreateBadRequestError(
                    "Some statuses are not in the workflow of this task.");
            }

            // dùng list này để map Id -> Name
            var statusNameById = statusList.ToDictionary(
                x => x.Id,
                x => x.Name ?? string.Empty
            );

            // 4) Validate users: must be project members
            var userIds = request.Items
                .Select(x => x.AssignUserId)
                .Where(id => id.HasValue)
                .Select(id => id!.Value)
                .Distinct()
                .ToList();

            if (userIds.Any())
            {
                var projectId = task.ProjectId
                    ?? throw CustomExceptionFactory.CreateBadRequestError("Task has no ProjectId.");

                var memberUserIds = await _db.ProjectMembers.AsNoTracking()
                    .Where(pm => pm.ProjectId == projectId
                              && pm.UserId.HasValue
                              && userIds.Contains(pm.UserId.Value))
                    .Select(pm => pm.UserId!.Value)
                    .ToListAsync(ct);

                var notMembers = userIds.Except(memberUserIds).ToList();
                if (notMembers.Any())
                {
                    throw CustomExceptionFactory.CreateBadRequestError(
                        "Some users are not members of this project.");
                }
            }

            // 5) Build dictionary: statusId → assignUserId (null = unassign)
            var assignments = request.Items
                .DistinctBy(x => x.WorkflowStatusId)
                .ToDictionary(
                    x => x.WorkflowStatusId,
                    x => x.AssignUserId);

            // 6) Upsert to DB
            await _taskWorkflowRepo.UpsertAssignmentsAsync(request.TaskId, assignments, ct);

            // 7) Company activity log
            var companyId = await _db.Projects.AsNoTracking()
                .Where(p => p.Id == task.ProjectId)
                .Select(p => (Guid)p.CompanyId)
                .FirstAsync(ct);

            var actorUserName = await GetUserName(actorUserId) ?? "Unknown";

            await _log.CreateLog(new CompanyActivityLog
            {
                CompanyId = companyId,
                ActorUserId = actorUserId,
                Title = "Updated task workflow assignees",
                Description = $"User '{actorUserName}' updated workflow assignees for task '{task.Title}'."
            });

            // 8) Notifications per assignee
            var assignmentsByUser = request.Items
                .Where(x => x.AssignUserId.HasValue)
                .GroupBy(x => x.AssignUserId!.Value)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(x => x.WorkflowStatusId).Distinct().ToList()
                );

            foreach (var kvp in assignmentsByUser)
            {
                var assigneeId = kvp.Key;

                // Optionally skip notifying the actor themself
                if (assigneeId == actorUserId)
                    continue;

                var statusNames = kvp.Value
                    .Select(id => statusNameById.TryGetValue(id, out var n) ? n : null)
                    .Where(n => !string.IsNullOrWhiteSpace(n))
                    .Distinct()
                    .ToList();

                var statusesText = statusNames.Any()
                    ? string.Join(", ", statusNames)
                    : "workflow statuses";

                await _notificationService.CreateNotificationAsync(new SendNotificationRequest
                {
                    UserId = assigneeId,
                    Title = $"You have been assigned in the workflow of task \"{task.Title}\"",
                    Body = $"User {actorUserName} updated the workflow and assigned you to the following statuses: {statusesText} in task \"{task.Title}\".",
                    LinkKey = "TASK_DETAIL_PAGE",   // đổi nếu FE dùng key khác
                    IdLink = task.Id,
                    Event = "TaskWorkflowAssigneeUpdated",
                    NotificationType = "TASK",
                }, ct);
            }

            // 9) Return latest assignments
            return await GetAssignmentsForTaskAsync(request.TaskId, ct);
        }


        #endregion
    }
}
