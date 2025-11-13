// Fusion.API/Controllers/ProjectBoardController.cs
using Fusion.Repository.Bases.Page.ProjectBoard;
using Fusion.Service.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/projects/{projectId:guid}/sprint-board")]
public class ProjectBoardController : ControllerBase
{
    private readonly IProjectBoardService _svc;
    public ProjectBoardController(IProjectBoardService svc) => _svc = svc;

    // ====== 1) Lấy nhiều sprint + tasks ======
    [HttpGet]
    public async Task<IActionResult> GetBoard(
        Guid projectId,
        [FromQuery] Guid? sprintId,
        [FromQuery] bool includeClosed = false,
        [FromQuery] DateOnly? from = null,
        [FromQuery] DateOnly? to = null,
        CancellationToken ct = default)
    {
        var data = await _svc.GetSprintBoardAsync(projectId, sprintId, includeClosed, from, to, ct);
        return Ok(new { succeeded = true, statusCode = 200, message = "", data });
    }

    // ====== 2) Move task (đổi cột/đổi sprint + optional order) ======
    public sealed class MoveTaskRequest
    {
        public Guid ToStatusId { get; set; }
        public Guid? ToSprintId { get; set; }
        public int? NewOrder { get; set; }
        public Guid ActorUserId { get; set; }
    }

    [HttpPost("tasks/{taskId:guid}/move")]
    public async Task<IActionResult> MoveTask(
        Guid projectId, Guid taskId, [FromBody] MoveTaskRequest req, CancellationToken ct = default)
    {
        var dto = await _svc.MoveTaskAsync(projectId, taskId, req.ToStatusId, req.ToSprintId, req.NewOrder, req.ActorUserId, ct);
        return Ok(new { succeeded = true, statusCode = 200, message = "Moved", data = dto });
    }

    // ====== 3) Reorder trong 1 cột ======
    public sealed class ReorderItem { public Guid TaskId { get; set; } public int Order { get; set; } }
    public sealed class ReorderColumnRequest
    {
        public Guid SprintId { get; set; }
        public Guid StatusId { get; set; }
        public List<ReorderItem> Items { get; set; } = new();
    }

    [HttpPost("reorder")]
    public async Task<IActionResult> ReorderColumn(
        Guid projectId, [FromBody] ReorderColumnRequest req, CancellationToken ct = default)
    {
        await _svc.ReorderColumnAsync(
            projectId, req.SprintId, req.StatusId,
            req.Items.Select(i => (i.TaskId, i.Order)), ct);

        return Ok(new { succeeded = true, statusCode = 200, message = "Reordered" });
    }
}
