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
using Fusion.Service.ViewModels.AITaskGenerate;
using Fusion.Service.ViewModels.Comment.Response;
using Fusion.Service.ViewModels.Project.Responses;
using Fusion.Service.ViewModels.ProjectMembers.Responses;
using Fusion.Service.ViewModels.Projects.Responses;
using Fusion.Service.ViewModels.Sprint.Responses;
using Fusion.Service.ViewModels.Task.Request;
using Fusion.Service.ViewModels.Task.Response;
using Fusion.Service.ViewModels.WorkflowStatus;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Fusion.Service.Services;

public class TaskService : ITaskService
{
    private readonly FusionDbContext _db;           
    private readonly ITaskRepository _repo;         
    private readonly IMapper _mapper;
    private readonly ICompanyActivityService _log;
    private readonly ICurrentService _current;
    private readonly ITaskWorkflowService _taskWorkflowService;
    private readonly ICloudinaryService _cloudinary;

    public TaskService(
        FusionDbContext db,
        ITaskRepository repo,
        IMapper mapper,
        ICompanyActivityService log,
        ICurrentService current,
        ITaskWorkflowService taskWorkflowService,
        ICloudinaryService cloudinary)
    {
        _db = db;
        _repo = repo;
        _mapper = mapper;
        _log = log;
        _current = current;
        _taskWorkflowService = taskWorkflowService;
        _cloudinary = cloudinary;
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
        var max = await _db.ProjectTasks.AsNoTracking().Include(t => t.Ticket)
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
    private async Task<Guid> GetWorkflowIdForTask(Guid? projectId, Guid? currentStatusId, CancellationToken ct)
    {
        if (currentStatusId.HasValue)
        {
            Guid? wfFromStatus = await _db.WorkflowStatuses.AsNoTracking()
                .Where(ws => ws.Id == currentStatusId.Value)
                .Select(ws => ws.WorkflowId)                 // Guid?
                .FirstOrDefaultAsync(ct);

            if (wfFromStatus.HasValue && wfFromStatus.Value != Guid.Empty)
                return wfFromStatus.Value;
        }

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
            Guid? wfAny = await _db.ProjectTasks.AsNoTracking().Include(t => t.Ticket)
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
    public async Task<ProjectTaskResponse> ChangeStatusById(
      Guid id, Guid statusId, Guid userId, CancellationToken ct = default)
    {
        var e = await _repo.FindByIdAsync(id, ct)
            ?? throw CustomExceptionFactory.CreateNotFoundError(
                ResponseMessages.NOT_FOUND.FormatMessage("Task"));

        var st = await EnsureStatus(statusId, ct);

        var oldStatusId = e.CurrentStatusId;          // status cũ

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

        // CHỈ notify nếu thực sự đổi status
        if (oldStatusId != st.Id)
        {
            await _taskWorkflowService.NotifyAssigneeOnStatusChangeAsync(
        e.Id,
        oldStatusId,   
        st.Id,
        userId,
        ct);
        }

        return _mapper.Map<ProjectTaskResponse>(e);
    }


    /* -------------------- Reorder (drag & drop) -------------------- */
    public async Task<ProjectTaskResponse> ReorderAsync(
     Guid projectId,
     Guid sprintId,
     Guid taskId,
     Guid toStatusId,
     int toIndex,
     Guid userId,
     CancellationToken ct = default)
    {
        await using var trx = await _db.Database.BeginTransactionAsync(ct);

        var task = await _db.ProjectTasks
            .FirstOrDefaultAsync(t => t.Id == taskId && !t.IsDeleted, ct)
            ?? throw CustomExceptionFactory.CreateNotFoundError(
                ResponseMessages.NOT_FOUND.FormatMessage("Task"));

        if (task.ProjectId != projectId)
            throw CustomExceptionFactory.CreateBadRequestError("Task does not belong to the project.");

        var sprint = await _db.Sprints.AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == sprintId && s.ProjectId == projectId, ct)
            ?? throw CustomExceptionFactory.CreateNotFoundError(
                ResponseMessages.NOT_FOUND.FormatMessage("Sprint"));

        var toStatus = await EnsureStatus(toStatusId, ct);

        var oldStatusId = task.CurrentStatusId;
        var changedStatus = oldStatusId != toStatus.Id;    // CHỈ gửi notify nếu true

        // Danh sách các task trong (sprint, toStatus)
        var list = await _db.ProjectTasks
            .Where(t => t.SprintId == sprintId && t.CurrentStatusId == toStatusId && !t.IsDeleted && t.Id != taskId)
            .OrderBy(t => t.OrderInSprint)
            .ToListAsync(ct);

        // Clamp index
        var insertAt = Math.Max(0, Math.Min(toIndex, list.Count));
        list.Insert(insertAt, task);

        // Nếu đổi cột thì cập nhật status + status text
        if (changedStatus)
        {
            task.CurrentStatusId = toStatus.Id;
            task.Status = !string.IsNullOrWhiteSpace(toStatus.Code) ? toStatus.Code : toStatus.Name;
        }

        if (task.SprintId != sprintId)
            task.SprintId = sprintId;

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

        // CHỈ notify khi đổi status (kéo qua cột khác), không notify khi chỉ reorder trong cùng cột
        if (changedStatus)
        {
            await _taskWorkflowService.NotifyAssigneeOnStatusChangeAsync(
         task.Id,
         oldStatusId,     
         toStatus.Id,    
         userId,
         ct);
        }

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
        ParentTaskId = rootId, 
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

        var vm = _mapper.Map<ProjectTaskResponse>(e);

        var wfAssignments = await _taskWorkflowService.GetAssignmentsForTaskAsync(id, ct);
        vm.WorkflowAssignments = wfAssignments;

        // NEW: load comments chuẩn, sort mới nhất lên đầu, có attachments
        var comments = await GetCommentsByTaskIdAsync(id, ct);
        vm.Comments = comments.ToList();

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

        var responseItems = new List<ProjectTaskResponse>();

        foreach (var t in paged.Items)
        {
            var resp = _mapper.Map<ProjectTaskResponse>(t);

            // AssigneeIds
            resp.AssigneeIds = t.TaskWorkflows?.Select(a => a.AssignUserId.Value).ToList() ?? new List<Guid>();

            // await lấy workflow assignments
            resp.WorkflowAssignments = await _taskWorkflowService.GetAssignmentsForTaskAsync(resp.Id, ct);

            responseItems.Add(resp);
        }

        return new PagedResult<ProjectTaskResponse>
        {
            Items = responseItems,
            TotalCount = paged.TotalCount,
            PageNumber = paged.PageNumber,
            PageSize = paged.PageSize
        };
    }

    /* -------------------- Get Task By UserId -------------------- */
    public async Task<PagedResult<TaskResponse>> GetAllTaskByUserId(Guid userId, TaskFilterRequest request, CancellationToken token = default)
    {
        var taskData = await _repo.GetAllTaskByUserId(userId, request, token);

        if (!taskData.Items.Any())
        {
            throw CustomExceptionFactory.CreateNotFoundError(
                ResponseMessages.NOT_FOUND.FormatMessage("List Task"));
        }

        var mapped = taskData.Items.Select(t => new TaskResponse
        {
            TaskId = t.Id,
            Code = t.Code ?? "",
            Title = t.Title ?? "",
            Img = t.Img,
            Description = t.Description ?? "",
            Point = t.Point ?? 0,
            Type = t.Type?.ToString() ?? "Unknown",
            Priority = t.Priority?.ToString() ?? "None",
            Severity = t.Severity?.ToString() ?? "None",
            Status = t.Status?.ToString() ?? "None",
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
            TicketId = t.TicketId,
          
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
                WorkflowId = t.Project.WorkflowId ?? Guid.Empty,
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
                WorkflowId = t.CurrentStatus.WorkflowId,
                Color = t.CurrentStatus.Color
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

        return new PagedResult<TaskResponse>
        {
            Items = mapped,
            TotalCount = taskData.TotalCount,
            PageNumber = taskData.PageNumber,
            PageSize = taskData.PageSize,
        };
    }

    public async Task<TaskResponse> GetTaskDetailByTaskIdAsync(Guid userId, Guid taskId, CancellationToken token = default)
    {
        var t = await _repo.GetTaskDetailByTaskIdAsync(userId, taskId, token);

        if (t == null)
            throw CustomExceptionFactory.CreateNotFoundError(
                ResponseMessages.NOT_FOUND.FormatMessage("Task"));

        return new TaskResponse
        {
            TaskId = t.Id,
            Code = t.Code ?? "",
            Title = t.Title ?? "",
            Img = t.Img,
            Point = t.Point ?? 0,
            Description = t.Description ?? "",
            Type = t.Type?.ToString() ?? "Unknown",
            Priority = t.Priority?.ToString() ?? "None",
            Severity = t.Severity?.ToString() ?? "None",
            Status = t.Status?.ToString() ?? "None",
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
                Name = t.Project?.Name ?? "",
                Code = t.Project?.Code ?? "",
                CompanyHiredId = t.Project?.CompanyRequestId ?? Guid.Empty,
                CompanyId = t.Project?.CompanyId ?? Guid.Empty,
                Description = t.Project?.Description ?? "Unknown",
                IsHired = t.Project?.IsHired ?? false,
                Status = t.Project?.Status ?? "None",
                ProjectRequestId = t.Project?.ProjectRequestId ?? Guid.Empty,
                StartDate = t.Project?.StartDate,
                EndDate = t.Project?.EndDate,
                CreatedBy = t.Project?.CreatedBy ?? Guid.Empty,
                WorkflowId = t.Project?.WorkflowId ?? Guid.Empty,
            },

            Sprint = t.Sprint == null ? null : new SprintResponse
            {
                Id = t.Sprint.Id,
                Name = t.Sprint?.Name ?? "",
                Start = t.Sprint?.StartDate ?? DateTime.MinValue,
                End = t.Sprint?.EndDate ?? DateTime.MinValue,
                CapacityHours = t.Sprint?.CapacityHours ?? 0,
                Color = t.Sprint?.Color ?? "Unknown",
                Status = t.Sprint.Status,
                CommittedPoints = t.Sprint?.CommittedPoints ?? 0,
                CreatedAt = t.Sprint?.CreatedAt ?? DateTime.MinValue,
                Goal = t.Sprint?.Goal ?? "Unknown",
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

            TaskAttachments = (t.Attachments ?? new List<ProjectTaskAttachment>())
                .Select(a => new TaskAttachmentResponse
                {
                    TaskId = a.TaskId,
                    Id = a.Id,
                    ContentType = a.ContentType ?? "Unknown",
                    Description = a.Description ?? "Unknown",
                    FileName = a.FileName ?? "Unknown",
                    IsImage = a.IsImage,
                    Size = a?.SizeBytes ?? 0L,
                    UploadedAt = a?.UploadedAt ?? DateTime.MinValue,
                    Url = a.Url ?? "Unknown",
                    UploadedBy = a.UploadedBy,
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
                    Body = c.Body ?? "",
                    CreateAt = c.CreateAt,
                    Status = c.Status ?? "",
                    UpdateAt = c.UpdateAt,
                    TaskId = c.TaskId
                })
                .ToList()
        };
    }

    public async Task<TaskResponse> GetTaskDetailForAdminByTaskIdAsync(Guid userId, Guid taskId, CancellationToken token = default)
    {
        var t = await _repo.GetTaskDetailForAdminByTaskIdAsync(userId, taskId, token);

        if (t == null)
            throw CustomExceptionFactory.CreateNotFoundError(
                ResponseMessages.NOT_FOUND.FormatMessage("Task"));

        return new TaskResponse
        {
            TaskId = t.Id,
            Code = t.Code ?? "",
            Title = t.Title ?? "",
            Img = t.Img,
            Point = t.Point ?? 0,
            Description = t.Description ?? "",
            Type = t.Type?.ToString() ?? "Unknown",
            Priority = t.Priority?.ToString() ?? "None",
            Severity = t.Severity?.ToString() ?? "None",
            Status = t.Status?.ToString() ?? "None",
            EstimateHours = t.EstimateHours,
            RemainingHours = t.RemainingHours,
            CarryOverCount = t.CarryOverCount,
            OrderInSprint = t.OrderInSprint,

            IsBacklog = t.IsBacklog,
            IsDeleted = t.IsDeleted,
            CreateAt = t.CreateAt,
            DueDate = t.DueDate,

            CreateBy = t.CreatedBy ?? Guid.Empty,
            CreateByName = t.CreatedByNavigation?.UserName ?? "Unknown",

            ParentTaskId = t.ParentTaskId,
            SourceTaskId = t.SourceTaskId,

            Project = t.Project == null ? null : new ProjectResponse
            {
                Id = t.Project.Id,
                Name = t.Project?.Name ?? "",
                Code = t.Project?.Code ?? "",
                CompanyHiredId = t.Project?.CompanyRequestId ?? Guid.Empty,
                CompanyId = t.Project?.CompanyId ?? Guid.Empty,
                Description = t.Project?.Description ?? "Unknown",
                IsHired = t.Project?.IsHired ?? false,
                Status = t.Project?.Status ?? "None",
                ProjectRequestId = t.Project?.ProjectRequestId ?? Guid.Empty,
                StartDate = t.Project?.StartDate,
                EndDate = t.Project?.EndDate,
                CreatedBy = t.Project?.CreatedBy ?? Guid.Empty,
                WorkflowId = t.Project?.WorkflowId ?? Guid.Empty,
            },

            Sprint = t.Sprint == null ? null : new SprintResponse
            {
                Id = t.Sprint.Id,
                Name = t.Sprint?.Name ?? "",
                Start = t.Sprint?.StartDate ?? DateTime.MinValue,
                End = t.Sprint?.EndDate ?? DateTime.MinValue,
                CapacityHours = t.Sprint?.CapacityHours ?? 0,
                Color = t.Sprint?.Color ?? "Unknown",
                Status = t.Sprint.Status,
                CommittedPoints = t.Sprint?.CommittedPoints ?? 0,
                CreatedAt = t.Sprint?.CreatedAt ?? DateTime.MinValue,
                Goal = t.Sprint?.Goal ?? "Unknown",
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

            TaskAttachments = (t.Attachments ?? new List<ProjectTaskAttachment>())
                .Select(a => new TaskAttachmentResponse
                {
                    TaskId = a.TaskId,
                    Id = a.Id,
                    ContentType = a.ContentType ?? "Unknown",
                    Description = a.Description ?? "Unknown",
                    FileName = a.FileName ?? "Unknown",
                    IsImage = a.IsImage,
                    Size = a?.SizeBytes ?? 0L,
                    UploadedAt = a?.UploadedAt ?? DateTime.MinValue,
                    Url = a.Url ?? "Unknown",
                    UploadedBy = a.UploadedBy,
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
                    Body = c.Body ?? "",
                    CreateAt = c.CreateAt,
                    Status = c.Status ?? "",
                    UpdateAt = c.UpdateAt,
                    TaskId = c.TaskId
                })
                .ToList()
        };
    }

    public async Task<List<ProjectTaskResponse>> GetSubTasksByTaskIdAsync(Guid userId,Guid taskId, CancellationToken token = default)
    {
        var subTasks = await _repo.GetSubTasksByTaskIdAsync(userId ,taskId, token);

        if (!subTasks.Any())
            throw CustomExceptionFactory.CreateNotFoundError($"Task with Id {taskId} do not have any subtasks");

        return _mapper.Map<List<ProjectTaskResponse>>(subTasks);
    }

    #endregion
    #region Attachments

    public async Task<IReadOnlyList<TaskAttachmentResponse>> UploadAttachmentsAsync(
    Guid taskId,
    IReadOnlyList<IFormFile> files,
    string? description,
    Guid userId,
    CancellationToken ct = default)
    {
        if (files == null || files.Count == 0)
            throw CustomExceptionFactory.CreateBadRequestError("No files to upload.");

        var task = await _db.ProjectTasks
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == taskId && !t.IsDeleted, ct);

        if (task == null)
            throw CustomExceptionFactory.CreateNotFoundError(
                ResponseMessages.NOT_FOUND.FormatMessage("Task"));

        var attachments = new List<ProjectTaskAttachment>();

        foreach (var file in files)
        {
            if (file == null || file.Length == 0) continue;

            var upload = await _cloudinary.UploadFileAsync(file, "task-attachments", ct);

            var entity = new ProjectTaskAttachment
            {
                Id = Guid.NewGuid(),
                TaskId = taskId,
                FileName = file.FileName,
                Url = upload.Url,
                ContentType = file.ContentType,
                SizeBytes = file.Length,
                Description = description,
                UploadedAt = DateTime.UtcNow,
                UploadedBy = userId,
                PublicId = upload.PublicId, // KHÔNG ĐỂ NULL nữa
                IsImage = upload.IsImage
            };

            attachments.Add(entity);
            _db.ProjectTaskAttachments.Add(entity);
        }

        await _db.SaveChangesAsync(ct);

        // map sang response
        var userName = await GetUserName(userId);

        return attachments.Select(a => new TaskAttachmentResponse
        {
            Id = a.Id,
            TaskId = a.TaskId,
            FileName = a.FileName,
            Url = a.Url,
            ContentType = a.ContentType,
            Size = a.SizeBytes,
            Description = a.Description,
            UploadedAt = a.UploadedAt,
            UploadedBy = a.UploadedBy,
            UploadedByName = userName,
            IsImage = a.IsImage
        }).ToList();
    }



    public async Task<IReadOnlyList<TaskAttachmentResponse>> GetAttachmentsAsync(
     Guid taskId,
     CancellationToken ct = default)
    {
        var taskExists = await _db.ProjectTasks
            .AnyAsync(t => t.Id == taskId && !t.IsDeleted, ct);

        if (!taskExists)
            throw CustomExceptionFactory.CreateNotFoundError(
                ResponseMessages.NOT_FOUND.FormatMessage("Task"));

        var list = await _db.ProjectTaskAttachments
            .AsNoTracking()
            .Where(a => a.TaskId == taskId && a.CommentId == null) // chỉ file gắn trực tiếp task
            .OrderByDescending(a => a.UploadedAt)
            .ToListAsync(ct);

        var result = new List<TaskAttachmentResponse>();

        foreach (var a in list)
        {
            result.Add(new TaskAttachmentResponse
            {
                Id = a.Id,
                TaskId = a.TaskId,
                FileName = a.FileName,
                Url = a.Url,
                ContentType = a.ContentType,
                Size = a.SizeBytes,
                Description = a.Description,
                UploadedAt = a.UploadedAt,
                UploadedBy = a.UploadedBy,
                UploadedByName = await GetUserName(a.UploadedBy),
                IsImage = a.IsImage
            });
        }

        return result;
    }


    public async Task<bool> DeleteAttachmentAsync(
        Guid taskId,
        Guid attachmentId,
        Guid userId,
        CancellationToken ct = default)
    {
        var attachment = await _db.ProjectTaskAttachments
            .FirstOrDefaultAsync(a => a.Id == attachmentId && a.TaskId == taskId, ct);

        if (attachment == null)
            throw CustomExceptionFactory.CreateNotFoundError(
                ResponseMessages.NOT_FOUND.FormatMessage("Attachment"));

        // xoá trên Cloudinary nếu có
        if (!string.IsNullOrWhiteSpace(attachment.Url))
        {
            var publicId = _cloudinary.ExtractPublicIdFromUrl(attachment.Url);
            if (!string.IsNullOrEmpty(publicId))
            {
                await _cloudinary.DeleteImageAsync(publicId, ct);
            }
        }

        _db.ProjectTaskAttachments.Remove(attachment);
        await _db.SaveChangesAsync(ct);

        return true;
    }

    #endregion
    #region Comments

    // Lấy comment của 1 task – sort mới nhất lên đầu, kèm author + attachments
    public async Task<IReadOnlyList<CommentResponse>> GetCommentsByTaskIdAsync(
        Guid taskId,
        CancellationToken ct = default)
    {
        var taskExists = await _db.ProjectTasks
            .AnyAsync(t => t.Id == taskId && !t.IsDeleted, ct);

        if (!taskExists)
            throw CustomExceptionFactory.CreateNotFoundError(
                ResponseMessages.NOT_FOUND.FormatMessage("Task"));

        var comments = await _db.Comments
            .Where(c => c.TaskId == taskId)
            .OrderByDescending(c => c.CreateAt) // comment mới nhất lên đầu
            .ToListAsync(ct);

        if (!comments.Any())
            return Array.Empty<CommentResponse>();

        var authorIds = comments
            .Where(c => c.AuthorUserId.HasValue)
            .Select(c => c.AuthorUserId!.Value)
            .Distinct()
            .ToList();

        var authors = await _db.Users
            .Where(u => authorIds.Contains(u.Id))
            .Select(u => new { u.Id, u.UserName, u.Avatar })
            .ToListAsync(ct);

        var authorLookup = authors.ToDictionary(a => a.Id, a => a);

        var commentIds = comments.Select(c => c.Id).ToList();

        var attachments = await _db.ProjectTaskAttachments
            .AsNoTracking()
            .Where(a => a.CommentId != null && commentIds.Contains(a.CommentId.Value))
            .ToListAsync(ct);

        var attLookup = attachments
            .GroupBy(a => a.CommentId!.Value)
            .ToDictionary(g => g.Key, g => g.ToList());

        var result = new List<CommentResponse>(comments.Count);

        foreach (var c in comments)
        {
            authorLookup.TryGetValue(c.AuthorUserId ?? Guid.Empty, out var author);

            var attForThis = attLookup.TryGetValue(c.Id, out var list)
                ? list
                : new List<ProjectTaskAttachment>();

            result.Add(new CommentResponse
            {
                Id = c.Id,
                TaskId = c.TaskId,
                AuthorUserId = c.AuthorUserId ?? Guid.Empty,
                AuthorName = author?.UserName ?? "Unknown",
                AuthorAvatar = author?.Avatar,
                Body = c.Body ?? "",
                Status = c.Status ?? "Active",
                CreateAt = c.CreateAt,
                UpdateAt = c.UpdateAt,
                Attachments = attForThis.Select(a => new CommentAttachmentResponse
                {
                    Id = a.Id,
                    CommentId = c.Id,
                    FileName = a.FileName,
                    Url = a.Url,
                    ContentType = a.ContentType,
                    Size = a.SizeBytes,
                    IsImage = a.IsImage
                }).ToList()
            });
        }

        return result;
    }

    // Thêm comment mới + upload file/ảnh/video cho comment
    public async Task<CommentResponse> AddCommentAsync(
        Guid taskId,
        string? body,
        IReadOnlyList<IFormFile>? files,
        Guid userId,
        CancellationToken ct = default)
    {
        var task = await _db.ProjectTasks
            .FirstOrDefaultAsync(t => t.Id == taskId && !t.IsDeleted, ct);

        if (task == null)
            throw CustomExceptionFactory.CreateNotFoundError(
                ResponseMessages.NOT_FOUND.FormatMessage("Task"));

        if (string.IsNullOrWhiteSpace(body) &&
            (files == null || files.Count == 0))
        {
            throw CustomExceptionFactory.CreateBadRequestError(
                "Comment cannot be empty.");
        }

        var now = DateTime.UtcNow;

        var comment = new Comment
        { 
            TaskId = taskId,
            AuthorUserId = userId,
            Body = body?.Trim(),
            Status = "Active",
            CreateAt = now,
            UpdateAt = now
        };

        _db.Comments.Add(comment);

        var attachmentEntities = new List<ProjectTaskAttachment>();

        if (files != null && files.Count > 0)
        {
            foreach (var file in files)
            {
                if (file == null || file.Length == 0) continue;

                var upload = await _cloudinary.UploadFileAsync(
                    file,
                    "task-comments",
                    ct);

                var entity = new ProjectTaskAttachment
                {
                    Id = Guid.NewGuid(),
                    TaskId = taskId,
                    CommentId = comment.Id,                 // GẮN VỚI COMMENT
                    FileName = file.FileName,
                    Url = upload.Url,
                    ContentType = file.ContentType,
                    SizeBytes = file.Length,
                    Description = null,
                    UploadedAt = now,
                    UploadedBy = userId,
                    PublicId = upload.PublicId,
                    IsImage = upload.IsImage
                };

                attachmentEntities.Add(entity);
                _db.ProjectTaskAttachments.Add(entity);
            }
        }

        await _db.SaveChangesAsync(ct);

        var author = await _db.Users
            .AsNoTracking()
            .Where(u => u.Id == userId)
            .Select(u => new { u.UserName, u.Avatar })
            .FirstOrDefaultAsync(ct);

        return new CommentResponse
        {
            Id = comment.Id,
            TaskId = comment.TaskId,
            AuthorUserId = userId,
            AuthorName = author?.UserName ?? "Unknown",
            AuthorAvatar = author?.Avatar,
            Body = comment.Body ?? "",
            Status = comment.Status ?? "Active",
            CreateAt = comment.CreateAt,
            UpdateAt = comment.UpdateAt,
            Attachments = attachmentEntities.Select(a => new CommentAttachmentResponse
            {
                Id = a.Id,
                CommentId = comment.Id,
                FileName = a.FileName,
                Url = a.Url,
                ContentType = a.ContentType,
                Size = a.SizeBytes,
                IsImage = a.IsImage
            }).ToList()
        };
    }

    #endregion
    #region Draft tasks (IsBacklog == true, SprintId == null)

    public async Task<ProjectTaskResponse> CreateDraftTaskAsync(
        ProjectTaskRequest req,
        Guid userId,
        CancellationToken ct = default)
    {
        if (req.ProjectId == null || req.ProjectId == Guid.Empty)
            throw CustomExceptionFactory.CreateBadRequestError("ProjectId is required for draft task.");

        // 1) Validate project
        var project = await _db.Projects.AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == req.ProjectId, ct)
            ?? throw CustomExceptionFactory.CreateNotFoundError(
                ResponseMessages.NOT_FOUND.FormatMessage("Project"));

        // 2) Draft: luôn backlog, không thuộc sprint
        req.SprintId = null;

        // 3) Chọn Status: ưu tiên WorkflowStatusId → StatusCode → default (TODO/first)
        WorkflowStatus? status = null;

        if (req.WorkflowStatusId != null)
        {
            status = await _db.WorkflowStatuses.AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == req.WorkflowStatusId, ct);
        }

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
                    .OrderBy(x => x.Position)
                    .FirstOrDefaultAsync(ct)
                ?? throw CustomExceptionFactory.CreateBadRequestError("No workflow status available.");
        }

        // 4) Sinh Code PRJ-T-###
        var seq = await _db.ProjectTasks.AsNoTracking()
            .LongCountAsync(t => t.ProjectId == req.ProjectId, ct) + 1;
        var prefix = string.IsNullOrWhiteSpace(project.Code) ? "PRJ" : project.Code!;
        var code = $"{prefix}-T-{seq:000}";

        // 5) Map + defaults cho draft
        var now = DateTime.UtcNow;
        var entity = _mapper.Map<ProjectTask>(req);
        entity.Id = Guid.NewGuid();
        entity.ProjectId = req.ProjectId;
        entity.SprintId = null;             // draft: không sprint
        entity.IsBacklog = true;           // draft = backlog
        entity.Code = code;
        entity.CurrentStatusId = status.Id;
        entity.Status = !string.IsNullOrWhiteSpace(status.Code) ? status.Code : status.Name;
        entity.OrderInSprint = null;       // không nằm trong sprint => không order
        entity.RemainingHours = req.EstimateHours;
        entity.CreateAt = now;
        entity.UpdateAt = now;
        entity.CreatedBy = userId;
        entity.IsDeleted = false;

        // Assignees
        entity.Assignees = new List<TaskWorkflow>();
        if (req.AssigneeIds?.Count > 0)
        {
            foreach (var assignUserId in req.AssigneeIds.Distinct())
                entity.Assignees.Add(new TaskWorkflow
                {
                    TaskId = entity.Id,
                    AssignUserId = assignUserId
                });
        }

        // 6) Persist
        await _repo.AddAsync(entity, ct);

        // 7) Workflow assignments (nếu FE gửi)
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
            .Select(p => (Guid)p.CompanyId)
            .FirstAsync(ct);

        // 8) Log
        await _log.CreateLog(new CompanyActivityLog
        {
            CompanyId = companyId,
            ActorUserId = _current.GetUserId(),
            Title = "Create draft task",
            Description = $"User '{await GetUserName(_current.GetUserId())}' created draft task '{entity.Title}'"
        });

        return _mapper.Map<ProjectTaskResponse>(entity);
    }

    public async Task<ProjectTaskResponse> UpdateDraftTaskAsync(
        ProjectTaskRequest req,
        Guid userId,
        CancellationToken ct = default)
    {
        var e = await _repo.FindByIdAsync(req.Id, ct)
            ?? throw CustomExceptionFactory.CreateNotFoundError(
                ResponseMessages.NOT_FOUND.FormatMessage("Draft task"));

        // Chỉ cho sửa draft
        if (!e.IsBacklog || e.SprintId != null)
            throw CustomExceptionFactory.CreateBadRequestError("Task is not a draft task.");

        // Update các trường cơ bản
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

        // Draft: luôn backlog, không thuộc sprint
        e.SprintId = null;
        e.IsBacklog = true;
        e.OrderInSprint = null;

        // Đổi status nếu FE gửi (được phép)
        if (req.WorkflowStatusId != null || !string.IsNullOrWhiteSpace(req.StatusCode))
        {
            WorkflowStatus? st = null;
            if (req.WorkflowStatusId != null)
            {
                st = await _db.WorkflowStatuses.AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Id == req.WorkflowStatusId, ct);
            }

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
            }
        }

        // Assignees (replace-all) nếu FE gửi
        if (req.AssigneeIds != null)
        {
            e.Assignees.Clear();
            foreach (var assignUserId in req.AssigneeIds.Distinct())
                e.Assignees.Add(new TaskWorkflow
                {
                    TaskId = e.Id,
                    AssignUserId = assignUserId
                });
        }

        e.UpdateAt = DateTime.UtcNow;
        await _repo.UpdateAsync(e, ct);

        var companyId = await _db.Projects
            .Where(p => p.Id == e.ProjectId)
            .Select(p => (Guid)p.CompanyId)
            .FirstAsync(ct);

        // Workflow assignments (nếu FE gửi)
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
            Title = "Update draft task",
            Description = $"User '{await GetUserName(_current.GetUserId())}' updated draft task '{e.Title}'"
        });

        return _mapper.Map<ProjectTaskResponse>(e);
    }

    public async Task<ProjectTaskResponse> GetDraftTaskByIdAsync(
        Guid id,
        CancellationToken ct = default)
    {
        var e = await _repo.FindByIdAsync(id, ct)
            ?? throw CustomExceptionFactory.CreateNotFoundError(
                ResponseMessages.NOT_FOUND.FormatMessage("Draft task"));

        if (!e.IsBacklog || e.SprintId != null)
            throw CustomExceptionFactory.CreateNotFoundError(
                ResponseMessages.NOT_FOUND.FormatMessage("Draft task"));

        var vm = _mapper.Map<ProjectTaskResponse>(e);

        vm.WorkflowAssignments = await _taskWorkflowService.GetAssignmentsForTaskAsync(id, ct);
        var comments = await GetCommentsByTaskIdAsync(id, ct);
        vm.Comments = comments.ToList();

        return vm;
    }

    public async Task<PagedResult<ProjectTaskResponse>> GetDraftTasksByProjectIdAsync(
        Guid projectId,
        PagedRequest request,
        CancellationToken ct = default)
    {
        // Validate project tồn tại
        var projectExists = await _db.Projects.AsNoTracking()
            .AnyAsync(p => p.Id == projectId, ct);

        if (!projectExists)
            throw CustomExceptionFactory.CreateNotFoundError(
                ResponseMessages.NOT_FOUND.FormatMessage("Project"));

        var query = _db.ProjectTasks
            .AsNoTracking()
            .Include(t => t.TaskWorkflows)
            .Where(t =>
                t.ProjectId == projectId &&
                t.IsBacklog &&               // draft
                t.SprintId == null &&        // chưa vào sprint nào
                !t.IsDeleted);

        // Optional: search theo Title/Code nếu PagedRequest có SearchTerm
      

        // Sort: mặc định mới nhất trước
        query = query.OrderByDescending(t => t.CreateAt);

        var pageNumber = request.PageNumber <= 0 ? 1 : request.PageNumber;
        var pageSize = request.PageSize <= 0 ? 20 : request.PageSize;

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        var responseItems = new List<ProjectTaskResponse>();

        foreach (var t in items)
        {
            var resp = _mapper.Map<ProjectTaskResponse>(t);
            resp.AssigneeIds = t.TaskWorkflows?
                .Where(a => a.AssignUserId.HasValue)
                .Select(a => a.AssignUserId!.Value)
                .ToList() ?? new List<Guid>();

            resp.WorkflowAssignments = await _taskWorkflowService.GetAssignmentsForTaskAsync(resp.Id, ct);

            responseItems.Add(resp);
        }

        return new PagedResult<ProjectTaskResponse>
        {
            Items = responseItems,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }

    public async Task<bool> DeleteDraftTaskAsync(
        Guid id,
        Guid userId,
        CancellationToken ct = default)
    {
        var e = await _repo.FindByIdAsync(id, ct)
            ?? throw CustomExceptionFactory.CreateNotFoundError(
                ResponseMessages.NOT_FOUND.FormatMessage("Draft task"));

        if (!e.IsBacklog || e.SprintId != null)
            throw CustomExceptionFactory.CreateBadRequestError("Task is not a draft task.");

        // Tận dụng logic DeleteTaskAsync để log + soft delete
        return await DeleteTaskAsync(id, userId, ct);
    }
    public async Task<ProjectTaskResponse> MaterializeDraftTaskAsync(
    Guid draftTaskId,
    Guid sprintId,
    Guid? workflowStatusId,
    string? statusCode,
    Guid userId,
    CancellationToken ct = default)
    {
        // 1) Lấy draft task
        var e = await _repo.FindByIdAsync(draftTaskId, ct)
            ?? throw CustomExceptionFactory.CreateNotFoundError(
                ResponseMessages.NOT_FOUND.FormatMessage("Draft task"));

        if (!e.IsBacklog || e.SprintId != null)
            throw CustomExceptionFactory.CreateBadRequestError("Task is not a draft task.");

        // 2) Lấy sprint & verify cùng project
        var sprint = await _db.Sprints.FirstOrDefaultAsync(s => s.Id == sprintId, ct)
            ?? throw CustomExceptionFactory.CreateNotFoundError(
                ResponseMessages.NOT_FOUND.FormatMessage("Sprint"));

        if (sprint.ProjectId != e.ProjectId)
            throw CustomExceptionFactory.CreateBadRequestError("Sprint must be in the same project as the draft task.");

        // 3) Xác định workflowId đang dùng
        var wfId = await GetWorkflowIdForMove(e.ProjectId, e.CurrentStatusId, ct);

        // 4) Resolve status target:
        // - ưu tiên WorkflowStatusId FE gửi
        // - tiếp theo StatusCode FE gửi
        // - fallback: status hiện tại của draft / default TODO trong workflow
        Guid? currentStatusForResolve = workflowStatusId ?? e.CurrentStatusId;
        var statusTextForResolve = !string.IsNullOrWhiteSpace(statusCode)
            ? statusCode
            : e.Status;

        var resolved = await ResolveStatusForWorkflow(
            currentStatusForResolve,
            statusTextForResolve,
            wfId,
            ct);

        // 5) Cập nhật draft thành task trong sprint
        e.SprintId = sprint.Id;
        e.IsBacklog = false;
        e.CurrentStatusId = resolved.Id;
        e.Status = !string.IsNullOrWhiteSpace(resolved.Code) ? resolved.Code : resolved.Name;
        e.OrderInSprint = await GetNextOrderInSprint(sprint.Id, resolved.Id, ct);
        // Lần đầu vào sprint => KHÔNG tăng CarryOverCount
        e.UpdateAt = DateTime.UtcNow;

        await _repo.UpdateAsync(e, ct);

        // 6) Log activity
        var companyId = await GetCompanyIdOfProject(e.ProjectId, ct);
        await _log.CreateLog(new CompanyActivityLog
        {
            CompanyId = companyId,
            ActorUserId = _current.GetUserId(),
            Title = "Materialize draft task",
            Description = $"User '{await GetUserName(_current.GetUserId())}' materialized draft task '{e.Title}' to sprint '{sprint.Name}'"
        });

        return _mapper.Map<ProjectTaskResponse>(e);
    }

    #endregion
    #region Ticket backlog tasks (Tasks created from Ticket)

    public async Task<ProjectTaskResponse> CreateTaskForTicketAsync(
        Guid ticketId,
        ProjectTaskRequest req,
        Guid userId,
        CancellationToken ct = default)
    {
        // 0) Load ticket + validate
        var ticket = await _db.Tickets
            .AsNoTracking()
            .FirstOrDefaultAsync(t =>
                t.Id == ticketId &&
                (!t.IsDeleted.HasValue || !t.IsDeleted.Value),
                ct);

        if (ticket == null)
            throw CustomExceptionFactory.CreateNotFoundError(
                ResponseMessages.NOT_FOUND.FormatMessage("Ticket"));

        if (!ticket.ProjectId.HasValue || ticket.ProjectId == Guid.Empty)
            throw CustomExceptionFactory.CreateBadRequestError(
                "Ticket is not associated with any project, cannot create tasks.");

        // Ép ProjectId của task = ProjectId của ticket
        req.ProjectId = ticket.ProjectId.Value;

        // Task được tạo từ Ticket luôn là backlog, chưa nằm trong sprint
        req.SprintId = null;

        // ---------- Giống logic CreateDraftTaskAsync ----------

        // 1) Validate project
        var project = await _db.Projects.AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == req.ProjectId, ct)
            ?? throw CustomExceptionFactory.CreateNotFoundError(
                ResponseMessages.NOT_FOUND.FormatMessage("Project"));

        // 2) Chọn status: ưu tiên WorkflowStatusId → StatusCode → default (TODO/first)
        WorkflowStatus? status = null;

        if (req.WorkflowStatusId != null)
        {
            status = await _db.WorkflowStatuses.AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == req.WorkflowStatusId, ct);
        }

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
                    .OrderBy(x => x.Position)
                    .FirstOrDefaultAsync(ct)
                ?? throw CustomExceptionFactory.CreateBadRequestError("No workflow status available.");
        }

        // 3) Sinh Code PRJ-T-###
        var seq = await _db.ProjectTasks.Include(t => t.Ticket).AsNoTracking()
            .LongCountAsync(t => t.ProjectId == req.ProjectId, ct) + 1;

        var prefix = string.IsNullOrWhiteSpace(project.Code) ? "PRJ" : project.Code!;
        var code = $"{prefix}-T-{seq:000}";

        // 4) Map + defaults: luôn backlog, có TicketId
        var now = DateTime.UtcNow;
        var entity = _mapper.Map<ProjectTask>(req);

        entity.Id = Guid.NewGuid();
        entity.ProjectId = req.ProjectId;
        entity.SprintId = null;          // từ ticket: luôn backlog
        entity.IsBacklog = true;
        entity.TicketId = ticket.Id;     // liên kết ticket
        entity.Code = code;

        entity.CurrentStatusId = status.Id;
        entity.Status = !string.IsNullOrWhiteSpace(status.Code) ? status.Code : status.Name;
        entity.OrderInSprint = null;     // chưa nằm trong sprint => không order

        entity.RemainingHours = req.EstimateHours;
        entity.CreateAt = now;
        entity.UpdateAt = now;
        entity.CreatedBy = userId;
        entity.IsDeleted = false;

        // Assignees
        entity.Assignees = new List<TaskWorkflow>();
        if (req.AssigneeIds?.Count > 0)
        {
            foreach (var assignUserId in req.AssigneeIds.Distinct())
            {
                entity.Assignees.Add(new TaskWorkflow
                {
                    TaskId = entity.Id,
                    AssignUserId = assignUserId
                });
            }
        }

        // 5) Persist
        await _repo.AddAsync(entity, ct);

        // 6) Workflow assignments (nếu FE gửi)
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
            .Select(p => (Guid)p.CompanyId)
            .FirstAsync(ct);

        // 7) Log
        var actorId = _current.GetUserId();
        await _log.CreateLog(new CompanyActivityLog
        {
            CompanyId = companyId,
            ActorUserId = actorId,
            Title = "Create ticket task",
            Description =
                $"User '{await GetUserName(actorId)}' created backlog task '{entity.Title}' from ticket '{ticket.TicketName ?? ticket.Id.ToString()}'"
        });

        return _mapper.Map<ProjectTaskResponse>(entity);
    }

    public async Task<PagedResult<ProjectTaskResponse>> GetTasksByTicketIdAsync(
      Guid ticketId,
      PagedRequest request,
      CancellationToken ct = default)
    {
        // 1) Kiểm tra ticket tồn tại
        var ticketExists = await _db.Tickets
            .AsNoTracking()
            .AnyAsync(t =>
                t.Id == ticketId &&
                (!t.IsDeleted.HasValue || !t.IsDeleted.Value),
                ct);

        if (!ticketExists)
        {
            throw CustomExceptionFactory.CreateNotFoundError(
                ResponseMessages.NOT_FOUND.FormatMessage("Ticket"));
        }

        // 2) Query tasks theo ticket
        var query = _db.ProjectTasks
            .AsNoTracking()
            .Include(t => t.TaskWorkflows).Include(t => t.Ticket)
            .Where(t => t.TicketId == ticketId && !t.IsDeleted);

        // 3) Chuẩn hoá SortColumn cho ToPagedResultAsync
        if (string.IsNullOrWhiteSpace(request.SortColumn))
        {
            request.SortColumn = nameof(ProjectTask.CreateAt);
            request.SortDescending = true;
        }
        else
        {
            // Tìm property thật trên ProjectTask theo tên (ignore case)
            var prop = typeof(ProjectTask).GetProperty(
                request.SortColumn,
                BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

            if (prop == null)
            {
                // Không tìm thấy cột sort → fallback an toàn
                request.SortColumn = nameof(ProjectTask.CreateAt);
                request.SortDescending = true;
            }
            else
            {
                // Chuẩn lại tên property đúng casing để Dynamic LINQ không lỗi
                request.SortColumn = prop.Name;
            }
        }

        // 4) Áp dụng phân trang + sort qua helper chung
        var paged = await query.ToPagedResultAsync(request, ct);

        // 5) Map sang response + load WorkflowAssignments
        var items = new List<ProjectTaskResponse>(paged.Items.Count);

        foreach (var t in paged.Items)
        {
            var resp = _mapper.Map<ProjectTaskResponse>(t);

            // AssigneeIds từ TaskWorkflows
            resp.AssigneeIds = t.TaskWorkflows?
                    .Where(a => a.AssignUserId.HasValue)
                    .Select(a => a.AssignUserId!.Value)
                    .ToList()
                ?? new List<Guid>();

            // Workflow assignments
            resp.WorkflowAssignments = await _taskWorkflowService
                .GetAssignmentsForTaskAsync(resp.Id, ct);

            items.Add(resp);
        }

        // 6) Trả PagedResult<ProjectTaskResponse>
        return new PagedResult<ProjectTaskResponse>
        {
            Items = items,
            TotalCount = paged.TotalCount,
            PageNumber = paged.PageNumber,
            PageSize = paged.PageSize
        };
    }

    #endregion

}
