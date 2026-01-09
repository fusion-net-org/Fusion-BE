using Fusion.Service.Commons.BaseResponses;
using Fusion.Service.IServices;
using Fusion.Service.ViewModels.ProjectComponent;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Fusion.API.Controllers
{
    [Route("api/project-components")]
    [ApiController]
    public class ProjectComponentController : ControllerBase
    {
        private readonly IProjectComponentService _projectComponentService;

        public ProjectComponentController(IProjectComponentService projectComponentService)
        {
            _projectComponentService = projectComponentService;
        }

        [HttpPost("bulk")]
        [ProducesResponseType(StatusCodes.Status201Created,Type = typeof(ResponseModel<List<ProjectComponentResponse>>))]
        public async Task<IActionResult> CreateMany(
          [FromBody] List<CreateProjectComponent> request,
          CancellationToken cancellationToken = default)
        {
            if (request == null || !request.Any())
            {
                return BadRequest(ResponseModel<string>.Error(
                    StatusCodes.Status400BadRequest,
                    "Project component list is empty"));
            }

            var result = await _projectComponentService.CreateManyAsync(request, cancellationToken);

            return StatusCode(
                StatusCodes.Status201Created,
                ResponseModel<List<ProjectComponentResponse>>.Ok(
                    data: result,
                    message: "Create project components successfully"
                )
            );
        }


        [HttpGet("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseModel<ProjectComponentResponse>))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            var result = await _projectComponentService.GetByIdAsync(id, cancellationToken);

            if (result == null)
            {
                return NotFound(ResponseModel<string>.Error(
                    StatusCodes.Status404NotFound,
                    "Project component not found"));
            }

            return Ok(ResponseModel<ProjectComponentResponse>.Ok(
                data: result,
                message: "Get project component by id successfully"));
        }

        [HttpGet("projects/{projectId:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseModel<List<ProjectComponentResponse>>))]
        public async Task<IActionResult> GetByProjectId(
            Guid projectId,
            CancellationToken cancellationToken = default)
        {
            var result = await _projectComponentService.GetByProjectIdAsync(projectId, cancellationToken);

            return Ok(ResponseModel<List<ProjectComponentResponse>>.Ok(
                data: result,
                message: "Get project components by project id successfully"));
        }

        [HttpGet("project-requests/{projectRequestId:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseModel<List<ProjectComponentResponse>>))]
        public async Task<IActionResult> GetByProjectRequestId(
            Guid projectRequestId,
            CancellationToken cancellationToken = default)
        {
            var result = await _projectComponentService.GetByProjectRequestIdAsync(projectRequestId, cancellationToken);

            return Ok(ResponseModel<List<ProjectComponentResponse>>.Ok(
                data: result,
                message: "Get project components by project request id successfully"));
        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseModel<ProjectComponentResponse>))]
        public async Task<IActionResult> Update(
            [FromBody] UpdateProjectComponent request,
            CancellationToken cancellationToken = default)
        {

            var result = await _projectComponentService.UpdateAsync(request, cancellationToken);

            return Ok(ResponseModel<ProjectComponentResponse>.Ok(
                data: result,
                message: "Update project component successfully"));
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseModel<bool>))]
        public async Task<IActionResult> Delete(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            var result = await _projectComponentService.DeleteAsync(id, cancellationToken);

            return Ok(ResponseModel<bool>.Ok(
                data: result,
                message: "Delete project component successfully"));
        }
    }
}
