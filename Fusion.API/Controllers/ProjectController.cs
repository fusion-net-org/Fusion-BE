
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
using System.Security.Claims;

namespace Fusion.API.Controllers
{
    [Route("api")]
    [ApiController]
    public class ProjectController : ControllerBase
    {
        private readonly IProjectService _service;

        public ProjectController(IProjectService service)
        {
            _service = service;
        }

        [HttpPost("companies/{companyId:guid}/projects")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseModel<ProjectDetailResponse>))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> CreateProject([FromRoute] Guid companyId,
                                                       [FromBody] ProjectCreateRequest request,
                                                       CancellationToken ct)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
                return Unauthorized(ResponseModel<string>.Error(StatusCodes.Status401Unauthorized, "Don't find token!"));

            var result = await _service.CreateProjectAsync(companyId, request, userId, ct);

            return Ok(ResponseModel<ProjectDetailResponse>.Ok(
                data: result,
                message: ResponseMessageHelper.FormatMessage(ResponseMessages.CREATE_SUCCESS, "project")));
        }
        [HttpGet("companies/{companyId:guid}/projects")]
        [ProducesResponseType(typeof(ResponseModel<PagedResult<ProjectListItemResponse>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetProjects(Guid companyId, [FromQuery] ProjectListSearchRequest req, CancellationToken ct)
        {
            var result = await _service.GetProjectsForCompanyAsync(companyId, req, ct);
            return Ok(ResponseModel<ProjectListResult>.Ok(result));
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
