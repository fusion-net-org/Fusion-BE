
using Azure;
using FirebaseAdmin.Messaging;
using Fusion.API.Auth;
using Fusion.Repository.Bases.Page;
using Fusion.Repository.Bases.Page.Project;
using Fusion.Repository.Bases.Responses;
using Fusion.Repository.ViewModels;
using Fusion.Repository.ViewModels.Project;
using Fusion.Service.Commons.BaseResponses;
using Fusion.Service.Commons.Helpers;
using Fusion.Service.IServices;
using Fusion.Service.Services;
using Fusion.Service.ViewModels.Project.Requests;
using Fusion.Service.ViewModels.Project.Requests.Overview;
using Fusion.Service.ViewModels.Project.Responses;
using Fusion.Service.ViewModels.Project.Responses.Overview;
using Fusion.Service.ViewModels.ProjectMembers.Responses;
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
        private readonly ICurrentService _current;
        public ProjectController(IProjectService service, ICurrentService current)
        {
            _service = service;
            _current = current;
        }


        [Authorize(Roles = "Admin")]
        [HttpGet("growth-and-completion")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseModel<ProjectGrowthOverviewResponse>))]

        public async Task<IActionResult> GetProjectGrowthAndCompletion(
        [FromQuery] ProjectGrowthOverviewRequest req,
        CancellationToken ct)
        {
            var data = await _service.GetProjectGrowthOverviewAsync(req, ct);

            return Ok(ResponseModel<ProjectGrowthOverviewResponse>.Ok(
                 data: data,
                 message: ResponseMessageHelper.FormatMessage(ResponseMessages.GET_SUCCESS, "Growth and completion project")
            ));
        }
        
        [HttpPost("companies/{companyId:guid}/projects")]
        [HasPermission("PROJECT_CREATE")]
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
            var userIdClaim = _current.GetUserId();
            var result = await _service.GetProjectsForCompanyAsync(companyId, userIdClaim, req, ct);
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

        //[Authorize(Roles = "Admin")]
        [HttpGet("admin/projects")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseModel<PagedResult<ProjectSummaryResponseV2>>))]
        public async Task<IActionResult> GetProjectsForAdminAsync( [FromQuery] ProjectSummarySearchRequest request, CancellationToken cancellationToken = default)
        {
            var response = await _service.GetProjectsForAdminAsync(request, cancellationToken);
            return Ok(ResponseModel<PagedResult<ProjectSummaryResponseV2>>.Ok(
                data: response,
                message: ResponseMessageHelper.FormatMessage(ResponseMessages.GET_SUCCESS, "Project Admin")
            ));
        }

        [HttpGet("admin/{projectId:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseModel<ProjectSummaryResponseV2>))]
        public async Task<IActionResult> GetProjectsByIdForAdminAsync(Guid projectId, CancellationToken cancellationToken = default)
        {
            var response = await _service.GetProjectsByIdForAdminAsync(projectId, cancellationToken);
            return Ok(ResponseModel<ProjectSummaryResponseV2>.Ok(
                data: response,
                message: ResponseMessageHelper.FormatMessage(ResponseMessages.GET_SUCCESS, "Project Admin")
            ));
        }

        [HttpGet("detail/{projectId:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseModel<ProjectSummaryResponseV2>))]
        public async Task<IActionResult> GetProjectsByIdDetailAsync(Guid projectId, CancellationToken cancellationToken = default)
        {
            var response = await _service.GetProjectsByIdDetailsAsync(projectId, cancellationToken);
            return Ok(ResponseModel<ProjectSummaryResponseV2>.Ok(
                data: response,
                message: ResponseMessageHelper.FormatMessage(ResponseMessages.GET_SUCCESS, "Project Admin")
            ));
        }

        [HttpGet("projects/{projectId:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseModel<ProjectResponseVersion3>))]
        public async Task<IActionResult> GetProjectByID(Guid projectId, CancellationToken ct = default)
        {
            var response = await _service.GetProjectById(projectId, ct);
            return Ok(ResponseModel<ProjectResponseVersion3>.Ok(
                data: response,
                message: ResponseMessageHelper.FormatMessage(ResponseMessages.GET_SUCCESS, "Project Detail")
            ));
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("project-execution-overview")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseModel<ProjectExecutionOverviewResponse>))]
        public async Task<IActionResult> GetProjectExecutionOverview( [FromQuery] ProjectGrowthOverviewRequest req, CancellationToken ct)
        {
            var data = await _service.GetProjectExecutionOverviewAsync(req, ct);

            return Ok(ResponseModel<ProjectExecutionOverviewResponse>.Ok(
                data: data,
                message: ResponseMessageHelper.FormatMessage( ResponseMessages.GET_SUCCESS, "Project execution overview")
            ));
        }

        [HttpGet("projects/user")]
        [ProducesResponseType(typeof(ResponseModel<PagedResult<ProjectSummaryResponseV2>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetProjectsByUserId([FromQuery] ProjectSummarySearchRequest request, CancellationToken ct)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
                return Unauthorized(ResponseModel<string>.Error(StatusCodes.Status401Unauthorized, "Don't find token!"));

            var result = await _service.GetProjectsByUserIdAsync(request, userId, ct);
            return Ok(ResponseModel<PagedResult<ProjectSummaryResponseV2>>.Ok(result));
        }

        [HttpGet("companies/projects-by-company")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseModel<List<ProjectResponseVersion3>>))]
        public async Task<IActionResult> GetProjectsByCompany(
        [FromQuery] Guid companyId,
        [FromQuery] Guid? companyRequestId,
        [FromQuery] Guid? executorCompanyId,
        CancellationToken ct)
        {
            var result = await _service.GetProjectsByCompanyAsync(companyId, companyRequestId, executorCompanyId, ct);

            return Ok(ResponseModel<List<ProjectResponseVersion3>>.Ok(
                data: result,
                message: ResponseMessageHelper.FormatMessage(ResponseMessages.GET_SUCCESS, "Project List")
            ));
        }
        [HttpGet("companies/{companyId:guid}/projects-by-company-request")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseModel<List<ProjectResponseVersion3>>))]
        public async Task<IActionResult> GetProjectsByCompanyRequest(
        [FromRoute] Guid companyId,
        CancellationToken ct)
        {
            var result = await _service.GetProjectsByCompanyRequestAsync(companyId, ct);

            return Ok(ResponseModel<List<ProjectResponseVersion3>>.Ok(
                data: result,
                message: ResponseMessageHelper.FormatMessage(ResponseMessages.GET_SUCCESS, "Projects By Company Request")
            ));
        }
        [HttpPost("projects/{projectId:guid}/close")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseModel<bool>))]
        public async Task<IActionResult> CloseProject(Guid projectId, CancellationToken ct)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
                return Unauthorized(ResponseModel<string>.Error(StatusCodes.Status401Unauthorized, "Don't find token!"));

            var ok = await _service.CloseProjectAsync(projectId, userId, ct);

            return Ok(ResponseModel<bool>.Ok(
                data: ok,
                message: ResponseMessageHelper.FormatMessage(ResponseMessages.UPDATE_SUCCESS, "Close project")));
        }

        [HttpPost("projects/{projectId:guid}/reopen")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseModel<bool>))]
        public async Task<IActionResult> ReopenProject(Guid projectId, CancellationToken ct)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
                return Unauthorized(ResponseModel<string>.Error(StatusCodes.Status401Unauthorized, "Don't find token!"));

            var ok = await _service.ReopenProjectAsync(projectId, userId, ct);

            return Ok(ResponseModel<bool>.Ok(
                data: ok,
                message: ResponseMessageHelper.FormatMessage(ResponseMessages.UPDATE_SUCCESS, "Reopen project")));
        }

        [Authorize]
        [HttpGet("projects/{projectId:guid}/access")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseModel<ProjectAccessCheckResponse>))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> CheckProjectAccess(Guid projectId, CancellationToken ct)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
                return Unauthorized(ResponseModel<string>.Error(StatusCodes.Status401Unauthorized, "Don't find token!"));

            var data = await _service.CheckProjectAccessAsync(projectId, userId, ct);

            return Ok(ResponseModel<ProjectAccessCheckResponse>.Ok(
                data: data,
                message: ResponseMessageHelper.FormatMessage(ResponseMessages.GET_SUCCESS, "Project access")
            ));
        }
        [HttpGet("projects/{projectId:guid}/progress")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseModel<ProjectProgressResponse>))]
        public async Task<IActionResult> GetProjectProgress([FromRoute] Guid projectId, CancellationToken ct)
        {
            var data = await _service.GetProjectProgressAsync(projectId, ct);

            return Ok(ResponseModel<ProjectProgressResponse>.Ok(
                data: data,
                message: ResponseMessageHelper.FormatMessage(ResponseMessages.GET_SUCCESS, "Project progress")
            ));
        }

        [HttpPost("projects/{projectId:guid}/close/v2")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseModel<CloseProjectResponse>))]
        public async Task<IActionResult> CloseProjectV2(Guid projectId, [FromBody] CloseProjectRequest request, CancellationToken ct)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
                return Unauthorized(ResponseModel<string>.Error(StatusCodes.Status401Unauthorized, "Don't find token!"));

            var result = await _service.CloseProjectAsync(projectId, userId, request.ForceClose.Value, ct);

            var message = result.NeedConfirm? ProjectCloseMessages.NEED_CONFIRM: result.SentToRequester
                ? ProjectCloseMessages.SENT_TO_REQUESTER
                : ProjectCloseMessages.CLOSED;

            return Ok(ResponseModel<CloseProjectResponse>.Ok(
                data: result,
                message: message));

        }

        [HttpGet("projects/{projectId}/close-summary")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseModel<CloseProjectSummaryDto>))]
        public async Task<IActionResult> GetCloseProjectSummary(Guid projectId, CancellationToken ct)
        {
            var result = await _service
                .GetCloseProjectSummaryAsync(projectId, ct);

            return Ok(ResponseModel<CloseProjectSummaryDto>.Ok(
                            data: result,
                            message: ResponseMessageHelper.FormatMessage(ResponseMessages.GET_SUCCESS, "Project close progress")));
        }

        public static class ProjectCloseMessages
        {
            public const string NEED_CONFIRM = "Project is not completed. Confirmation is required.";
            public const string SENT_TO_REQUESTER = "Close project request has been sent to requester.";
            public const string CLOSED = "Project has been closed successfully.";
        }
    }
}
