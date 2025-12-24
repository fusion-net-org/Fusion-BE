using Fusion.Repository.Bases.Page;
using Fusion.Repository.Bases.Page.CompanyActivityLog;
using Fusion.Repository.Bases.Page.TaskLogEvent;
using Fusion.Repository.Entities;
using Fusion.Service.Commons.BaseResponses;
using Fusion.Service.IServices;
using Fusion.Service.Services;
using Fusion.Service.ViewModels.TaskLogEventQuery;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Fusion.API.Controllers;

[ApiController]
[Authorize]
[Route("api")] // ✅ base route
public sealed class TaskLogEventsController : ControllerBase
{
    private readonly ITaskLogEventService _service;

    public TaskLogEventsController(ITaskLogEventService service)
    {
        _service = service;
    }

    // ✅ FE đang gọi đúng endpoint này
    // GET /api/projects/{projectId}/activities?PageNumber=1&PageSize=25&SortDescending=true
  

    // GET /api/projects/{projectId}/activities/{id}
    [HttpGet("projects/{projectId:guid}/activities/{id:long}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseModel<TaskLogEvent>))]
    public async Task<IActionResult> GetProjectActivityById(
        [FromRoute] Guid projectId,
        [FromRoute] long id,
        CancellationToken ct)
    {
        // nếu muốn chặt chẽ: nên check id thuộc projectId trong repo/service
        var log = await _service.GetByIdAsync(id, ct);
        return Ok(ResponseModel<TaskLogEvent>.Ok(log, "Fetched activity successfully"));
    }

   

  

    // PUT /api/tasklogevents/update_isView?taskId=...&isView=true
    [HttpPut("tasklogevents/update_isView")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseModel<bool>))]
    public async Task<IActionResult> UpdateIsView(
        [FromQuery] Guid taskId,
        [FromQuery] bool isView,
        CancellationToken ct)
    {
        var ok = await _service.UpdateIsViewForTaskAsync(taskId, isView, ct);
        return Ok(ResponseModel<bool>.Ok(ok, "Visibility updated."));
    }

    private static TaskLogEventPagedSearchRequest BuildRequest(TaskLogEventQuery? query)
        => new()
        {
            KeyWord = query?.Keyword,
            Action = query?.Action,
            ActorId = query?.ActorId,
            DateRange = (query?.From.HasValue == true || query?.To.HasValue == true)
                ? new DateRangeRequest { From = query!.From, To = query!.To }
                : null,
            PageNumber = query?.PageNumber ?? 1,
            PageSize = query?.PageSize ?? 10,
            SortColumn = string.IsNullOrWhiteSpace(query?.SortColumn) ? "CreatedAt" : query!.SortColumn,
            SortDescending = query?.SortDescending ?? true
        };
    [HttpGet("projects/{projectId:guid}/activities")]
    public async Task<IActionResult> GetProjectActivitiesPaged(
     [FromRoute] Guid projectId,
     [FromQuery] TaskLogEventQuery? query,
     CancellationToken ct)
    {
        var search = query?.Keyword;
        if (string.IsNullOrWhiteSpace(search))
            search = HttpContext.Request.Query["Search"].ToString();

        var action = query?.Action;
        if (string.IsNullOrWhiteSpace(action))
            action = HttpContext.Request.Query["Actions"].ToString();

        // ✅ fallback parse DateOnly
        DateOnly? from = query?.From;
        if (!from.HasValue)
        {
            var raw = HttpContext.Request.Query["From"].ToString();
            if (DateOnly.TryParse(raw, out var d)) from = d;
        }

        DateOnly? to = query?.To;
        if (!to.HasValue)
        {
            var raw = HttpContext.Request.Query["To"].ToString();
            if (DateOnly.TryParse(raw, out var d)) to = d;
        }

        var req = new TaskLogEventPagedSearchRequest
        {
            KeyWord = search,
            Action = action,
            ActorId = query?.ActorId,

            DateRange = (from.HasValue || to.HasValue)
                ? new DateRangeRequest { From = from, To = to }
                : null,

            PageNumber = query?.PageNumber ?? 1,
            PageSize = query?.PageSize ?? 10,
            SortColumn = string.IsNullOrWhiteSpace(query?.SortColumn) ? "CreatedAt" : query!.SortColumn,
            SortDescending = query?.SortDescending ?? true
        };

        var result = await _service.GetPagedProjectActivitiesAsync(projectId, req, ct);
        return Ok(ResponseModel<PagedResult<ProjectActivityVm>>.Ok(result, "Fetched project activities successfully"));
    }
    // GET /api/tasklogevents?taskId=...
    [HttpGet("tasklogevents")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseModel<PagedResult<ProjectActivityVm>>))]
    public async Task<IActionResult> GetTaskLogsPaged(
        [FromQuery] Guid taskId,
        [FromQuery] TaskLogEventQuery? query,
        CancellationToken ct)
    {
        var req = BuildRequest(query);
        var result = await _service.GetPagedTaskLogsVmByTaskIdAsync(taskId, req, ct);

        return Ok(ResponseModel<PagedResult<ProjectActivityVm>>.Ok(result, "Fetched task logs successfully"));
    }

    [HttpGet("tasklogevents/{id:long}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseModel<ProjectActivityVm>))]
    public async Task<IActionResult> GetById([FromRoute] long id, CancellationToken ct)
    {
        var log = await _service.GetTaskLogVmByIdAsync(id, ct);
        return Ok(ResponseModel<ProjectActivityVm>.Ok(log, "Fetched task log successfully"));
    }

}
