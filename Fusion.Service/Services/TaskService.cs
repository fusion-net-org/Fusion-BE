// Fusion.Service/Services/TaskService.cs
using AutoMapper;
using Fusion.Repository.Bases.Exceptions;
using Fusion.Repository.Bases.Page;
using Fusion.Repository.Bases.Responses;
using Fusion.Repository.Data;
using Fusion.Repository.Entities;
using Fusion.Repository.IRepositories;
using Fusion.Service.Commons.Helpers;
using Fusion.Service.IServices;
using Fusion.Service.ViewModels.Task.Request;
using Fusion.Service.ViewModels.Task.Response;
using Microsoft.EntityFrameworkCore;

namespace Fusion.Service.Services;

public class TaskService : ITaskService
{
    private readonly FusionDbContext _db;            // <— DÙNG CÁI NÀY CHO TRUY VẤN
    private readonly ITaskRepository _repo;          // <— GHI/UPDATE/DELETE
    private readonly IMapper _mapper;
    private readonly ICompanyActivityService _log;
    private readonly ICurrentService _current;

    public TaskService(
        FusionDbContext db,
        ITaskRepository repo,
        IMapper mapper,
        ICompanyActivityService log,
        ICurrentService current)
    {
        _db = db;
        _repo = repo;
        _mapper = mapper;
        _log = log;
        _current = current;
    }

    /* -------------------- CREATE -------------------- */
    public async Task<ProjectTaskResponse> CreateTaskAsync(
        ProjectTaskRequest req, Guid userId, CancellationToken ct = default)
    {
        // 1) Validate project
        var project = await _db.Projects.AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == req.ProjectId, ct)
            ?? throw CustomExceptionFactory.CreateNotFoundError(
                ResponseMessages.NOT_FOUND.FormatMessage("Project"));

        // 2) Sprint optional, nhưng nếu có phải thuộc Project
        if (req.SprintId != null)
        {
            var ok = await _db.Sprints.AsNoTracking()
                .AnyAsync(s => s.Id == req.SprintId && s.ProjectId == req.ProjectId, ct);
            if (!ok)
                throw CustomExceptionFactory.CreateNotFoundError(
                    ResponseMessages.NOT_FOUND.FormatMessage("Sprint"));
        }

        // 3) Chọn Status: ưu tiên WorkflowStatusId → StatusCode → default (TODO/first by Order)
        WorkflowStatus? status = null;
        if (req.WorkflowStatusId != null)
            status = await _db.WorkflowStatuses.AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == req.WorkflowStatusId, ct);

        if (status == null && !string.IsNullOrWhiteSpace(req.StatusCode))
        {
            var key = req.StatusCode.Trim().ToLower();
            status = await _db.WorkflowStatuses.AsNoTracking()
                .FirstOrDefaultAsync(x => x.Code != null && x.Code.ToLower() == key, ct);
        }

        if (status == null)
        {
            status = await _db.WorkflowStatuses.AsNoTracking()
                .OrderBy(x => x.Position)
                .FirstOrDefaultAsync(x => x.Category == "TODO", ct)
                ?? await _db.WorkflowStatuses.AsNoTracking()
                    .OrderBy(x => x.Position).FirstOrDefaultAsync(ct)
                ?? throw CustomExceptionFactory.CreateBadRequestError("No workflow status available.");
        }

        // 4) Sinh Code PRJ-T-###
        var seq = await _db.ProjectTasks.AsNoTracking()
            .LongCountAsync(t => t.ProjectId == req.ProjectId, ct) + 1;
        var prefix = string.IsNullOrWhiteSpace(project.Code) ? "PRJ" : project.Code!;
        var code = $"{prefix}-T-{seq:000}";

        // 5) OrderInSprint theo (Sprint + Status)
        int? orderInSprint = null;
        if (req.SprintId != null)
        {
            var max = await _db.ProjectTasks.AsNoTracking()
                .Where(t => t.SprintId == req.SprintId && t.CurrentStatusId == status.Id && !t.IsDeleted)
                .Select(t => t.OrderInSprint)
    .FirstOrDefaultAsync(ct);
            orderInSprint = (max ?? 0) + 1;
        }

        // 6) Map + defaults
        var now = DateTime.UtcNow;
        var entity = _mapper.Map<ProjectTask>(req);
        entity.Id = Guid.NewGuid();
        entity.ProjectId = req.ProjectId;
        entity.SprintId = req.SprintId;
        entity.IsBacklog = req.SprintId == null;
        entity.Code = code;
        entity.CurrentStatusId = status.Id;
        entity.Status = !string.IsNullOrWhiteSpace(status.Code) ? status.Code : status.Name;
        entity.OrderInSprint = orderInSprint;
        entity.RemainingHours = req.EstimateHours;
        entity.CreateAt = now;
        entity.UpdateAt = now;
        entity.CreatedBy = userId;
        entity.IsDeleted = false;

        // Assignees
        entity.Assignees = new List<ProjectTaskAssignee>();
        if (req.AssigneeIds?.Count > 0)
        {
            foreach (var uid in req.AssigneeIds.Distinct())
                entity.Assignees.Add(new ProjectTaskAssignee { TaskId = entity.Id, UserId = uid });
        }

        // 7) Persist
        await _repo.AddAsync(entity, ct);
        var companyId = await _db.Projects
    .Where(p => p.Id == entity.ProjectId)
    .Select(p => (Guid)p.CompanyId)    // ép về Guid
    .FirstAsync(ct);
        // 8) Log
        await _log.CreateLog(new CompanyActivityLog
        {
            CompanyId = companyId,
            ActorUserId = _current.GetUserId(),
            Title = "Create task",
            Description = $"User '{await GetUserName(_current.GetUserId())}' created task '{entity.Title}'"
        });

        return _mapper.Map<ProjectTaskResponse>(entity);
    }

    /* -------------------- UPDATE -------------------- */
    public async Task<ProjectTaskResponse?> UpdateTaskAsync(
        ProjectTaskRequest req, Guid userId, CancellationToken ct = default)
    {
        var e = await _repo.FindByIdAsync(req.Id, ct)
            ?? throw CustomExceptionFactory.CreateNotFoundError(
                ResponseMessages.NOT_FOUND.FormatMessage("Task"));

        e.Title = string.IsNullOrWhiteSpace(req.Title) ? e.Title : req.Title.Trim();
        e.Description = req.Description ?? e.Description;
        e.Type = req.Type ?? e.Type;
        e.Priority = req.Priority ?? e.Priority;
        e.Severity = req.Severity ?? e.Severity;
        e.Point = req.Point;
        e.EstimateHours = req.EstimateHours;
        e.DueDate = req.DueDate;
        e.ParentTaskId = req.ParentTaskId;
        e.SourceTaskId = req.SourceTaskId;

        // đổi sprint → tính lại order
        if (req.SprintId != e.SprintId)
        {
            if (req.SprintId != null)
            {
                var ok = await _db.Sprints.AsNoTracking()
                    .AnyAsync(s => s.Id == req.SprintId && s.ProjectId == e.ProjectId, ct);
                if (!ok) throw CustomExceptionFactory.CreateNotFoundError(
                    ResponseMessages.NOT_FOUND.FormatMessage("Sprint"));
            }
            e.SprintId = req.SprintId;
            e.IsBacklog = req.SprintId == null;

            if (req.SprintId != null && e.CurrentStatusId != null)
            {
                var max = await _db.ProjectTasks.AsNoTracking()
                    .Where(t => t.SprintId == req.SprintId && t.CurrentStatusId == e.CurrentStatusId && !t.IsDeleted)
                    .Select(t => t.OrderInSprint)
    .FirstOrDefaultAsync(ct);
                e.OrderInSprint = (max ?? 0) + 1;
            }
            else e.OrderInSprint = null;
        }

        // đổi status nếu gửi
        if (req.WorkflowStatusId != null || !string.IsNullOrWhiteSpace(req.StatusCode))
        {
            WorkflowStatus? st = null;
            if (req.WorkflowStatusId != null)
                st = await _db.WorkflowStatuses.AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Id == req.WorkflowStatusId, ct);

            if (st == null && !string.IsNullOrWhiteSpace(req.StatusCode))
            {
                var key = req.StatusCode.Trim().ToLower();
                st = await _db.WorkflowStatuses.AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Code != null && x.Code.ToLower() == key, ct);
            }

            if (st != null)
            {
                e.CurrentStatusId = st.Id;
                e.Status = !string.IsNullOrWhiteSpace(st.Code) ? st.Code : st.Name;

                if (e.SprintId != null)
                {
                    var max = await _db.ProjectTasks.AsNoTracking()
                        .Where(t => t.SprintId == e.SprintId && t.CurrentStatusId == st.Id && !t.IsDeleted)
                       .Select(t => t.OrderInSprint)
    .FirstOrDefaultAsync(ct);
                    e.OrderInSprint = (max ?? 0) + 1;
                }
            }
        }

        // assignees (replace-all)
        if (req.AssigneeIds != null)
        {
            e.Assignees.Clear();
            foreach (var uid in req.AssigneeIds.Distinct())
                e.Assignees.Add(new ProjectTaskAssignee { TaskId = e.Id, UserId = uid });
        }

        e.UpdateAt = DateTime.UtcNow;
        await _repo.UpdateAsync(e, ct);

        var companyId = await _db.Projects
       .Where(p => p.Id == e.ProjectId)
       .Select(p => (Guid)p.CompanyId)    // ép về Guid
       .FirstAsync(ct);

        await _log.CreateLog(new CompanyActivityLog
        {
            CompanyId = companyId,
            ActorUserId = _current.GetUserId(),
            Title = "Update task",
            Description = $"User '{await GetUserName(_current.GetUserId())}' updated task '{e.Title}'"
        });

        return _mapper.Map<ProjectTaskResponse>(e);
    }

    /* -------------------- READS -------------------- */
    public async Task<ProjectTaskResponse?> GetTaskByIdAsync(Guid id, CancellationToken ct = default)
    {
        var e = await _repo.FindByIdAsync(id, ct)
            ?? throw CustomExceptionFactory.CreateNotFoundError(
                ResponseMessages.NOT_FOUND.FormatMessage("Task"));
        return _mapper.Map<ProjectTaskResponse>(e);
    }

    public async Task<PagedResult<ProjectTaskResponse>> GetAllTasksAsync(
        PagedRequest request, CancellationToken ct = default)
    {
        var paged = await _repo.GetAllAsync(request, ct);
        return new PagedResult<ProjectTaskResponse>
        {
            Items = paged.Items.Select(_mapper.Map<ProjectTaskResponse>).ToList(),
            TotalCount = paged.TotalCount,
            PageNumber = paged.PageNumber,
            PageSize = paged.PageSize
        };
    }

    /* -------------------- DELETE -------------------- */
    public async Task<bool> DeleteTaskAsync(Guid id, Guid userId = default, CancellationToken ct = default)
    {
        var e = await _repo.FindByIdAsync(id, ct)
            ?? throw CustomExceptionFactory.CreateNotFoundError(
                ResponseMessages.NOT_FOUND.FormatMessage("Task"));

        var ok = await _repo.SoftDeleteAsync(id, ct);
        if (ok)
        {
            var companyId = await _db.Projects
     .Where(p => p.Id == e.ProjectId)
     .Select(p => (Guid)p.CompanyId)    
     .FirstAsync(ct);

            await _log.CreateLog(new CompanyActivityLog
            {
                CompanyId = companyId,
                ActorUserId = _current.GetUserId(),
                Title = "Delete task",
                Description = $"User '{await GetUserName(_current.GetUserId())}' deleted task '{e.Title}'"
            });
        }
        return ok;
    }

    /* -------------------- CHANGE STATUS -------------------- */
    public async Task<ProjectTaskResponse> ChangeStatus(
        Guid id, string statusText, Guid userId, CancellationToken ct = default)
    {
        var e = await _repo.FindByIdAsync(id, ct)
            ?? throw CustomExceptionFactory.CreateNotFoundError(
                ResponseMessages.NOT_FOUND.FormatMessage("Task"));

        var key = statusText.Trim().ToLower();
        var st = await _db.WorkflowStatuses.AsNoTracking()
            .FirstOrDefaultAsync(x =>
                (x.Code != null && x.Code.ToLower() == key) ||
                (x.Name != null && x.Name.ToLower() == key), ct)
            ?? throw CustomExceptionFactory.CreateBadRequestError($"Status '{statusText}' not found.");

        e.CurrentStatusId = st.Id;
        e.Status = !string.IsNullOrWhiteSpace(st.Code) ? st.Code : st.Name;

        if (e.SprintId != null)
        {
            var max = await _db.ProjectTasks.AsNoTracking()
                .Where(t => t.SprintId == e.SprintId && t.CurrentStatusId == st.Id && !t.IsDeleted)
                .Select(t => t.OrderInSprint)
    .FirstOrDefaultAsync(ct);
            e.OrderInSprint = (max ?? 0) + 1;
        }

        e.UpdateAt = DateTime.UtcNow;
        await _repo.UpdateAsync(e, ct);

        var companyId = await _db.Projects
     .Where(p => p.Id == e.ProjectId)
     .Select(p => (Guid)p.CompanyId)    // ép về Guid
     .FirstAsync(ct);

        await _log.CreateLog(new CompanyActivityLog
        {
            CompanyId = companyId,
            ActorUserId = _current.GetUserId(),
            Title = "Change task status",
            Description = $"User '{await GetUserName(_current.GetUserId())}' changed status of '{e.Title}' to '{st.Code ?? st.Name}'"
        });

        return _mapper.Map<ProjectTaskResponse>(e);
    }

    /* -------------------- Helpers -------------------- */
    private async Task<string?> GetUserName(Guid userId)
        => (await _db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId))?.UserName;
}
