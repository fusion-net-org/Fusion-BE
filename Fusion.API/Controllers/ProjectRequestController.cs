using Fusion.Repository.Bases.Page;
using Fusion.Repository.Bases.Page.Company;
using Fusion.Repository.Bases.Page.ProjectRequest;
using Fusion.Repository.Bases.Responses;
using Fusion.Service.Commons.BaseResponses;
using Fusion.Service.IServices;
using Fusion.Service.Services;
using Fusion.Service.ViewModels.Companies.Requests;
using Fusion.Service.ViewModels.Companies.Responses;
using Fusion.Service.ViewModels.Projects.Requests;
using Fusion.Service.ViewModels.Projects.Responses;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Fusion.API.Controllers
{
    [Route("api/projectrequest")]
    [ApiController]
    public class ProjectRequestController : ControllerBase
    {
        private readonly IProjectRequestService _projectRequestService;

        public ProjectRequestController(IProjectRequestService projectRequestService)
        {
            _projectRequestService = projectRequestService;
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(ResponseModel<ProjectRequestResponse>))]
        public async Task<IActionResult> CreateProjectRequest([FromQuery] CreateProjectRequestRequest request, CancellationToken cancellationToken = default)
        {
            var emailClaim = User.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Email || c.Type == ClaimTypes.Email || c.Type == "email");
            var email = emailClaim?.Value; if (email == null)
            {
                return Unauthorized(ResponseModel<CompanyResponse>.Error(
                    statusCode: StatusCodes.Status401Unauthorized,
                    message: "Unauthorized: User identity not found"
                ));
            }

            var result = await _projectRequestService.AddProjectRequestAsync(request, email, cancellationToken);

            return Ok(ResponseModel<ProjectRequestResponse>.Ok(
                data: result,
                message: ResponseMessageHelper.FormatMessage(ResponseMessages.CREATE_SUCCESS, "project request")));
        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseModel<ProjectRequestResponse>))]
        public async Task<IActionResult> UpdateProjectRequest(Guid id, [FromQuery] UpdateProjectRequestRequest request, CancellationToken cancellationToken = default)
        {
            var emailClaim = User.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Email || c.Type == ClaimTypes.Email || c.Type == "email");
            var email = emailClaim?.Value; if (email == null)
            {
                return Unauthorized(ResponseModel<CompanyResponse>.Error(
                    statusCode: StatusCodes.Status401Unauthorized,
                    message: "Unauthorized: User identity not found"
                ));
            }

            var result = await _projectRequestService.UpdateProjectRequestAsync(id, request, email, cancellationToken);

            return Ok(ResponseModel<ProjectRequestResponse>.Ok(
                data: result,
                message: ResponseMessageHelper.FormatMessage(ResponseMessages.UPDATE_SUCCESS, "project request")));
        }

        [HttpGet("paged/{companyId:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseModel<PagedResult<ProjectRequestResponse>>))]
        public async Task<IActionResult> GetProjectRequestPaged(Guid companyId, [FromQuery] ProjectRequestSearchRequest request, CancellationToken cancellationToken)
        {
            var result = await _projectRequestService.SearchProjectRequestAsync(request, companyId, cancellationToken);
            return Ok(ResponseModel<PagedResult<ProjectRequestResponse>>.Ok(
                data: result,
                message: "Get paged project request successfully"));
        }

        [HttpGet("paged/admin")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseModel<PagedResult<ProjectRequestResponseV2>>))]
        public async Task<IActionResult> GetProjectRequestAdminPaged([FromQuery] ProjectRequestSearchAdminRequest request, CancellationToken cancellationToken)
        {

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized(new { Message = "Invalid or missing token" });
            }


            var result = await _projectRequestService.SearchProjectRequestAdminAsync(request, userId, cancellationToken);
            return Ok(ResponseModel<PagedResult<ProjectRequestResponseV2>>.Ok(
                data: result,
                message: "Get paged project request successfully"));
        }


        [HttpDelete("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> DeleteProjectRequestAsync(Guid id, string? reason,CancellationToken cancellationToken)
        {
            var emailClaim = User.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Email || c.Type == ClaimTypes.Email || c.Type == "email");
            var email = emailClaim?.Value; if (email == null)
            {
                return Unauthorized(ResponseModel<CompanyResponse>.Error(
                    statusCode: StatusCodes.Status401Unauthorized,
                    message: "Unauthorized: User identity not found"
                ));
            }
            //var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            //if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            //{
            //    return Unauthorized(ResponseModel<string>.Error(
            //        StatusCodes.Status401Unauthorized,
            //        "Don't find token!"));
            //}
            var result = await _projectRequestService.DeleteProjectRequestAsync(id,reason,cancellationToken);
            return Ok(ResponseModel<bool>.Ok(
                data: result,
                message: "Delete project request successfully"));
        }

        [HttpPost("{id:guid}/restore")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseModel<bool>))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> RestoreProjectRequest(Guid id, CancellationToken cancellationToken)
        {
            var emailClaim = User.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Email
                                                              || c.Type == ClaimTypes.Email
                                                              || c.Type == "email");
            var email = emailClaim?.Value;
            if (email == null)
            {
                return Unauthorized(ResponseModel<CompanyResponse>.Error(
                    statusCode: StatusCodes.Status401Unauthorized,
                    message: "Unauthorized: User identity not found"
                ));
            }

            var result = await _projectRequestService.RestoreProjectRequestAsync(id, cancellationToken);

            return Ok(ResponseModel<bool>.Ok(
                data: result,
                message: "Project request restored successfully"
            ));
        }



        [HttpGet("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseModel<ProjectRequestResponse>))]
        public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
        {
            var result = await _projectRequestService.GetProjectRequestByIdAsync(id, cancellationToken);
            return Ok(ResponseModel<ProjectRequestResponse>.Ok(
                data: result,
                message: "Get project request by id successfully"));
        }

        [HttpGet("{id:guid}/admin")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseModel<ProjectRequestResponseV2>))]
        public async Task<IActionResult> GetProjectRequestAdminById(Guid id, CancellationToken cancellationToken)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized(new { Message = "Invalid or missing token" });
            }

            var result = await _projectRequestService.GetProjectRequestAdminByIdAsync(id, userId, cancellationToken);
            return Ok(ResponseModel<ProjectRequestResponseV2>.Ok(
                data: result,
                message: "Get project request by id successfully"));
        }

        [HttpPost("{id:guid}/accept")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseModel<ProjectRequestResponse>))]
        public async Task<IActionResult> AcceptProjectRequest(Guid id, CancellationToken cancellationToken)
        {
            var emailClaim = User.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Email || c.Type == ClaimTypes.Email || c.Type == "email");
            var executorEmail = emailClaim?.Value; if (executorEmail == null)
            {
                return Unauthorized(ResponseModel<CompanyResponse>.Error(
                    statusCode: StatusCodes.Status401Unauthorized,
                    message: "Unauthorized: User identity not found"
                ));
            }

            var result = await _projectRequestService.AcceptProjectRequestAsync(id, executorEmail, cancellationToken);

            return Ok(ResponseModel<ProjectRequestResponse>.Ok(
                data: result,
                message: "Project request accepted successfully"));
        }

        [HttpPost("{id:guid}/reject")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseModel<ProjectRequestRejectResponse>))]
        public async Task<IActionResult> RejectProjectRequest(Guid id, string? reason, CancellationToken cancellationToken)
        {
            var emailClaim = User.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Email || c.Type == ClaimTypes.Email || c.Type == "email");
            var executorEmail = emailClaim?.Value; if (executorEmail == null)
            {
                return Unauthorized(ResponseModel<CompanyResponse>.Error(
                    statusCode: StatusCodes.Status401Unauthorized,
                    message: "Unauthorized: User identity not found"
                ));
            }
            var result = await _projectRequestService.RejectProjectRequestAsync(
                id, executorEmail, reason, cancellationToken);

            return Ok(ResponseModel<ProjectRequestRejectResponse>.Ok(
                data: result,
                message: "Project request rejected successfully"));
        }

        [HttpGet("companies/{companyId:guid}/partners/{partnerId:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseModel<PagedResult<ProjectRequestResponse>>))]
        public async Task<IActionResult> GetProjectRequestPartnerPaged(Guid companyId, Guid partnerId, [FromQuery] ProjectRequestSearchRequest request, CancellationToken cancellationToken)
        {
            var result = await _projectRequestService.SearchProjectRequestAsync(request, companyId, partnerId, cancellationToken);
            return Ok(ResponseModel<PagedResult<ProjectRequestResponse>>.Ok(
                data: result,
                message: "Get paged project request successfully"));
        }
        [HttpPost("{id:guid}/close")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseModel<bool>))]
        public async Task<IActionResult> CloseProjectRequest(Guid id, CancellationToken ct)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
                return Unauthorized(ResponseModel<string>.Error(StatusCodes.Status401Unauthorized, "Invalid or missing token"));

            var ok = await _projectRequestService.CloseProjectRequestAsync(id, userId, ct);

            return Ok(ResponseModel<bool>.Ok(
                data: ok,
                message: ResponseMessageHelper.FormatMessage(ResponseMessages.UPDATE_SUCCESS, "Close project request")));
        }

        [HttpPost("{id:guid}/reopen")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseModel<bool>))]
        public async Task<IActionResult> ReopenProjectRequest(Guid id, CancellationToken ct)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
                return Unauthorized(ResponseModel<string>.Error(StatusCodes.Status401Unauthorized, "Invalid or missing token"));

            var ok = await _projectRequestService.ReopenProjectRequestAsync(id, userId, ct);

            return Ok(ResponseModel<bool>.Ok(
                data: ok,
                message: ResponseMessageHelper.FormatMessage(ResponseMessages.UPDATE_SUCCESS, "Reopen project request")));
        }

    }
}
