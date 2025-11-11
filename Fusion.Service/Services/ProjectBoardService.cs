// Fusion.Service/Services/ProjectBoardService.cs
using Fusion.Repository.Bases.Page.ProjectBoard;
using Fusion.Repository.Entities;
using Fusion.Repository.Repositories;
using Microsoft.EntityFrameworkCore;

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

            var sprints = await _repo.GetSprintsAsync(projectId, sprintId, includeClosed, from, to, ct);
            if (sprints.Count == 0) return new MultiSprintBoardResponseDto();

            var statuses = await _repo.GetStatusesAsync(project.WorkflowId!.Value, ct);
            if (statuses.Count == 0) throw new InvalidOperationException("Workflow has no statuses.");

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
                    IsFinal = x.IsEnd
                });

            var sprintDtos = sprints.Select(s => new SprintVmDto
            {
                Id = s.Id,
                Name = s.Name ?? "Sprint",
                Start = s.StartDate,
                End = s.EndDate,
                State = MapSprintState(s.Status),
                CapacityHours = s.CapacityHours,
                CommittedPoints = s.CommittedPoints,
                WorkflowId = project.WorkflowId,
                StatusOrder = statusOrder,
                StatusMeta = statusMeta
            }).ToList();

            var sprintIds = sprintDtos.Select(x => x.Id).ToList();

            var tasks = await _repo.GetTasksForSprintsAsync(projectId, sprintIds, ct);
            var assRows = await _repo.GetAssigneesForTasksAsync(tasks.Select(t => t.Id), ct);
            var assigneesMap = assRows.GroupBy(a => a.TaskId)
                .ToDictionary(g => g.Key, g => g.ToList());

            var startStatusId = statuses.FirstOrDefault(x => x.IsStart)?.Id ?? statusOrder.First();

            var taskDtos = new List<TaskVmDto>();
            foreach (var t in tasks)
            {
                var curStatusId = t.CurrentStatusId ?? startStatusId;
                if (!statusMeta.ContainsKey(curStatusId)) curStatusId = startStatusId;
                var meta = statusMeta[curStatusId];

                var assignees = assigneesMap.TryGetValue(t.Id, out var rows)
                    ? rows.Select(a => new MemberRefDto(
                            a.UserId.ToString(),
                            a.User?.UserName ?? a.User?.Email ?? "Member",
                            a.User?.Avatar
                        )).ToList()
                    : new List<MemberRefDto>();

                taskDtos.Add(new TaskVmDto
                {
                    Id = t.Id,
                    Code = t.Code ?? "",
                    Title = t.Title ?? "",
                    Type = t.Type ?? "Task",
                    Priority = t.Priority ?? "Medium",
                    Severity = t.Severity,

                    StoryPoints = t.Point,
                    EstimateHours = t.EstimateHours,
                    RemainingHours = t.RemainingHours,
                    DueDate = t.DueDate.HasValue ? new DateTimeOffset(DateTime.SpecifyKind(t.DueDate.Value, DateTimeKind.Utc)) : null,

                    SprintId = t.SprintId,
                    WorkflowStatusId = curStatusId,
                    StatusCode = meta.Code,
                    StatusCategory = meta.Category,

                    Assignees = assignees,
                    DependsOn = new List<Guid>(),
                    ParentTaskId = t.ParentTaskId,
                    CarryOverCount = t.CarryOverCount,

                    OpenedAt = t.CreateAt ?? DateTime.UtcNow,
                    UpdatedAt = t.UpdateAt ?? t.CreateAt ?? DateTime.UtcNow,
                    CreatedAt = t.CreateAt ?? DateTime.UtcNow,

                    SourceTicketId = t.SourceTaskId,
                    SourceTicketCode = null
                });
            }

            return new MultiSprintBoardResponseDto
            {
                Sprints = sprintDtos,
                Tasks = taskDtos
            };
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
                    IsFinal = x.IsEnd
                });
            var startStatusId = statuses.FirstOrDefault(x => x.IsStart)?.Id ?? statusOrder.First();
            var curStatusId = task.CurrentStatusId ?? startStatusId;
            if (!statusMeta.ContainsKey(curStatusId)) curStatusId = startStatusId;
            var meta = statusMeta[curStatusId];

            var assRows = await _repo.GetAssigneesForTasksAsync(new[] { task.Id }, ct);
            var assignees = assRows.Select(a => new MemberRefDto(
                a.UserId.ToString(),
                a.User?.UserName ?? a.User?.Email ?? "Member",
                a.User?.Avatar
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
