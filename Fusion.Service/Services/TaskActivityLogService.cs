// Fusion.Service/Services/TaskActivityLogService.cs
using Fusion.Repository.Bases.Page.TaskLogEvent;
using Fusion.Repository.Data;
using Fusion.Repository.Entities;
using Fusion.Service.Commons.Helpers;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Fusion.Service.Services;

public interface ITaskActivityLogService
{
    Task TryWriteAsync(
        Guid taskId,
        string action,
        string? message = null,
        IEnumerable<string>? changedCols = null,
        object? metadata = null,
        bool isView = false,
        CancellationToken ct = default);

    Task TryWriteStatusChangedAsync(
        ProjectTask task,
        Guid? oldStatusId,
        string? oldStatusText,
        WorkflowStatus newStatus,
        CancellationToken ct = default);

    Task TryWriteReorderedAsync(
        ProjectTask task,
        Guid toSprintId,
        WorkflowStatus toStatus,
        Guid? oldSprintId,
        Guid? oldStatusId,
        int? oldOrder,
        int? newOrder,
        bool changedSprint,
        bool changedStatus,
        CancellationToken ct = default);

    Task TryWriteMovedToSprintAsync(
        ProjectTask task,
        Guid? oldSprintId,
        Guid newSprintId,
        Guid? oldStatusId,
        Guid newStatusId,
        int? oldOrder,
        int? newOrder,
        int carryOverCount,
        CancellationToken ct = default);
}

public sealed class TaskActivityLogService : ITaskActivityLogService
{
    private readonly FusionDbContext _db;
    private readonly ITaskLogEventService _taskLog;
    private readonly ICurrentService _current;

    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    private sealed record RefVm(Guid Id, string Name, string? Code);
    private sealed record TaskRefVm(Guid Id, string Code, string Title);

    public TaskActivityLogService(
        FusionDbContext db,
        ITaskLogEventService taskLog,
        ICurrentService current)
    {
        _db = db;
        _taskLog = taskLog;
        _current = current;
    }

    public async Task TryWriteAsync(
        Guid taskId,
        string action,
        string? message = null,
        IEnumerable<string>? changedCols = null,
        object? metadata = null,
        bool isView = false,
        CancellationToken ct = default)
    {
        var actorId = _current.GetUserId();
        if (actorId == Guid.Empty) return;

        var metaDict = BuildMeta(message, metadata);

        var log = new TaskLogEvent
        {
            TaskId = taskId,
            ActorId = actorId,
            Action = action,
            ChangedCols = changedCols == null
                ? null
                : string.Join(", ", changedCols.Where(x => !string.IsNullOrWhiteSpace(x))),
            Metadata = metaDict.Count == 0 ? null : JsonSerializer.Serialize(metaDict, _jsonOptions),
            IsView = isView
        };

        try { await _taskLog.CreateAsync(log, ct); }
        catch { /* swallow */ }
    }

    public async Task TryWriteStatusChangedAsync(
        ProjectTask task,
        Guid? oldStatusId,
        string? oldStatusText,
        WorkflowStatus newStatus,
        CancellationToken ct = default)
    {
        var taskRef = new TaskRefVm(task.Id, task.Code ?? "", task.Title ?? "");
        var sprintRef = await GetSprintRefAsync(task.SprintId, ct);
        var oldStatusRef = await GetStatusRefAsync(oldStatusId, ct);
        var newStatusRef = new RefVm(newStatus.Id, newStatus.Name ?? "", newStatus.Code);

        var msg =
            $"Changed status of {taskRef.Code} \"{taskRef.Title}\" " +
            $"from \"{oldStatusRef?.Name ?? oldStatusText ?? "Unknown"}\" to \"{newStatusRef.Name}\"" +
            (sprintRef != null ? $" in sprint \"{sprintRef.Name}\"." : ".");

        await TryWriteAsync(
            task.Id,
            action: "TASK_STATUS_CHANGED",
            message: msg,
            changedCols: new[] { "CurrentStatusId", "Status", "OrderInSprint" },
            metadata: new
            {
                task = taskRef,
                sprint = sprintRef,
                oldStatus = oldStatusRef,
                newStatus = newStatusRef,
                oldStatusId,
                newStatusId = newStatus.Id
            },
            ct: ct);
    }

    public async Task TryWriteReorderedAsync(
        ProjectTask task,
        Guid toSprintId,
        WorkflowStatus toStatus,
        Guid? oldSprintId,
        Guid? oldStatusId,
        int? oldOrder,
        int? newOrder,
        bool changedSprint,
        bool changedStatus,
        CancellationToken ct = default)
    {
        var taskRef = new TaskRefVm(task.Id, task.Code ?? "", task.Title ?? "");

        var fromSprint = await GetSprintRefAsync(oldSprintId, ct);
        var toSprint = await GetSprintRefAsync(toSprintId, ct) ?? new RefVm(toSprintId, "Unknown", null);

        var fromStatus = await GetStatusRefAsync(oldStatusId, ct);
        var toStatusRef = new RefVm(toStatus.Id, toStatus.Name ?? "", toStatus.Code);

        var msg = BuildReorderMessage(
            taskRef,
            fromSprint, toSprint,
            fromStatus, toStatusRef,
            oldOrder, newOrder,
            changedStatus, changedSprint);

        await TryWriteAsync(
            task.Id,
            action: "TASK_REORDERED",
            message: msg,
            changedCols: new[] { "SprintId", "CurrentStatusId", "Status", "OrderInSprint" },
            metadata: new
            {
                task = taskRef,
                fromSprint,
                toSprint,
                fromStatus,
                toStatus = toStatusRef,
                oldOrder,
                newOrder,
                changedStatus,
                changedSprint
            },
            ct: ct);
    }

    public async Task TryWriteMovedToSprintAsync(
     ProjectTask task,
     Guid? oldSprintId,
     Guid newSprintId,
     Guid? oldStatusId,
     Guid newStatusId,
     int? oldOrder,
     int? newOrder,
     int carryOverCount,
     CancellationToken ct = default)
    {
        var taskRef = new TaskRefVm(task.Id, task.Code ?? "", task.Title ?? "");

        var fromSprint = await GetSprintRefAsync(oldSprintId, ct);
        var toSprint = await GetSprintRefAsync(newSprintId, ct) ?? new RefVm(newSprintId, "Unknown", null);

        var fromStatus = await GetStatusRefAsync(oldStatusId, ct);
        var toStatus = await GetStatusRefAsync(newStatusId, ct) ?? new RefVm(newStatusId, "Unknown", null);

        var msg =
            $"Moved {taskRef.Code} \"{taskRef.Title}\" " +
            $"from sprint \"{fromSprint?.Name ?? "Backlog"}\" ({fromStatus?.Name ?? "Unknown"}) " ;

        await TryWriteAsync(
            task.Id,
            action: "TASK_MOVED_TO_SPRINT",
            message: msg,
            changedCols: new[] { "SprintId", "IsBacklog", "CurrentStatusId", "Status", "OrderInSprint", "CarryOverCount" },
            metadata: new
            {
                task = taskRef,
                fromSprint,
                toSprint,
                fromStatus,
                toStatus,
                oldOrder,
                newOrder,
                carryOverCount
            },
            ct: ct);
    }


    // -------------------- helpers --------------------

    private async Task<TaskRefVm?> GetTaskRefAsync(Guid taskId, CancellationToken ct)
        => await _db.ProjectTasks.AsNoTracking()
            .Where(t => t.Id == taskId)
            .Select(t => new TaskRefVm(t.Id, t.Code ?? "", t.Title ?? ""))
            .FirstOrDefaultAsync(ct);

    private async Task<RefVm?> GetSprintRefAsync(Guid? sprintId, CancellationToken ct)
    {
        if (!sprintId.HasValue || sprintId.Value == Guid.Empty) return null;

        return await _db.Sprints.AsNoTracking()
            .Where(s => s.Id == sprintId.Value)
            .Select(s => new RefVm(s.Id, s.Name ?? "", null))
            .FirstOrDefaultAsync(ct);
    }

    private async Task<RefVm?> GetStatusRefAsync(Guid? statusId, CancellationToken ct)
    {
        if (!statusId.HasValue || statusId.Value == Guid.Empty) return null;

        return await _db.WorkflowStatuses.AsNoTracking()
            .Where(s => s.Id == statusId.Value)
            .Select(s => new RefVm(s.Id, s.Name ?? "", s.Code))
            .FirstOrDefaultAsync(ct);
    }

    private static IDictionary<string, object?> BuildMeta(string? message, object? extra)
    {
        var dict = new Dictionary<string, object?>();

        if (!string.IsNullOrWhiteSpace(message))
            dict["message"] = message;

        if (extra != null)
        {
            var props = extra.GetType().GetProperties();
            foreach (var p in props)
            {
                var val = p.GetValue(extra);
                var key = char.ToLowerInvariant(p.Name[0]) + p.Name[1..];
                dict[key] = val;
            }
        }

        return dict;
    }

    private static string BuildReorderMessage(
        TaskRefVm taskRef,
        RefVm? fromSprint, RefVm toSprint,
        RefVm? fromStatus, RefVm toStatus,
        int? oldOrder, int? newOrder,
        bool changedStatus,
        bool changedSprint)
    {
        var fromSprintName = fromSprint?.Name ?? "Backlog";
        var fromStatusName = fromStatus?.Name ?? "Unknown";
        var toStatusName = toStatus.Name;

        if (changedSprint && changedStatus)
            return $"Moved {taskRef.Code} \"{taskRef.Title}\" from sprint \"{fromSprintName}\" ({fromStatusName}) to sprint \"{toSprint.Name}\" ({toStatusName})";

        if (changedSprint && !changedStatus)
            return $"Moved {taskRef.Code} \"{taskRef.Title}\" from sprint \"{fromSprintName}\" to sprint \"{toSprint.Name}\" in column \"{toStatusName}\"";

        if (!changedSprint && changedStatus)
            return $"Moved {taskRef.Code} \"{taskRef.Title}\" from \"{fromStatusName}\" to \"{toStatusName}\" in sprint \"{toSprint.Name}\"";

        return $"Reordered {taskRef.Code} \"{taskRef.Title}\" in sprint \"{toSprint.Name}\" / \"{toStatusName}\" .";
    }
}
