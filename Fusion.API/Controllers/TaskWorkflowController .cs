using Fusion.Service.Commons.Helpers;
using Fusion.Service.Services;
using Fusion.Service.ViewModels.Task.Request;
using Fusion.Service.ViewModels.Task.Response;
using Microsoft.AspNetCore.Mvc;

namespace Fusion.API.Controllers
{
    [ApiController]
    [Route("api/tasks/{taskId:guid}/workflow-assignments")]
    public class TaskWorkflowController : ControllerBase
    {
        private readonly ITaskWorkflowService _service;
        private readonly ICurrentService _current;

        public TaskWorkflowController(
            ITaskWorkflowService service,
            ICurrentService current)
        {
            _service = service;
            _current = current;
        }

        [HttpGet]
        public async Task<ActionResult<TaskWorkflowAssignmentsResponse>> Get(Guid taskId, CancellationToken ct)
        {
            var result = await _service.GetAssignmentsForTaskAsync(taskId, ct);
            return Ok(result);
        }

        [HttpPut]
        public async Task<ActionResult<TaskWorkflowAssignmentsResponse>> Put(
            Guid taskId,
            [FromBody] TaskWorkflowAssignmentsRequest request,
            CancellationToken ct)
        {
            request.TaskId = taskId;
            var actorId = _current.GetUserId();
            var result = await _service.UpsertAssignmentsForTaskAsync(request, actorId, ct);
            return Ok(result);
        }
    }

}
