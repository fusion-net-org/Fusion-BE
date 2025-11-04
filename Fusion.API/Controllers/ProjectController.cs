
using Fusion.Repository.Bases.Page;
using Fusion.Repository.Bases.Page.Project;
using Fusion.Repository.Bases.Responses;
using Fusion.Repository.ViewModels;
using Fusion.Service.Commons.BaseResponses;
using Fusion.Service.IServices;
using Fusion.Service.ViewModels.Project.Requests;
using Fusion.Service.ViewModels.Project.Responses;
using Fusion.Service.ViewModels.ProjectMembers.Responses;
using Fusion.Service.ViewModels.Users.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Fusion.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProjectController : ControllerBase
    {
        private readonly IProjectService _service;

        public ProjectController(IProjectService service)
        {
            _service = service;
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseModel<ProjectsResponse>))]
        public async Task<IActionResult> CreateProject(CreateProjectRequest request, CancellationToken cancellationToken)
        {
            var response = await _service.CreateProjectAsync(request, cancellationToken);
            return Ok(ResponseModel<ProjectsResponse>.Ok(
                data: response,
                message: ResponseMessageHelper.FormatMessage(ResponseMessages.CREATE_SUCCESS, "Project")
            ));
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("{id:guid}/actor-project")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseModel<PagedResult<AllProjectOfMememberResponse>>))]
        public async Task<IActionResult> GetProjectByActorId([FromRoute] Guid id, [FromQuery]ProjectSearchRequest request, CancellationToken cancellationToken)
        {
            var response = await _service.GetProjectByActorIdAsync(id, request, cancellationToken);
            return Ok(ResponseModel<PagedResult<AllProjectOfMememberResponse>>.Ok(
                data: response,
                message: ResponseMessageHelper.FormatMessage(ResponseMessages.GET_SUCCESS, "Project List")
            ));
        }


        [Authorize(Roles = "Admin")]
        [HttpGet("{id:guid}/member-project")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseModel<PagedResult<AllProjectOfMememberResponse>>))]
        public async Task<IActionResult> GetProjectByMemberId([FromRoute] Guid id, [FromQuery]ProjectSearchRequest request, CancellationToken cancellationToken)
        {
            var response = await _service.GetProjectByMemberIdAsync(id, request, cancellationToken);
            return Ok(ResponseModel<PagedResult<AllProjectOfMememberResponse>>.Ok(
                data: response,
                message: ResponseMessageHelper.FormatMessage(ResponseMessages.GET_SUCCESS, "Project List")
            ));
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("count-project/status")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseModel<List<StatusCountResponse>>))]
        public async Task<IActionResult> GetCountProjectByStatus(CancellationToken cancellationToken)
        {
            var response = await _service.GetCountProjectByStatusAsync(cancellationToken);
            return Ok(ResponseModel<List<StatusCountResponse>>.Ok(
                data: response,
                message: ResponseMessageHelper.FormatMessage(ResponseMessages.GET_SUCCESS, "Project Status")
            ));
        }
    }
}
