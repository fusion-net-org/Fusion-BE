// Fusion.Service/Services/ProjectBoardService.cs
using Fusion.Repository.Bases.Page.ProjectBoard;
using Fusion.Repository.Entities;
using Fusion.Repository.Repositories;
using Fusion.Service.ViewModels.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Text.Json;

namespace Fusion.Service.Services
{
    public interface IProjectBoardService
    {
        Task<MultiSprintBoardResponseDto> GetSprintBoardAsync(
            Guid projectId, Guid? sprintId = null, bool includeClosed = false,
            DateOnly? from = null, DateOnly? to = null, CancellationToken ct = default);

        // NEW: đổi cột (và/hoặc đổi sprint), optional update vị trí trong cột
        Task<TaskVmDto> MoveTaskAsync(
            Guid projectId, Guid taskId, Guid toStatusId,
            Guid? toSprintId, int? newOrder, Guid actorUserId, CancellationToken ct = default);

        // NEW: reorder thứ tự trong 1 cột của 1 sprint
        Task ReorderColumnAsync(
            Guid projectId, Guid sprintId, Guid statusId,
            IEnumerable<(Guid taskId, int order)> orders, CancellationToken ct = default);
        Task<PagedResult<TaskVmDto>> GetTaskListAsync(
          Guid projectId,
          ProjectTaskListQuery query,
          CancellationToken ct = default);


    }

    public class ProjectBoardService : IProjectBoardService
    {
        private readonly IProjectBoardRepository _repo;
        public ProjectBoardService(IProjectBoardRepository repo) => _repo = repo;

        public async Task<MultiSprintBoardResponseDto> GetSprintBoardAsync(
    Guid projectId, Guid? sprintId = null, bool includeClosed = false,
    DateOnly? from = null, DateOnly? to = null, CancellationToken ct = default)
        {
            var project = await _repo.GetProjectWithWorkflowAsync(projectId, ct)
                         ?? throw new KeyNotFoundException("Project not found");
            if (project.WorkflowId == null)
                throw new InvalidOperationException("Project has no workflow assigned.");

            var workflowId = project.WorkflowId.Value;

            // 1) Load sprint
            var sprints = await _repo.GetSprintsAsync(projectId, sprintId, includeClosed, from, to, ct);
            if (sprints.Count == 0) return new MultiSprintBoardResponseDto();

            // 2) Load statuses + transitions
            var statuses = await _repo.GetStatusesAsync(workflowId, ct);
            if (statuses.Count == 0)
                throw new InvalidOperationException("Workflow has no statuses.");

            var transitions = await _repo.GetTransitionsAsync(workflowId, ct);

            var statusOrder = statuses
                .OrderBy(x => x.Position)
                .Select(x => x.Id)
                .ToList();

            var statusMeta = statuses
                .OrderBy(x => x.Position)
                .ToDictionary(
                    x => x.Id,
                    x => new StatusMetaDto
                    {
                        Id = x.Id,
                        Code = !string.IsNullOrWhiteSpace(x.Code) ? x.Code! : Slug(x.Name),
                        Name = x.Name ?? "",
                        Category = NormalizeCategory(x.Category, x.IsStart, x.IsEnd),
                        Order = x.Position,
                        WipLimit = null,
                        Color = x.Color,
                        IsFinal = x.IsEnd,
                        IsStart = x.IsStart,
                        Roles = ParseRoles(x.RolesJson)
                    });
            var components = await _repo.GetComponentsAsync(projectId, ct);
            var componentDtos = components.Select(c => new ComponentVmDto
            {
                Id = c.Id,
                Name = c.Name,
                Description = c.Description,
                CreatedAt = c.CreatedAt,
                CreatedBy = c.CreatedBy
            }).ToList();
            var workflowDto = new WorkflowBoardDto
            {
                Id = workflowId,
                Name = project.Workflow?.Name ?? project.Name ?? "Workflow",
                StatusOrder = statusOrder,
                StatusMeta = statusMeta,
                Transitions = transitions
                    .Select(tr => new WorkflowTransitionDto
                    {
                        Id = tr.Id,
                        WorkflowId = tr.WorkflowId ?? workflowId,
                        FromStatusId = tr.FromStatusId ?? Guid.Empty,
                        ToStatusId = tr.ToStatusId ?? Guid.Empty,
                        Type = tr.Type,
                        Label = tr.Label,
                        Rule = tr.Rule,
                        Roles = ParseRoles(tr.RoleNamesJson),
                        EnforceTransitions = tr.EnforceTransitions
                    })
                    .ToList()
            };

            // 3) Sprint DTO không còn statusMeta/statusOrder
            var sprintDtos = sprints.Select(s => new SprintVmDto
            {
                Id = s.Id,
                Name = s.Name ?? "Sprint",
                Start = s.StartDate,
                End = s.EndDate,
                State = MapSprintState(s.Status),
                CapacityHours = s.CapacityHours,
                CommittedPoints = s.CommittedPoints,
                WorkflowId = workflowId
            }).ToList();

            var sprintIds = sprintDtos.Select(x => x.Id).ToList();

            // 4) build tasks như cũ (sử dụng statusMeta ở trên)
            var tasks = await _repo.GetTasksForSprintsAsync(projectId, sprintIds, ct);
            var assRows = await _repo.GetAssigneesForTasksAsync(tasks.Select(t => t.Id), ct);
            var assigneesMap = assRows.GroupBy(a => a.TaskId)
                .ToDictionary(g => g.Key, g => g.ToList());

            var startStatusId = statuses.FirstOrDefault(x => x.IsStart)?.Id ?? statusOrder.First();

            var taskDtos = new List<TaskVmDto>();
            foreach (var t in tasks)
            {
                var curStatusId = t.CurrentStatusId ?? startStatusId;
                if (!statusMeta.TryGetValue(curStatusId, out var meta))
                {
                    curStatusId = startStatusId;
                    meta = statusMeta[curStatusId];
                }

                var assignees = assigneesMap.TryGetValue(t.Id, out var rows)
                    ? rows.Select(a => new MemberRefDto(
                            a.AssignUserId.ToString(),
                            a.AssignUser?.UserName ?? a.AssignUser?.Email ?? "Member",
                            a.AssignUser?.Avatar
                        )).ToList()
                    : new List<MemberRefDto>();
                var effectiveComponent = t.Component;
                taskDtos.Add(new TaskVmDto
                {
                    Id = t.Id,
                    Code = t.Code ?? "",
                    Title = t.Title ?? "",
                    Type = t.Type ?? "Task",
                    Priority = t.Priority ?? "Medium",
                    Severity = t.Severity,
                    StoryPoints = t.Point,
                    IsClose = t.IsClose,
                    EstimateHours = t.EstimateHours,
                    RemainingHours = t.RemainingHours,
                    DueDate = t.DueDate.HasValue
                        ? new DateTimeOffset(DateTime.SpecifyKind(t.DueDate.Value, DateTimeKind.Utc))
                        : null,

                    SprintId = t.SprintId,
                    WorkflowStatusId = curStatusId,
                    StatusCode = meta.Code,
                    StatusCategory = meta.Category,

                    Assignees = assignees,
                    DependsOn = new List<Guid>(),
                    ParentTaskId = t.ParentTaskId,
                    CarryOverCount = t.CarryOverCount,
                    StatusName = meta.Name,

                    OpenedAt = t.CreateAt ?? DateTime.UtcNow,
                    UpdatedAt = t.UpdateAt ?? t.CreateAt ?? DateTime.UtcNow,
                    CreatedAt = t.CreateAt ?? DateTime.UtcNow,
                    ComponentId = effectiveComponent?.Id,
                    ComponentName = effectiveComponent?.Name,
                    TicketId = t.TicketId,
                    TicketName = t.Ticket?.TicketName,
                    SourceTicketId = t.SourceTaskId,
                    SourceTicketCode = null,
                });
            }

            // 5) response mới
            return new MultiSprintBoardResponseDto
            {
                Workflow = workflowDto,
                Sprints = sprintDtos,
                Tasks = taskDtos,
                Components = componentDtos
            };
        }

        public async Task<PagedResult<TaskVmDto>> GetTaskListAsync(
     Guid projectId,
     ProjectTaskListQuery query,
     CancellationToken ct = default)
        {
            query ??= new ProjectTaskListQuery();

            var project = await _repo.GetProjectWithWorkflowAsync(projectId, ct)
                         ?? throw new KeyNotFoundException("Project not found");

            if (project.WorkflowId == null)
                throw new InvalidOperationException("Project has no workflow assigned.");

            // Build workflow metadata
            var statuses = await _repo.GetStatusesAsync(project.WorkflowId.Value, ct);
            if (statuses.Count == 0)
                throw new InvalidOperationException("Workflow has no statuses.");

            var statusOrder = statuses
                .OrderBy(x => x.Position)
                .Select(x => x.Id)
                .ToList();

            var statusMeta = statuses
                .OrderBy(x => x.Position)
                .ToDictionary(
                    x => x.Id,
                    x => new StatusMetaDto
                    {
                        Id = x.Id,
                        Code = !string.IsNullOrWhiteSpace(x.Code) ? x.Code! : Slug(x.Name),
                        Name = x.Name ?? string.Empty,
                        Category = NormalizeCategory(x.Category, x.IsStart, x.IsEnd),
                        Order = x.Position,
                        WipLimit = null,
                        Color = x.Color,
                        IsFinal = x.IsEnd,
                        IsStart = x.IsStart,
                        Roles = ParseRoles(x.RolesJson)
                    });

            var startStatusId = statuses.FirstOrDefault(x => x.IsStart)?.Id ?? statusOrder.First();

            // Load sprints for this query
            var includeClosed = !query.OnlyActiveSprints;
            var sprints = await _repo.GetSprintsAsync(
                projectId,
                query.SprintId,
                includeClosed,
                from: null,
                to: null,
                ct);

            // Normalize page inputs
            var pageNumber = query.PageNumber <= 0 ? 1 : query.PageNumber;
            var pageSize = query.PageSize <= 0 ? 25 : query.PageSize;

            if (sprints.Count == 0)
            {
                // Use constructor because PagedResult<T> is an immutable record
                return new PagedResult<TaskVmDto>(
                    Array.Empty<TaskVmDto>(), // items
                    0,                        // totalCount
                    pageNumber,
                    pageSize
                );
            }

            var sprintIds = sprints.Select(s => s.Id).ToList();

            // Load all tasks for selected sprints
            var tasks = await _repo.GetTasksForSprintsAsync(projectId, sprintIds, ct);

            // ---------------- FILTERS ON ENTITY ----------------
            IEnumerable<ProjectTask> filtered = tasks;

            // Search on code + title
            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                var keyword = query.Search.Trim();
                filtered = filtered.Where(t =>
                    (!string.IsNullOrEmpty(t.Code) &&
                     t.Code.Contains(keyword, StringComparison.OrdinalIgnoreCase)) ||
                    (!string.IsNullOrEmpty(t.Title) &&
                     t.Title.Contains(keyword, StringComparison.OrdinalIgnoreCase)));
            }

            // Status category (TODO / IN_PROGRESS / REVIEW / DONE)
            if (!string.IsNullOrWhiteSpace(query.StatusCategory) &&
                !string.Equals(query.StatusCategory, "ALL", StringComparison.OrdinalIgnoreCase))
            {
                var cat = query.StatusCategory.Trim().ToUpperInvariant();
                var statusIds = statusMeta
                    .Where(kv => string.Equals(kv.Value.Category, cat,
                        StringComparison.OrdinalIgnoreCase))
                    .Select(kv => kv.Key)
                    .ToHashSet();

                filtered = filtered.Where(t =>
                {
                    var cur = t.CurrentStatusId ?? startStatusId;
                    return statusIds.Contains(cur);
                });
            }

            // Priority
            if (!string.IsNullOrWhiteSpace(query.Priority) &&
                !string.Equals(query.Priority, "ALL", StringComparison.OrdinalIgnoreCase))
            {
                var pr = query.Priority.Trim();
                filtered = filtered.Where(t =>
                    string.Equals(t.Priority ?? string.Empty, pr,
                        StringComparison.OrdinalIgnoreCase));
            }

            // Severity
            if (!string.IsNullOrWhiteSpace(query.Severity) &&
                !string.Equals(query.Severity, "ALL", StringComparison.OrdinalIgnoreCase))
            {
                var sev = query.Severity.Trim();
                filtered = filtered.Where(t =>
                    string.Equals(t.Severity ?? string.Empty, sev,
                        StringComparison.OrdinalIgnoreCase));
            }

            // Due date range (inclusive)
            if (query.DueFrom.HasValue)
            {
                var fromDate = query.DueFrom.Value.ToDateTime(TimeOnly.MinValue).Date;
                filtered = filtered.Where(t =>
                    t.DueDate.HasValue &&
                    t.DueDate.Value.Date >= fromDate);
            }

            if (query.DueTo.HasValue)
            {
                var toDate = query.DueTo.Value.ToDateTime(TimeOnly.MaxValue).Date;
                filtered = filtered.Where(t =>
                    t.DueDate.HasValue &&
                    t.DueDate.Value.Date <= toDate);
            }

            // Materialize list before assignee filter
            var filteredList = filtered.ToList();

            // Assignee filter (task must have at least one of selected assignees)
            if (query.AssigneeIds != null && query.AssigneeIds.Count > 0)
            {
                var assigneeIds = query.AssigneeIds
                    .Where(x => x != Guid.Empty)
                    .ToHashSet();

                if (assigneeIds.Count > 0)
                {
                    var assRows = await _repo.GetAssigneesForTasksAsync(
                        filteredList.Select(t => t.Id),
                        ct);

                    var taskIdsWithAssignee = assRows
     .Where(a => a.AssignUserId.HasValue && assigneeIds.Contains(a.AssignUserId.Value))
     .Select(a => a.TaskId)
     .ToHashSet();


                    filteredList = filteredList
                        .Where(t => taskIdsWithAssignee.Contains(t.Id))
                        .ToList();
                }
            }

            // Load assignee rows for DTO mapping
            var assRowsAll = await _repo.GetAssigneesForTasksAsync(
                filteredList.Select(t => t.Id),
                ct);

            var assigneesMap = assRowsAll
                .GroupBy(a => a.TaskId)
                .ToDictionary(g => g.Key, g => g.ToList());

            // ---------------- MAP TO DTO ----------------
            var dtoList = new List<TaskVmDto>(filteredList.Count);

            foreach (var t in filteredList)
            {
                var curStatusId = t.CurrentStatusId ?? startStatusId;
                if (!statusMeta.TryGetValue(curStatusId, out var meta))
                {
                    curStatusId = startStatusId;
                    meta = statusMeta[curStatusId];
                }

                var assignees = assigneesMap.TryGetValue(t.Id, out var rows)
                    ? rows.Select(a => new MemberRefDto(
                            a.AssignUserId.ToString(),
                            a.AssignUser?.UserName ?? a.AssignUser?.Email ?? "Member",
                            a.AssignUser?.Avatar
                        )).ToList()
                    : new List<MemberRefDto>();

                dtoList.Add(new TaskVmDto
                {
                    Id = t.Id,
                    Code = t.Code ?? string.Empty,
                    Title = t.Title ?? string.Empty,
                    Type = t.Type ?? "Task",
                    Priority = t.Priority ?? "Medium",
                    Severity = t.Severity,
                    IsClose = t.IsClose,
                    StoryPoints = t.Point,
                    EstimateHours = t.EstimateHours,
                    RemainingHours = t.RemainingHours,
                    DueDate = t.DueDate.HasValue
                        ? new DateTimeOffset(DateTime.SpecifyKind(t.DueDate.Value, DateTimeKind.Utc))
                        : null,

                    SprintId = t.SprintId,
                    WorkflowStatusId = curStatusId,
                    StatusCode = meta.Code,
                    StatusCategory = meta.Category,
                    ComponentId = t.Component?.Id,
                    ComponentName = t.Component?.Name,
                    Assignees = assignees,
                    DependsOn = new List<Guid>(),
                    ParentTaskId = t.ParentTaskId,
                    CarryOverCount = t.CarryOverCount,
                    StatusName = meta.Name,
                    OpenedAt = t.CreateAt ?? DateTime.UtcNow,
                    UpdatedAt = t.UpdateAt ?? t.CreateAt ?? DateTime.UtcNow,
                    CreatedAt = t.CreateAt ?? DateTime.UtcNow,

                    TicketId = t.TicketId,
                    TicketName = t.Ticket?.TicketName,
                    SourceTicketId = t.SourceTaskId,
                    SourceTicketCode = null
                });
            }

            // Sorting
            dtoList = SortTaskList(dtoList, query.SortBy);

            // Paging
            var totalCount = dtoList.Count;

            var items = dtoList
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            // Again: use the record constructor
            return new PagedResult<TaskVmDto>(
                items,
                totalCount,
                pageNumber,
                pageSize
            );
        }

        private static List<TaskVmDto> SortTaskList(
            IEnumerable<TaskVmDto> source,
            string? sortBy)
        {
            var key = (sortBy ?? "updatedDesc").Trim();

            IOrderedEnumerable<TaskVmDto> ordered;

            switch (key)
            {
                case "dueAsc":
                    ordered = source.OrderBy(t =>
                        t.DueDate ?? DateTimeOffset.MaxValue);
                    break;

                case "dueDesc":
                    ordered = source.OrderByDescending(t =>
                        t.DueDate ?? DateTimeOffset.MinValue);
                    break;

                case "priority":
                    var priorityOrder = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
                    {
                        ["Urgent"] = 1,
                        ["High"] = 2,
                        ["Medium"] = 3,
                        ["Low"] = 4
                    };
                    ordered = source.OrderBy(t =>
                    {
                        if (t.Priority == null) return int.MaxValue;
                        return priorityOrder.TryGetValue(t.Priority, out var v)
                            ? v
                            : int.MaxValue;
                    });
                    break;

                case "updatedDesc":
                default:
                    ordered = source.OrderByDescending(t => t.UpdatedAt);
                    break;
            }

            return ordered.ToList();
        }
        // ----------------- MOVE (đổi cột / đổi sprint) -----------------
        public async Task<TaskVmDto> MoveTaskAsync(
            Guid projectId, Guid taskId, Guid toStatusId,
            Guid? toSprintId, int? newOrder, Guid actorUserId, CancellationToken ct = default)
        {
            var project = await _repo.GetProjectWithWorkflowAsync(projectId, ct)
                         ?? throw new KeyNotFoundException("Project not found");
            if (project.WorkflowId == null)
                throw new InvalidOperationException("Project has no workflow assigned.");

            var statuses = await _repo.GetStatusesAsync(project.WorkflowId.Value, ct);
            if (!statuses.Any(x => x.Id == toStatusId))
                throw new InvalidOperationException("Invalid status.");

            var task = await _repo.GetTaskForMoveAsync(taskId, ct)
                       ?? throw new KeyNotFoundException("Task not found");
            if (task.ProjectId != projectId)
                throw new InvalidOperationException("Task does not belong to this project.");

            // Validate transition nếu có trạng thái hiện tại
            if (task.CurrentStatusId.HasValue && task.CurrentStatusId.Value != toStatusId)
            {
                var ok = await _repo.HasTransitionAsync(project.WorkflowId.Value, task.CurrentStatusId.Value, toStatusId, ct);
                if (!ok) throw new InvalidOperationException("Transition is not allowed by workflow.");
            }

            // Apply thay đổi
            task.CurrentStatusId = toStatusId;
            if (toSprintId.HasValue) task.SprintId = toSprintId.Value;
            if (newOrder.HasValue) task.OrderInSprint = newOrder.Value;
            task.UpdateAt = DateTime.UtcNow;

            // Log workflow
            await _repo.AddTaskWorkflowAsync(new TaskWorkflow
            {
                Id = Guid.NewGuid(),
                TaskId = task.Id,
                WorkflowStatusId = toStatusId,
                AssignUserId = actorUserId,
                CreatedAt = DateTime.UtcNow
            }, ct);

            await _repo.SaveAsync(ct);

            // Trả DTO tối thiểu cho realtime: build meta từ statuses
            var statusOrder = statuses.OrderBy(x => x.Position).Select(x => x.Id).ToList();
            var statusMeta = statuses.OrderBy(x => x.Position).ToDictionary(
                x => x.Id,
                x => new StatusMetaDto
                {
                    Id = x.Id,
                    Code = !string.IsNullOrWhiteSpace(x.Code) ? x.Code! : Slug(x.Name),
                    Name = x.Name ?? "",
                    Category = NormalizeCategory(x.Category, x.IsStart, x.IsEnd),
                    Order = x.Position,
                    WipLimit = null,
                    Color = x.Color,
                    IsFinal = x.IsEnd,
                    Roles = ParseRoles(x.RolesJson)
                });
            var startStatusId = statuses.FirstOrDefault(x => x.IsStart)?.Id ?? statusOrder.First();
            var curStatusId = task.CurrentStatusId ?? startStatusId;
            if (!statusMeta.ContainsKey(curStatusId)) curStatusId = startStatusId;
            var meta = statusMeta[curStatusId];

            var assRows = await _repo.GetAssigneesForTasksAsync(new[] { task.Id }, ct);
            var assignees = assRows.Select(a => new MemberRefDto(
                a.AssignUserId.ToString(),
                a.AssignUser?.UserName ?? a.AssignUser?.Email ?? "Member",
                a.AssignUser?.Avatar
            )).ToList();

            return new TaskVmDto
            {
                Id = task.Id,
                Code = task.Code ?? "",
                Title = task.Title ?? "",
                Type = task.Type ?? "Task",
                Priority = task.Priority ?? "Medium",
                Severity = task.Severity,

                StoryPoints = task.Point,
                EstimateHours = task.EstimateHours,
                RemainingHours = task.RemainingHours,
                DueDate = task.DueDate.HasValue ? new DateTimeOffset(DateTime.SpecifyKind(task.DueDate.Value, DateTimeKind.Utc)) : null,
                IsClose = task.IsClose,
                SprintId = task.SprintId,
                WorkflowStatusId = curStatusId,
                StatusCode = meta.Code,
                StatusCategory = meta.Category,

                Assignees = assignees,
                DependsOn = new List<Guid>(),
                ParentTaskId = task.ParentTaskId,
                CarryOverCount = task.CarryOverCount,

                OpenedAt = task.CreateAt ?? DateTime.UtcNow,
                UpdatedAt = task.UpdateAt ?? task.CreateAt ?? DateTime.UtcNow,
                CreatedAt = task.CreateAt ?? DateTime.UtcNow,

                ComponentId = task.Component?.Id,
                ComponentName = task.Component?.Name,

                TicketId = task.TicketId,
                TicketName = task.Ticket?.TicketName,
                SourceTicketId = task.SourceTaskId,
                SourceTicketCode = null
            };
        }

        // ----------------- REORDER trong 1 cột -----------------
        public async Task ReorderColumnAsync(
            Guid projectId, Guid sprintId, Guid statusId,
            IEnumerable<(Guid taskId, int order)> orders, CancellationToken ct = default)
        {
            var dict = orders.ToDictionary(x => x.taskId, x => x.order);

            var tasks = await _repo.GetTasksForSprintsAsync(projectId, new[] { sprintId }, ct);
            var now = DateTime.UtcNow;

            foreach (var t in tasks)
            {
                if (t.SprintId == sprintId && t.CurrentStatusId == statusId && dict.TryGetValue(t.Id, out var newOrder))
                {
                    // lấy tracked entity để update nhanh
                    var tracked = await _repo.GetTaskForMoveAsync(t.Id, ct);
                    if (tracked != null)
                    {
                        tracked.OrderInSprint = newOrder;
                        tracked.UpdateAt = now;
                    }
                }
            }
            await _repo.SaveAsync(ct);
        }

        // --------- helpers ---------
        private static IReadOnlyList<string> ParseRoles(string? rolesJson)
        {
            if (string.IsNullOrWhiteSpace(rolesJson))
                return Array.Empty<string>();

            try
            {
                // Dữ liệu chuẩn: ["Developer","QA"]
                var list = JsonSerializer.Deserialize<List<string>>(rolesJson);
                if (list == null) return Array.Empty<string>();

                return list
                    .Where(r => !string.IsNullOrWhiteSpace(r))
                    .Select(r => r.Trim())
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToArray();
            }
            catch
            {
                // Fallback: dữ liệu cũ kiểu "Developer, QA"
                return rolesJson
                    .Split(',', ';')
                    .Select(r => r.Trim())
                    .Where(r => !string.IsNullOrWhiteSpace(r))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToArray();
            }
        }
        private static string Slug(string? name)
        {
            if (string.IsNullOrWhiteSpace(name)) return "unknown";
            var s = new string(name.ToLower().Trim().Select(c => char.IsLetterOrDigit(c) ? c : '-').ToArray());
            while (s.Contains("--")) s = s.Replace("--", "-");
            return s.Trim('-');
        }

        private static string NormalizeCategory(string? category, bool isStart, bool isEnd)
        {
            if (!string.IsNullOrWhiteSpace(category))
            {
                var c = category.Trim().ToUpperInvariant();
                if (c is "TODO" or "IN_PROGRESS" or "REVIEW" or "DONE") return c;
            }
            if (isEnd) return "DONE";
            if (isStart) return "TODO";
            return "IN_PROGRESS";
        }

        private static string MapSprintState(Fusion.Repository.Enums.SprintStatus s) => s switch
        {
            Fusion.Repository.Enums.SprintStatus.Planning => "Planning",
            Fusion.Repository.Enums.SprintStatus.Active => "Active",
            Fusion.Repository.Enums.SprintStatus.Closed => "Closed",
            _ => "Planning"
        };
    }
}
