using Fusion.Repository.Bases.Page;
using Fusion.Repository.Bases.Page.Task;
using Fusion.Repository.Bases.Responses;
using Fusion.Service.Commons.BaseResponses;     // ResponseModel, ResponseMessages
using Fusion.Service.IServices;
using Fusion.Service.Services;
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
    private readonly ITaskChecklistService _checklistSvc;

    public TaskController(ITaskService svc, ITaskChecklistService checklistSvc)
    {
        _svc = svc;
        _checklistSvc = checklistSvc;
    }

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
    #region Handle Task
    // PATCH /api/tasks/{id}/status-id
    [HttpPatch("tasks/{id:guid}/status-id")]
    [ProducesResponseType(typeof(ResponseModel<ProjectTaskResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ChangeStatusById(Guid id, [FromBody] ChangeStatusByIdRequest body, CancellationToken ct)
    {
        var uid = GetUserId(); if (uid is null)
            return Unauthorized(ResponseModel<string>.Error(StatusCodes.Status401Unauthorized, "Missing token"));

        if (body?.StatusId == Guid.Empty)
            return BadRequest(ResponseModel<string>.Error(StatusCodes.Status400BadRequest, "statusId is required"));

        var data = await _svc.ChangeStatusById(id, body.StatusId, uid.Value, ct);
        return Ok(ResponseModel<ProjectTaskResponse>.Ok(data, "Task status changed."));
    }

    // PUT /api/projects/{projectId}/sprints/{sprintId}/tasks/reorder
    [HttpPut("projects/{projectId:guid}/sprints/{sprintId:guid}/tasks/reorder")]
    [ProducesResponseType(typeof(ResponseModel<ProjectTaskResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Reorder(Guid projectId, Guid sprintId, [FromBody] ReorderTaskRequest req, CancellationToken ct)
    {
        var uid = GetUserId(); if (uid is null)
            return Unauthorized(ResponseModel<string>.Error(StatusCodes.Status401Unauthorized, "Missing token"));

        if (req == null || req.TaskId == Guid.Empty || req.ToStatusId == Guid.Empty)
            return BadRequest(ResponseModel<string>.Error(StatusCodes.Status400BadRequest, "Invalid payload"));

        var data = await _svc.ReorderAsync(projectId, sprintId, req.TaskId, req.ToStatusId, req.ToIndex, uid.Value, ct);
        return Ok(ResponseModel<ProjectTaskResponse>.Ok(data, "Reordered."));
    }

    // POST /api/tasks/{id}/move
    [HttpPost("tasks/{id:guid}/move")]
    [ProducesResponseType(typeof(ResponseModel<ProjectTaskResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> MoveToSprint(Guid id, [FromBody] TaskMoveRequest req, CancellationToken ct)
    {
        var uid = GetUserId(); if (uid is null)
            return Unauthorized(ResponseModel<string>.Error(StatusCodes.Status401Unauthorized, "Missing token"));

        if (req?.ToSprintId == Guid.Empty)
            return BadRequest(ResponseModel<string>.Error(StatusCodes.Status400BadRequest, "toSprintId is required"));

        var data = await _svc.MoveToSprintAsync(id, req.ToSprintId, uid.Value, ct);
        return Ok(ResponseModel<ProjectTaskResponse>.Ok(data, "Moved to sprint."));
    }

    // POST /api/tasks/{id}/mark-done
    [HttpPost("tasks/{id:guid}/mark-done")]
    [ProducesResponseType(typeof(ResponseModel<ProjectTaskResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> MarkDone(Guid id, CancellationToken ct)
    {
        var uid = GetUserId(); if (uid is null)
            return Unauthorized(ResponseModel<string>.Error(StatusCodes.Status401Unauthorized, "Missing token"));

        var data = await _svc.MarkDoneAsync(id, uid.Value, ct);
        return Ok(ResponseModel<ProjectTaskResponse>.Ok(data, "Marked done."));
    }

    // POST /api/tasks/{id}/split
    [HttpPost("tasks/{id:guid}/split")]
    [ProducesResponseType(typeof(ResponseModel<SplitTaskResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Split(Guid id, CancellationToken ct)
    {
        var uid = GetUserId(); if (uid is null)
            return Unauthorized(ResponseModel<string>.Error(StatusCodes.Status401Unauthorized, "Missing token"));

        var data = await _svc.SplitAsync(id, uid.Value, ct);
        return Ok(ResponseModel<SplitTaskResponse>.Ok(data, "Task split."));
    }
    #endregion
    // ===== Get tasks by SprintId (paged) =====
    [HttpGet("sprints/{sprintId:guid}/tasks")]
    [ProducesResponseType(typeof(ResponseModel<PagedResult<ProjectTaskResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTasksBySprintId(Guid sprintId, [FromQuery] TaskBySprintRequest request, CancellationToken ct)
    {
        var data = await _svc.GetTasksBySprintIdAsync(sprintId, request, ct);
        return Ok(ResponseModel<PagedResult<ProjectTaskResponse>>.Ok(
            data, ResponseMessageHelper.FormatMessage(ResponseMessages.GET_SUCCESS, "tasks")));
    }
    // ===== CheckList =====
    #region CheckList
    // GET /api/tasks/{taskId}/checklist
    [HttpGet("tasks/{taskId:guid}/checklist")]
    [ProducesResponseType(typeof(ResponseModel<IReadOnlyList<TaskChecklistItemResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetChecklist(Guid taskId, CancellationToken ct)
    {
        var data = await _checklistSvc.GetByTaskIdAsync(taskId, ct);
        return Ok(ResponseModel<IReadOnlyList<TaskChecklistItemResponse>>.Ok(
            data, ResponseMessageHelper.FormatMessage(ResponseMessages.GET_SUCCESS, "checklist")));
    }
    // POST /api/tasks/{taskId}/checklist
    [HttpPost("tasks/{taskId:guid}/checklist")]
    [ProducesResponseType(typeof(ResponseModel<TaskChecklistItemResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> AddChecklistItem(
        Guid taskId,
        [FromBody] TaskChecklistItemCreateRequest req,
        CancellationToken ct)
    {
        var uid = GetUserId();
        if (uid is null)
            return Unauthorized(ResponseModel<string>.Error(StatusCodes.Status401Unauthorized, "Missing token"));

        req.TaskId = taskId;

        var data = await _checklistSvc.AddAsync(req, uid.Value, ct);
        return Ok(ResponseModel<TaskChecklistItemResponse>.Ok(
            data, ResponseMessageHelper.FormatMessage(ResponseMessages.CREATE_SUCCESS, "checklist item")));
    }
    // PUT /api/tasks/{taskId}/checklist/{id}
    [HttpPut("tasks/{taskId:guid}/checklist/{id:guid}")]
    [ProducesResponseType(typeof(ResponseModel<TaskChecklistItemResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateChecklistItem(
        Guid taskId,
        Guid id,
        [FromBody] TaskChecklistItemUpdateRequest req,
        CancellationToken ct)
    {
        var uid = GetUserId();
        if (uid is null)
            return Unauthorized(ResponseModel<string>.Error(StatusCodes.Status401Unauthorized, "Missing token"));

        req.Id = id;

        var data = await _checklistSvc.UpdateAsync(req, uid.Value, ct);
        return Ok(ResponseModel<TaskChecklistItemResponse>.Ok(
            data, ResponseMessageHelper.FormatMessage(ResponseMessages.UPDATE_SUCCESS, "checklist item")));
    }
    // PATCH /api/tasks/{taskId}/checklist/{id}/done
    [HttpPatch("tasks/{taskId:guid}/checklist/{id:guid}/done")]
    [ProducesResponseType(typeof(ResponseModel<TaskChecklistItemResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ToggleChecklistDone(
        Guid taskId,
        Guid id,
        [FromBody] ToggleChecklistItemRequest? req,
        CancellationToken ct)
    {
        var uid = GetUserId();
        if (uid is null)
            return Unauthorized(ResponseModel<string>.Error(StatusCodes.Status401Unauthorized, "Missing token"));

        var data = await _checklistSvc.ToggleDoneAsync(id, req?.IsDone, uid.Value, ct);
        return Ok(ResponseModel<TaskChecklistItemResponse>.Ok(
            data, "Checklist item updated."));
    }
    // DELETE /api/tasks/{taskId}/checklist/{id}
    [HttpDelete("tasks/{taskId:guid}/checklist/{id:guid}")]
    [ProducesResponseType(typeof(ResponseModel<bool>), StatusCodes.Status200OK)]
    public async Task<IActionResult> DeleteChecklistItem(
        Guid taskId,
        Guid id,
        CancellationToken ct)
    {
        var uid = GetUserId();
        if (uid is null)
            return Unauthorized(ResponseModel<string>.Error(StatusCodes.Status401Unauthorized, "Missing token"));

        var ok = await _checklistSvc.DeleteAsync(id, uid.Value, ct);
        return Ok(ResponseModel<bool>.Ok(
            ok, ResponseMessageHelper.FormatMessage(ResponseMessages.DELETE_SUCCESS, "checklist item")));
    }

    #endregion
}
