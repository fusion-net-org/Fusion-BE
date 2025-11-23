// Fusion.Service/Services/TaskService.cs
using AutoMapper;
using Fusion.Repository.Bases.Exceptions;
using Fusion.Repository.Bases.Page;
using Fusion.Repository.Bases.Page.Task;
using Fusion.Repository.Bases.Responses;
using Fusion.Repository.Data;
using Fusion.Repository.Entities;
using Fusion.Repository.IRepositories;
using Fusion.Service.Commons.Helpers;
using Fusion.Service.IServices;
using Fusion.Service.ViewModels.Comment.Response;
using Fusion.Service.ViewModels.Project.Responses;
using Fusion.Service.ViewModels.ProjectMembers.Responses;
using Fusion.Service.ViewModels.Projects.Responses;
using Fusion.Service.ViewModels.Sprint.Responses;
using Fusion.Service.ViewModels.Task.Request;
using Fusion.Service.ViewModels.Task.Response;
using Fusion.Service.ViewModels.WorkflowStatus;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace Fusion.Service.Services;

public class TaskService : ITaskService
{
    private readonly FusionDbContext _db;           
    private readonly ITaskRepository _repo;         
    private readonly IMapper _mapper;
    private readonly ICompanyActivityService _log;
    private readonly ICurrentService _current;
    private readonly ITaskWorkflowService _taskWorkflowService;
    public TaskService(
        FusionDbContext db,
        ITaskRepository repo,
        IMapper mapper,
        ICompanyActivityService log,
        ICurrentService current,
        ITaskWorkflowService taskWorkflowService)
    {
        _db = db;
        _repo = repo;
        _mapper = mapper;
        _log = log;
        _current = current;
        _taskWorkflowService = taskWorkflowService;
    }
    #region Helpers
    /* -------------------- Helpers -------------------- */
    private static string StripPartSuffix(string? title)
    => Regex.Replace(title ?? "", @"\s*\(Part\s+[A-Z]+\)\s*$", "", RegexOptions.IgnoreCase).Trim();

    // 1->A, 2->B, ..., 26->Z, 27->AA ...
    private static string AlphaIndex(int n)
    {
        var sb = new System.Text.StringBuilder();
        while (n > 0)
        {
            n--; sb.Insert(0, (char)('A' + (n % 26))); n /= 26;
        }
        return sb.ToString();
    }

    private async Task<string?> GetUserName(Guid userId)
        => (await _db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId))?.UserName;
    private async Task<int> GetNextOrderInSprint(Guid sprintId, Guid statusId, CancellationToken ct)
    {
        var max = await _db.ProjectTasks.AsNoTracking()
            .Where(t => t.SprintId == sprintId && t.CurrentStatusId == statusId && !t.IsDeleted)
            .MaxAsync(t => (int?)t.OrderInSprint, ct);
        return (max ?? 0) + 1;
    }

    private async Task<WorkflowStatus> EnsureStatus(Guid statusId, CancellationToken ct)
        => await _db.WorkflowStatuses.AsNoTracking().FirstOrDefaultAsync(x => x.Id == statusId, ct)
           ?? throw CustomExceptionFactory.CreateNotFoundError(
                ResponseMessages.NOT_FOUND.FormatMessage("Workflow status"));

    private async Task<WorkflowStatus> ResolveStatusForWorkflow(Guid? currentStatusId, string? currentCode, Guid workflowId, CancellationToken ct)
    {
        var all = await _db.WorkflowStatuses.AsNoTracking()
            .Where(x => x.WorkflowId == workflowId)
            .OrderBy(x => x.Position)
            .ToListAsync(ct);

        if (currentStatusId != null && all.Any(x => x.Id == currentStatusId))
            return all.First(x => x.Id == currentStatusId);

        if (!string.IsNullOrWhiteSpace(currentCode))
        {
            var hit = all.FirstOrDefault(x => x.Code != null && x.Code.ToLower() == currentCode.ToLower());
            if (hit != null) return hit;
        }

        return all.FirstOrDefault(x => x.Category == "TODO") ?? all.First(); // fallback
    }

    private async Task<Guid> GetCompanyIdOfProject(Guid? projectId, CancellationToken ct)
        => await _db.Projects.Where(p => p.Id == projectId)
               .Select(p => (Guid)p.CompanyId).FirstAsync(ct);
    // trong TaskService
    private async Task<Guid> GetWorkflowIdForTask(Guid? projectId, Guid? currentStatusId, CancellationToken ct)
    {
        // Ưu tiên: từ status hiện tại
        if (currentStatusId.HasValue)
        {
            Guid? wfFromStatus = await _db.WorkflowStatuses.AsNoTracking()
                .Where(ws => ws.Id == currentStatusId.Value)
                .Select(ws => ws.WorkflowId)                 // Guid?
                .FirstOrDefaultAsync(ct);

            if (wfFromStatus.HasValue && wfFromStatus.Value != Guid.Empty)
                return wfFromStatus.Value;
        }

        // Fallback: từ project
        if (!projectId.HasValue)
            throw CustomExceptionFactory.CreateBadRequestError("Task has no ProjectId.");

        Guid? wfFromProject = await _db.Projects.AsNoTracking()
            .Where(p => p.Id == projectId.Value)
            .Select(p => p.WorkflowId)                       // Guid?
            .FirstOrDefaultAsync(ct);

        if (wfFromProject.HasValue && wfFromProject.Value != Guid.Empty)
            return wfFromProject.Value;

        throw CustomExceptionFactory.CreateBadRequestError("Workflow is not configured for this project.");
    }
    private async Task<Guid> GetWorkflowIdForMove(Guid? projectId, Guid? currentStatusId, CancellationToken ct)
    {
        // 1) Từ status hiện tại
        if (currentStatusId.HasValue)
        {
            Guid? wfFromStatus = await _db.WorkflowStatuses.AsNoTracking()
                .Where(ws => ws.Id == currentStatusId.Value)
                .Select(ws => (Guid?)ws.WorkflowId)   // ép về nullable để an toàn
                .FirstOrDefaultAsync(ct);

            if (wfFromStatus.HasValue && wfFromStatus.Value != Guid.Empty)
                return wfFromStatus.Value;
        }

        // 2) Từ Project (nếu có cột WorkflowId)
        if (projectId.HasValue)
        {
            Guid? wfFromProject = await _db.Projects.AsNoTracking()
                .Where(p => p.Id == projectId.Value)
                .Select(p => p.WorkflowId)            // Guid? trong model; nếu Guid thì EF tự nâng lên Guid?
                .FirstOrDefaultAsync(ct);

            if (wfFromProject.HasValue && wfFromProject.Value != Guid.Empty)
                return wfFromProject.Value;
        }

        // 3) Fallback: từ bất kỳ task nào của project
        if (projectId.HasValue)
        {
            Guid? wfAny = await _db.ProjectTasks.AsNoTracking()
                .Where(t => t.ProjectId == projectId.Value && t.CurrentStatusId != null)
                .Join(_db.WorkflowStatuses.AsNoTracking(),
                      t => t.CurrentStatusId,
                      ws => ws.Id,
                      (t, ws) => (Guid?)ws.WorkflowId)
                .FirstOrDefaultAsync(ct);

            if (wfAny.HasValue && wfAny.Value != Guid.Empty)
                return wfAny.Value;
        }

        throw CustomExceptionFactory.CreateBadRequestError("Cannot determine workflow for this project/task.");
    }



    #endregion
    #region Handle Task
    /* -------------------- Change status by Id -------------------- */
    public async Task<ProjectTaskResponse> ChangeStatusById(Guid id, Guid statusId, Guid userId, CancellationToken ct = default)
    {
        var e = await _repo.FindByIdAsync(id, ct)
            ?? throw CustomExceptionFactory.CreateNotFoundError(ResponseMessages.NOT_FOUND.FormatMessage("Task"));

        var st = await EnsureStatus(statusId, ct);
        e.CurrentStatusId = st.Id;
        e.Status = !string.IsNullOrWhiteSpace(st.Code) ? st.Code : st.Name;

        if (e.SprintId != null)
            e.OrderInSprint = await GetNextOrderInSprint(e.SprintId.Value, st.Id, ct);
        else
            e.OrderInSprint = null;

        e.UpdateAt = DateTime.UtcNow;
        await _repo.UpdateAsync(e, ct);
      
        var companyId = await GetCompanyIdOfProject(e.ProjectId, ct);

        await _log.CreateLog(new CompanyActivityLog
        {
            CompanyId = companyId,
            ActorUserId = _current.GetUserId(),
            Title = "Change task status",
            Description = $"User '{await GetUserName(_current.GetUserId())}' changed status of '{e.Title}' to '{st.Code ?? st.Name}'"
        });

        return _mapper.Map<ProjectTaskResponse>(e);
    }

    /* -------------------- Reorder (drag & drop) -------------------- */
    public async Task<ProjectTaskResponse> ReorderAsync(Guid projectId, Guid sprintId, Guid taskId, Guid toStatusId, int toIndex, Guid userId, CancellationToken ct = default)
    {
        await using var trx = await _db.Database.BeginTransactionAsync(ct);

        var task = await _db.ProjectTasks.FirstOrDefaultAsync(t => t.Id == taskId && !t.IsDeleted, ct)
            ?? throw CustomExceptionFactory.CreateNotFoundError(ResponseMessages.NOT_FOUND.FormatMessage("Task"));

        if (task.ProjectId != projectId)
            throw CustomExceptionFactory.CreateBadRequestError("Task does not belong to the project.");

        var sprint = await _db.Sprints.AsNoTracking().FirstOrDefaultAsync(s => s.Id == sprintId && s.ProjectId == projectId, ct)
            ?? throw CustomExceptionFactory.CreateNotFoundError(ResponseMessages.NOT_FOUND.FormatMessage("Sprint"));

        var toStatus = await EnsureStatus(toStatusId, ct);
       

        // Danh sách các task trong (sprint, toStatus)
        var list = await _db.ProjectTasks
            .Where(t => t.SprintId == sprintId && t.CurrentStatusId == toStatusId && !t.IsDeleted && t.Id != taskId)
            .OrderBy(t => t.OrderInSprint)
            .ToListAsync(ct);

        // Clamp index
        var insertAt = Math.Max(0, Math.Min(toIndex, list.Count));
        list.Insert(insertAt, task);

        // Nếu đổi cột thì cập nhật status + status text
        if (task.CurrentStatusId != toStatusId)
        {
            task.CurrentStatusId = toStatus.Id;
            task.Status = !string.IsNullOrWhiteSpace(toStatus.Code) ? toStatus.Code : toStatus.Name;
        }

        if (task.SprintId != sprintId) task.SprintId = sprintId;
        task.IsBacklog = false;

        // Re-number từ 1
        var order = 1;
        foreach (var t in list)
            t.OrderInSprint = order++;

        task.UpdateAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
        await trx.CommitAsync(ct);

        var companyId = await GetCompanyIdOfProject(task.ProjectId, ct);
        await _log.CreateLog(new CompanyActivityLog
        {
            CompanyId = companyId,
            ActorUserId = _current.GetUserId(),
            Title = "Reorder task",
            Description = $"User '{await GetUserName(_current.GetUserId())}' reordered task '{task.Title}'"
        });

        return _mapper.Map<ProjectTaskResponse>(task);
    }

    /* -------------------- Move to sprint -------------------- */
    public async Task<ProjectTaskResponse> MoveToSprintAsync(Guid taskId, Guid toSprintId, Guid userId, CancellationToken ct = default)
    {
        var task = await _repo.FindByIdAsync(taskId, ct)
            ?? throw CustomExceptionFactory.CreateNotFoundError(ResponseMessages.NOT_FOUND.FormatMessage("Task"));

        var toSprint = await _db.Sprints.FirstOrDefaultAsync(s => s.Id == toSprintId, ct)
            ?? throw CustomExceptionFactory.CreateNotFoundError(ResponseMessages.NOT_FOUND.FormatMessage("Sprint"));

        if (toSprint.ProjectId != task.ProjectId)
            throw CustomExceptionFactory.CreateBadRequestError("Sprint must be in the same project.");

        // Chuẩn hoá status theo workflow của sprint mới
        var curCode = task.Status;
        var wfId = await GetWorkflowIdForMove(task.ProjectId, task.CurrentStatusId, ct);

        // Chuẩn hoá status theo workflow của sprint mới
        var resolved = await ResolveStatusForWorkflow(task.CurrentStatusId, curCode, wfId, ct);
        task.SprintId = toSprint.Id;
        task.IsBacklog = false;
        task.CurrentStatusId = resolved.Id;
        task.Status = !string.IsNullOrWhiteSpace(resolved.Code) ? resolved.Code : resolved.Name;
        task.OrderInSprint = await GetNextOrderInSprint(toSprint.Id, resolved.Id, ct);
        task.CarryOverCount = Math.Max(0, task.CarryOverCount) + 1;
        task.UpdateAt = DateTime.UtcNow;

        await _repo.UpdateAsync(task, ct);

        var companyId = await GetCompanyIdOfProject(task.ProjectId, ct);
        await _log.CreateLog(new CompanyActivityLog
        {
            CompanyId = companyId,
            ActorUserId = _current.GetUserId(),
            Title = "Move task to sprint",
            Description = $"User '{await GetUserName(_current.GetUserId())}' moved task '{task.Title}' to sprint '{toSprint.Name}'"
        });

        return _mapper.Map<ProjectTaskResponse>(task);
    }

    /* -------------------- Mark done (đưa về final status) -------------------- */
    public async Task<ProjectTaskResponse> MarkDoneAsync(Guid taskId, Guid userId, CancellationToken ct = default)
    {
        var task = await _repo.FindByIdAsync(taskId, ct)
            ?? throw CustomExceptionFactory.CreateNotFoundError(ResponseMessages.NOT_FOUND.FormatMessage("Task"));

        // Lấy đúng workflowId mà task đang dùng
        var workflowId = await GetWorkflowIdForTask(task.ProjectId, task.CurrentStatusId, ct);

        // Lấy danh sách status theo workflow đó
        var statuses = await _db.WorkflowStatuses.AsNoTracking()
            .Where(x => x.WorkflowId == workflowId)
            .OrderBy(x => x.Position)
            .ToListAsync(ct);

        if (statuses.Count == 0)
            throw CustomExceptionFactory.CreateBadRequestError("Workflow has no statuses.");

        // Ưu tiên IsFinal; nếu không có thì lấy status cuối
        var finalStatus = statuses.FirstOrDefault(x => x.IsEnd)
                       ?? statuses.FirstOrDefault(x => x.Category == "DONE")
                       ?? statuses.Last();

        return await ChangeStatusById(taskId, finalStatus.Id, userId, ct);
    }


    /* -------------------- Split -------------------- */
    public async Task<SplitTaskResponse> SplitAsync(Guid taskId, Guid userId, CancellationToken ct = default)
{
    await using var trx = await _db.Database.BeginTransactionAsync(ct);

    var task = await _repo.FindByIdAsync(taskId, ct)
        ?? throw CustomExceptionFactory.CreateNotFoundError(ResponseMessages.NOT_FOUND.FormatMessage("Task"));

    if (task.SprintId == null)
        throw CustomExceptionFactory.CreateBadRequestError("Task must be in a sprint to split.");

    // 1) Tìm sprint kế tiếp trong cùng project
    var sprints = await _db.Sprints
        .Where(s => s.ProjectId == task.ProjectId)
        .OrderBy(s => s.StartDate) // dùng Start/StartAt nếu entity bạn khác tên
        .ToListAsync(ct);

    var curIdx = sprints.FindIndex(s => s.Id == task.SprintId);
    var next = curIdx >= 0 && curIdx < sprints.Count - 1 ? sprints[curIdx + 1] : null;

    if (next == null)
        throw CustomExceptionFactory.CreateBadRequestError("No next sprint found to place the new part.");

    // 2) Lấy workflowId đúng (không đọc từ Sprint)
    var wfId = await GetWorkflowIdForMove(task.ProjectId, task.CurrentStatusId, ct);

    // 3) Status đầu tiên trong workflow
    var firstStatus = await _db.WorkflowStatuses.AsNoTracking()
        .Where(x => x.WorkflowId == wfId)
        .OrderBy(x => x.Position)
        .FirstOrDefaultAsync(ct)
        ?? throw CustomExceptionFactory.CreateBadRequestError("Target workflow has no status.");

    // 4) Xác định root của chuỗi split + baseTitle
    var rootId = task.ParentTaskId ?? task.Id;
    var rootTitle = task.ParentTaskId.HasValue
        ? (await _db.ProjectTasks.AsNoTracking()
                .Where(x => x.Id == rootId)
                .Select(x => x.Title)
                .FirstOrDefaultAsync(ct)) ?? task.Title ?? ""
        : task.Title ?? "";
    var baseTitle = StripPartSuffix(rootTitle);

    // 5) Tính chữ cái kế tiếp theo số part con đã có của root (B, C, ..., AA, AB, ...)
    var existingChildren = await _db.ProjectTasks.AsNoTracking()
        .CountAsync(x => x.ParentTaskId == rootId && !x.IsDeleted, ct); // đếm B.. (không tính A vì root.ParentTaskId == null)
    var nextLetter = AlphaIndex(existingChildren + 2); // +2 vì A=1 (root), B=2 (part mới đầu tiên)

    // 6) Chia effort từ CHÍNH task đang split
    var sp = Math.Max(0, task.Point ?? 0);
    var rh = Math.Max(0, task.RemainingHours ?? 0);
    if (sp < 2 && rh < 2)
        throw CustomExceptionFactory.CreateBadRequestError("Task is too small to split.");

    var takePts = sp >= 2 ? sp / 2 : 0;          // phần chuyển sang part mới
    var keepPts = sp - takePts;                  // phần giữ lại ở task hiện tại
    var takeHrs = rh >= 2 ? rh / 2 : 0;
    var keepHrs = Math.Max(0, rh - takeHrs);

    // 7) Nếu đang split ROOT lần đầu => đổi tên thành (Part A); nếu không thì giữ nguyên tên hiện có
    var isRoot = task.ParentTaskId == null && task.Id == rootId;
    if (isRoot)
    {
        // chỉ đổi nếu chưa là Part A
        if (!Regex.IsMatch(task.Title ?? "", @"\s*\(Part\s+A\)\s*$", RegexOptions.IgnoreCase))
            task.Title = $"{baseTitle} (Part A)";
    }

    // Cập nhật effort cho task hiện tại (part đang bị cắt bớt)
    task.Point = keepPts;
    task.RemainingHours = keepHrs;
    task.UpdateAt = DateTime.UtcNow;

    // 8) Sinh code cho part mới
    var seq = await _db.ProjectTasks.AsNoTracking()
        .LongCountAsync(t => t.ProjectId == task.ProjectId, ct) + 1;
    var prefix = await _db.Projects.Where(p => p.Id == task.ProjectId)
        .Select(p => p.Code)
        .FirstOrDefaultAsync(ct) ?? "PRJ";
    var newCode = $"{prefix}-T-{seq:000}";

    // 9) Tạo part mới (Part B/C/…)
    var newPart = new ProjectTask
    {
        Id = Guid.NewGuid(),
        ProjectId = task.ProjectId,
        SprintId = next.Id,
        IsBacklog = false,
        Code = newCode,
        Title = $"{baseTitle} (Part {nextLetter})",
        Type = task.Type,
        Priority = task.Priority,
        Severity = task.Severity,
        Point = takePts,
        EstimateHours = task.EstimateHours,
        RemainingHours = takeHrs,
        DueDate = task.DueDate,
        ParentTaskId = rootId, // luôn trỏ về ROOT để FE nhóm mượt
        SourceTaskId = task.SourceTaskId,
        CurrentStatusId = firstStatus.Id,
        Status = !string.IsNullOrWhiteSpace(firstStatus.Code) ? firstStatus.Code : firstStatus.Name,
        OrderInSprint = await GetNextOrderInSprint(next.Id, firstStatus.Id, ct),
        CreateAt = DateTime.UtcNow,
        UpdateAt = DateTime.UtcNow,
        CreatedBy = userId,
        IsDeleted = false,
    };

    _db.ProjectTasks.Add(newPart);
    await _db.SaveChangesAsync(ct);
    await trx.CommitAsync(ct);

    var companyId = await GetCompanyIdOfProject(task.ProjectId, ct);
    await _log.CreateLog(new CompanyActivityLog
    {
        CompanyId = companyId,
        ActorUserId = _current.GetUserId(),
        Title = "Split task",
        Description = $"User '{await GetUserName(_current.GetUserId())}' split '{baseTitle}' → created '{newPart.Title}'"
    });

    return new SplitTaskResponse
    {
        PartA = _mapper.Map<ProjectTaskResponse>(task),     // task đã được cập nhật (nếu là root lần đầu thì là Part A)
        PartB = _mapper.Map<ProjectTaskResponse>(newPart),  // part mới (B/C/…)
    };
}


    #endregion
    #region CRUD
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
        entity.Assignees = new List<TaskWorkflow>();
        if (req.AssigneeIds?.Count > 0)
        {
            foreach (var uid in req.AssigneeIds.Distinct())
                entity.Assignees.Add(new TaskWorkflow { TaskId = entity.Id, AssignUserId = uid });
        }

        // 7) Persist
        await _repo.AddAsync(entity, ct);
        if (req.WorkflowAssignments != null)
        {
            var validItems = req.WorkflowAssignments
                .Where(x => x.AssignUserId.HasValue && x.AssignUserId.Value != Guid.Empty)
                .Select(x => new TaskWorkflowAssignmentItemRequest
                {
                    WorkflowStatusId = x.WorkflowStatusId,
                    AssignUserId = x.AssignUserId
                })
                .ToList();

            if (validItems.Count > 0)
            {
                var wfReq = new TaskWorkflowAssignmentsRequest
                {
                    TaskId = entity.Id,
                    Items = validItems
                };

                await _taskWorkflowService.UpsertAssignmentsForTaskAsync(
                    wfReq,
                    userId,
                    ct);
            }
        }


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
                e.Assignees.Add(new TaskWorkflow { TaskId = e.Id, AssignUserId = uid });
        }

        e.UpdateAt = DateTime.UtcNow;
        await _repo.UpdateAsync(e, ct);

        var companyId = await _db.Projects
       .Where(p => p.Id == e.ProjectId)
       .Select(p => (Guid)p.CompanyId)    // ép về Guid
       .FirstAsync(ct);
        if (req.WorkflowAssignments != null)
        {
            var wfReq = new TaskWorkflowAssignmentsRequest
            {
                TaskId = e.Id,
                Items = req.WorkflowAssignments
                    .Select(x => new TaskWorkflowAssignmentItemRequest
                    {
                        WorkflowStatusId = x.WorkflowStatusId,
                        AssignUserId = x.AssignUserId
                    })
                    .ToList()
            };

            await _taskWorkflowService.UpsertAssignmentsForTaskAsync(
                wfReq,
                userId,
                ct);
        }

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

        // map task trước
        var vm = _mapper.Map<ProjectTaskResponse>(e);

        var wfAssignments = await _taskWorkflowService.GetAssignmentsForTaskAsync(id, ct);
        vm.WorkflowAssignments = wfAssignments;

        return vm;
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
    public async Task<PagedResult<ProjectTaskResponse>> GetTasksBySprintIdAsync(Guid sprintId, TaskBySprintRequest request, CancellationToken ct = default)
    {
        var sprintExists = await _db.Sprints.AsNoTracking().AnyAsync(s => s.Id == sprintId, ct);
        if (!sprintExists)
            throw CustomExceptionFactory.CreateNotFoundError(
                ResponseMessages.NOT_FOUND.FormatMessage("Sprint"));

        var paged = await _repo.GetTasksBySprintIdAsync(sprintId, request, ct);

        return new PagedResult<ProjectTaskResponse>
        {
            Items = paged.Items.Select(_mapper.Map<ProjectTaskResponse>).ToList(),
            TotalCount = paged.TotalCount,
            PageNumber = paged.PageNumber,
            PageSize = paged.PageSize
        };
    }

    /* -------------------- Get Task By UserId -------------------- */
    public async Task<PagedResult<TaskResponse>> GetAllTaskByUserId(Guid userId, TaskFilterRequest request, CancellationToken token = default)
    {
        var taskData = await _repo.GetAllTaskByUserId(userId, request, token);

        var mapped = taskData.Items.Select(t => new TaskResponse
        {
            TaskId = t.Id,
            Code = t.Code ?? "",
            Title = t.Title ?? "",
            Img = t.Img,
            Type = t.Type?.ToString() ?? "Unknown",
            Priority = t.Priority?.ToString() ?? "None",
            Severity = t.Severity?.ToString() ?? "None",
            Status = t.CurrentStatus?.Name ?? "N/A",
            Point = t.Point,
            EstimateHours = t.EstimateHours,
            RemainingHours = t.RemainingHours,
            CarryOverCount = t.CarryOverCount,
            OrderInSprint = t.OrderInSprint,

            IsBacklog = t.IsBacklog,
            CreateAt = t.CreateAt,
            DueDate = t.DueDate,

            CreateBy = t.CreatedBy ?? Guid.Empty,
            CreateByName = t.CreatedByNavigation?.UserName ?? "Unknown",

            ParentTaskId = t.ParentTaskId,
            SourceTaskId = t.SourceTaskId,

            Project = t.Project == null ? null : new ProjectResponse
            {
                Id = t.Project.Id,
                Name = t.Project.Name ?? "",
                Code = t.Project.Code ?? "",
                CompanyHiredId = t.Project?.CompanyRequestId ?? Guid.Empty,
                CompanyId = t.Project?.CompanyId ?? Guid.Empty,
                Description = t.Project?.Description ?? "Unknown",
                IsHired = t.Project?.IsHired ?? false,
                Status = t.Project?.Status ?? "None",
                ProjectRequestId = t.Project?.ProjectRequestId ?? Guid.Empty,
                StartDate = t.Project?.StartDate,
                EndDate = t.Project?.EndDate,
                CreatedBy = t.Project?.CreatedBy ?? Guid.Empty,
            },

            Sprint = t.Sprint == null ? null : new SprintResponse
            {
                Id = t.Sprint.Id,
                Name = t.Sprint.Name ?? "",
                Start = t.Sprint.StartDate ?? DateTime.MinValue,
                End = t.Sprint.EndDate ?? DateTime.MinValue,
                CapacityHours = t.Sprint.CapacityHours ?? 0,
                Color = t.Sprint.Color ?? "Unknown",
                Status = t.Sprint.Status,
                CommittedPoints = t.Sprint.CommittedPoints ?? 0,
                CreatedAt = t.Sprint.CreatedAt ?? DateTime.MinValue,
                Goal = t.Sprint.Goal ?? "Unknown",
                IsDeleted = t.Sprint.IsDeleted,
            },

            WorkflowStatus = t.CurrentStatus == null ? null : new WorkflowStatusResponse
            {
                Id = t.CurrentStatus.Id,
                Name = t.CurrentStatus.Name ?? "Unknown",
                Position = t.CurrentStatus.Position,
                GuardNameKey = t.CurrentStatus.GuardNameKey ?? "Unknown",
                IsEnd = t.CurrentStatus.IsEnd,
                IsStart = t.CurrentStatus.IsStart,
                WorkflowId = t.CurrentStatus.WorkflowId
            },

            Members = (t.TaskWorkflows ?? new List<TaskWorkflow>())
                .Where(a => a.AssignUser != null)
                .Select(a => new ProjectMemberSummaryResponse
                {
                    MemberId = a.AssignUser?.Id ?? Guid.Empty,
                    MemberName = a.AssignUser?.UserName ?? "Unknown",
                    Avatar = a.AssignUser?.Avatar ?? "Unknown",
                })
                .ToList(),

            Checklist = (t.ChecklistItems ?? new List<ProjectTaskChecklistItem>())
                .Select(c => new TaskChecklistItemResponse
                {
                    Id = c.Id,
                    Label = c.Label ?? "Unknown",
                    IsDone = c.IsDone,
                    OrderIndex = c.OrderIndex,
                    CreatedAt = c.CreatedAt,
                    TaskId = c.TaskId,
                })
                .ToList(),

            Dependencies = (t.Dependencies ?? new List<ProjectTaskDependency>())
                .Where(d => d.DependsOnTask != null)
                .Select(d => new TaskDependencyResponse
                {
                    TaskId = d.DependsOnTaskId,
                    Title = d.DependsOnTask?.Title ?? "",
                    Code = d.DependsOnTask?.Code ?? "",
                    Priority = d.DependsOnTask?.Priority?.ToString() ?? "",
                    Status = d.DependsOnTask?.CurrentStatus?.Name ?? "N/A",
                    Point = d.DependsOnTask?.Point,
                    EstimateHours = d.DependsOnTask?.EstimateHours
                })
                .ToList(),

            Comments = (t.Comments ?? new List<Comment>())
                .Select(c => new CommentResponse 
                { 
                    Id = c.Id,
                    AuthorUserId = c.AuthorUserId ?? Guid.Empty,
                    Body = c.Body ?? "Unknown",
                    CreateAt = c.CreateAt,
                    Status = c.Status ?? "Unknown",
                    UpdateAt = c.UpdateAt,
                    TaskId = c.TaskId
                })
                .ToList()
        }).ToList();

        return new PagedResult<TaskResponse> {
            Items = mapped,
            TotalCount = taskData.TotalCount,
            PageNumber = taskData.PageNumber,
            PageSize = taskData.PageSize,
        };
    }

    #endregion
}
