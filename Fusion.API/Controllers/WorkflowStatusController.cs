using System.Threading.Tasks;
using Fusion.Repository.Bases.Page;
using Fusion.Repository.Bases.Page.Workflow;
using Fusion.Repository.Bases.Page.Workflowstatus;
using Fusion.Repository.Bases.Page.Workflowstatus;
using Fusion.Service.Commons.BaseResponses;
using Fusion.Service.IServices;
using Fusion.Service.ViewModels.WorkflowStatus;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Fusion.API.Controllers
{
    [Route("api/workflow-status")]
    [ApiController]
    public class WorkflowStatusController : ControllerBase
    {
        private readonly IWorkflowStatusService _workflowStatusService;

        public WorkflowStatusController(IWorkflowStatusService workflowStatusService)
        {
            _workflowStatusService = workflowStatusService;
        }

        [HttpGet("by-project")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseModel<PagedResult<WorkflowStatusResponse>>))]
        public async Task<IActionResult> GetByProject([FromQuery] WorkflowStatusPagedRequest request)
        {
            if (request == null || request.ProjectId == Guid.Empty)
            {
                return BadRequest(ResponseModel<PagedResult<WorkflowStatusResponse>>.Error(
                    StatusCodes.Status400BadRequest,
                    "ProjectId is required"));
            }

            var result = await _workflowStatusService.GetWorkflowStatusesByProjectAsync(request);

            return Ok(ResponseModel<PagedResult<WorkflowStatusResponse>>.Ok(
                data: result,
                message: "Get workflow statuses by project successfully"));
        }
    }
}
