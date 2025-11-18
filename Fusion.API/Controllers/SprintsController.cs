using System.Security.Claims;
using Fusion.API.Auth;
using Fusion.API.Context;
using Fusion.Repository.Bases.Responses;
using Fusion.Service.Commons.BaseResponses;
using Fusion.Service.Services;
using Fusion.Service.ViewModels.Sprint;
using Fusion.Service.ViewModels.Sprint.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Fusion.API.Controllers
{
    [Route("api/sprints")]
    [ApiController]
    [Authorize]
    public class SprintsController : ControllerBase
    {
        private readonly ISprintService _service;

        public SprintsController(ISprintService service)
        {
            _service = service;
        }

        /// <summary>
        /// Create a new sprint
        /// </summary>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseModel<SprintVm>))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Create([FromBody] SprintCreateRequest req, CancellationToken ct)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized(ResponseModel<string>.Error(
                    StatusCodes.Status401Unauthorized,
                    "Don't find token!"));
            }

            var vm = await _service.CreateAsync(userId, req, ct);

            return Ok(ResponseModel<SprintVm>.Ok(
                data: vm,
                message: ResponseMessageHelper.FormatMessage(ResponseMessages.CREATE_SUCCESS, "sprint")));
        }

        /// <summary>
        /// Get sprint by id
        /// </summary>
        [HttpGet("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseModel<SprintVm>))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(Guid id, [FromQuery] Guid projectId, CancellationToken ct)
        {
            var vm = await _service.GetAsync(id, projectId, ct);

            return Ok(ResponseModel<SprintVm>.Ok(
                data: vm,
                message: ResponseMessageHelper.FormatMessage(ResponseMessages.GET_SUCCESS, "sprint")));
        }

        /// <summary>
        /// Start sprint (Planning -> Active)
        /// </summary>
        [HttpPost("{id:guid}/start")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseModel<bool>))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Start(Guid id, [FromQuery] Guid projectId, CancellationToken ct)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out _))
            {
                return Unauthorized(ResponseModel<string>.Error(
                    StatusCodes.Status401Unauthorized,
                    "Don't find token!"));
            }

            await _service.StartAsync(id, projectId, ct);

            return Ok(ResponseModel<bool>.Ok(
                data: true,
                message: ResponseMessageHelper.FormatMessage(ResponseMessages.UPDATE_SUCCESS, "sprint")));
        }

        /// <summary>
        /// Add tasks to sprint
        /// </summary>
        [HttpPost("{id:guid}/tasks")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseModel<bool>))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> AddTasks(Guid id, [FromQuery] Guid projectId, [FromBody] List<Guid> taskIds, CancellationToken ct)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized(ResponseModel<string>.Error(
                    StatusCodes.Status401Unauthorized,
                    "Don't find token!"));
            }

            await _service.AddTasksAsync(id, projectId, taskIds, userId, ct);

            return Ok(ResponseModel<bool>.Ok(
                data: true,
                message: ResponseMessageHelper.FormatMessage(ResponseMessages.UPDATE_SUCCESS, "sprint tasks")));
        }

        /// <summary>
        /// Complete sprint
        /// </summary>
        [HttpPost("{id:guid}/complete")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseModel<bool>))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Complete(
            Guid id,
            [FromQuery] Guid projectId,
            [FromQuery] bool carryBacklog,
            [FromQuery] Guid? nextSprintId,
            CancellationToken ct)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out _))
            {
                return Unauthorized(ResponseModel<string>.Error(
                    StatusCodes.Status401Unauthorized,
                    "Don't find token!"));
            }

            await _service.CompleteAsync(id, projectId, carryBacklog, nextSprintId, ct);

            return Ok(ResponseModel<bool>.Ok(
                data: true,
                message: ResponseMessageHelper.FormatMessage(ResponseMessages.UPDATE_SUCCESS, "sprint")));
        }
        [HttpGet("projects/{projectId:guid}")]
        public async Task<IActionResult> GetProjectSprints([FromRoute] Guid projectId, [FromQuery] SprintQuery query, CancellationToken ct)
        {
            var result = await _service.GetProjectSprintsAsync(projectId, query, ct);
            return Ok(new
            {
                data = new
                {
                    items = result.Items,
                    totalCount = result.TotalCount,
                    pageNumber = result.PageNumber,
                    pageSize = result.PageSize
                }
            });
        }
        /// <summary>
        /// Get charts data for a project (Sprint Status, Workload, Task Progress)
        /// </summary>
        [HttpGet("projects/{projectId:guid}/charts")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseModel<SprintChartsVm>))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetCharts([FromRoute] Guid projectId, CancellationToken ct)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out _))
            {
                return Unauthorized(ResponseModel<string>.Error(
                    StatusCodes.Status401Unauthorized,
                    "Don't find token!"));
            }

            var data = await _service.GetChartsDataAsync(projectId, ct);

            return Ok(ResponseModel<SprintChartsVm>.Ok(
                data: data,
                message: "Charts data retrieved successfully"
            ));
        }

        // GET /api/projects/{projectId}/sprints/{sprintId}
        [HttpGet("{sprintId:guid}")]
        public async Task<IActionResult> GetDetail([FromRoute] Guid projectId, [FromRoute] Guid sprintId, CancellationToken ct)
        {
            var vm = await _service.GetProjectSprintDetailAsync(projectId, sprintId, ct);
            return vm is null ? NotFound() : Ok(new { data = vm });
        }
    }
}
