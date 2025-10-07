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


        [HttpDelete("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> DeleteProjectRequestAsync(Guid id, CancellationToken cancellationToken)
        {
            var emailClaim = User.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Email || c.Type == ClaimTypes.Email || c.Type == "email");
            var email = emailClaim?.Value; if (email == null)
            {
                return Unauthorized(ResponseModel<CompanyResponse>.Error(
                    statusCode: StatusCodes.Status401Unauthorized,
                    message: "Unauthorized: User identity not found"
                ));
            }

            var result = await _projectRequestService.DeleteProjectRequestAsync(id, cancellationToken);
            return Ok(ResponseModel<bool>.Ok(
                data: result,
                message: "Delete project request successfully"));
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
        public async Task<IActionResult> RejectProjectRequest(Guid id, string reason, CancellationToken cancellationToken)
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
    }
}
