using Fusion.Repository.Bases.Page;
using Fusion.Repository.Bases.Responses;
using Fusion.Service.Commons.BaseResponses;     // ResponseModel, ResponseMessages
using Fusion.Service.IServices;
using Fusion.Service.ViewModels.Task.Request;
using Fusion.Service.ViewModels.Task.Response;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Fusion.API.Controllers;

[ApiController]
[Authorize]
[Route("api")]
public class TaskController : ControllerBase
{
    private readonly ITaskService _svc;

    public TaskController(ITaskService svc) => _svc = svc;

    private Guid? GetUserId()
    {
        var s = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
             ?? User.FindFirst("sub")?.Value;
        return Guid.TryParse(s, out var id) ? id : (Guid?)null;
    }

    // ===== Create =====
    // Cho phép: POST /api/tasks  (ProjectId nhận trong body)
    [HttpPost("tasks")]
    [ProducesResponseType(typeof(ResponseModel<ProjectTaskResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Create([FromBody] ProjectTaskRequest req, CancellationToken ct)
    {
        var uid = GetUserId();
        if (uid is null)
            return Unauthorized(ResponseModel<string>.Error(StatusCodes.Status401Unauthorized, "Missing token"));

        var data = await _svc.CreateTaskAsync(req, uid.Value, ct);
        return Ok(ResponseModel<ProjectTaskResponse>.Ok(
            data, ResponseMessageHelper.FormatMessage(ResponseMessages.CREATE_SUCCESS, "task")));
    }

    // Option: POST /api/projects/{projectId}/tasks  (FE thích pass projectId trên route)
    [HttpPost("projects/{projectId:guid}/tasks")]
    [ProducesResponseType(typeof(ResponseModel<ProjectTaskResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> CreateUnderProject(Guid projectId, [FromBody] ProjectTaskRequest req, CancellationToken ct)
    {
        req.ProjectId = projectId;
        return await Create(req, ct);
    }

    // ===== Update =====
    [HttpPut("tasks/{id:guid}")]
    [ProducesResponseType(typeof(ResponseModel<ProjectTaskResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Update(Guid id, [FromBody] ProjectTaskRequest req, CancellationToken ct)
    {
        var uid = GetUserId();
        if (uid is null)
            return Unauthorized(ResponseModel<string>.Error(StatusCodes.Status401Unauthorized, "Missing token"));

        req.Id = id;
        var data = await _svc.UpdateTaskAsync(req, uid.Value, ct);
        return Ok(ResponseModel<ProjectTaskResponse>.Ok(
            data, ResponseMessageHelper.FormatMessage(ResponseMessages.UPDATE_SUCCESS, "task")));
    }

    // ===== Get by id =====
    [HttpGet("tasks/{id:guid}")]
    [ProducesResponseType(typeof(ResponseModel<ProjectTaskResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var data = await _svc.GetTaskByIdAsync(id, ct);
        return Ok(ResponseModel<ProjectTaskResponse>.Ok(
            data, ResponseMessageHelper.FormatMessage(ResponseMessages.GET_SUCCESS, "task")));
    }

    // ===== Get paged =====
    [HttpGet("tasks")]
    [ProducesResponseType(typeof(ResponseModel<PagedResult<ProjectTaskResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] PagedRequest paging, CancellationToken ct)
    {
        var data = await _svc.GetAllTasksAsync(paging, ct);
        return Ok(ResponseModel<PagedResult<ProjectTaskResponse>>.Ok(
            data, ResponseMessageHelper.FormatMessage(ResponseMessages.GET_SUCCESS, "tasks")));
    }

    // ===== Delete (soft) =====
    [HttpDelete("tasks/{id:guid}")]
    [ProducesResponseType(typeof(ResponseModel<bool>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var uid = GetUserId();
        if (uid is null)
            return Unauthorized(ResponseModel<string>.Error(StatusCodes.Status401Unauthorized, "Missing token"));

        var ok = await _svc.DeleteTaskAsync(id, uid.Value, ct);
        return Ok(ResponseModel<bool>.Ok(
            ok, ResponseMessageHelper.FormatMessage(ResponseMessages.DELETE_SUCCESS, "task")));
    }

    // ===== Change status (qua query ?status= hoặc body) =====
    public sealed class ChangeStatusRequest { public string? Status { get; set; } }

    [HttpPatch("tasks/{id:guid}/status")]
    [ProducesResponseType(typeof(ResponseModel<ProjectTaskResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ChangeStatus(
        Guid id,
        [FromQuery] string? status,
        [FromBody] ChangeStatusRequest? body,
        CancellationToken ct = default)
    {
        var uid = GetUserId();
        if (uid is null)
            return Unauthorized(ResponseModel<string>.Error(StatusCodes.Status401Unauthorized, "Missing token"));

        var value = (body?.Status ?? status)?.Trim();
        if (string.IsNullOrWhiteSpace(value))
            return BadRequest(ResponseModel<string>.Error(StatusCodes.Status400BadRequest, "status is required"));

        var data = await _svc.ChangeStatus(id, value!, uid.Value, ct);
        return Ok(ResponseModel<ProjectTaskResponse>.Ok(
            data, $"Task status changed to '{value}'."));
    }
}
