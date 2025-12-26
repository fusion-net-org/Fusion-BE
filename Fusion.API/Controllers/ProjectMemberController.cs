using Fusion.API.Auth;
using Fusion.Repository.Bases.Page;
using Fusion.Repository.Bases.Page.ProjectMember;
using Fusion.Repository.Bases.Responses;
using Fusion.Service.Commons.BaseResponses;
using Fusion.Service.IServices;
using Fusion.Service.ViewModels.ProjectMembers.Request;
using Fusion.Service.ViewModels.ProjectMembers.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Fusion.API.Controllers
{
    [Route("api/projectmember")]
    [ApiController]
    [Authorize]
    public class ProjectMemberController : ControllerBase
    {
        private readonly IProjectMemberService _projectMemberService;

        public ProjectMemberController(IProjectMemberService projectMemberService)
        {
            _projectMemberService = projectMemberService;
        }

        /// <summary>
        /// GetProjectsByMember
        /// </summary>
        [HttpGet("{companyId:guid}/{memberId:guid}")]
        public async Task<IActionResult> GetProjectsByMember(
            Guid companyId,
            Guid memberId,
            [FromQuery] ProjectMemberSearchRequest request,
            CancellationToken cancellationToken)
        {
            var result = await _projectMemberService.GetProjectsByMemberAsync(
                companyId,
                memberId,
                request,
                cancellationToken
            );

            return Ok(ResponseModel<PagedResult<MemberProjectListResponse>>.Ok(
                data: result,
                message: ResponseMessageHelper.FormatMessage(ResponseMessages.SUCCESS, "Successfully retrieved member's projects")
            ));
        }

        [HttpGet("{memberId:guid}")]
        public async Task<IActionResult> GetProjectsByMemberId(
           Guid memberId,
           [FromQuery] ProjectMemberSearchRequest request,
           CancellationToken cancellationToken)
        {
            var result = await _projectMemberService.GetAllProjectsByMemberIdAsync(
                memberId,
                request,
                cancellationToken
            );

            return Ok(ResponseModel<PagedResult<AllProjectOfMememberResponse>>.Ok(
                data: result,
                message: ResponseMessageHelper.FormatMessage(ResponseMessages.SUCCESS, "Successfully retrieved member's projects")
            ));
        }

        /// <summary>
        /// Get Project Members By ProjectId
        /// </summary>
        [HttpGet("project/{projectId:guid}")]
        public async Task<IActionResult> GetProjectMembersByProjectId(
            Guid projectId,
            [FromQuery] ProjectMemberSearchRequestV2 request,
            CancellationToken cancellationToken)
        {
            var result = await _projectMemberService.GetProjectMemberByProjectId(
                projectId,
                request,
                cancellationToken
            );

            return Ok(ResponseModel<PagedResult<ProjectMemberResponseV2>>.Ok(
                data: result,
                message: ResponseMessageHelper.FormatMessage(ResponseMessages.SUCCESS, "Successfully retrieved project members")
            ));
        }

        [HttpGet("project/{projectId:guid}/members-with-role")]
        public async Task<IActionResult> GetProjectMembersWithRole(
           Guid projectId,
           CancellationToken cancellationToken)
        {
            var result = await _projectMemberService.GetProjectMembersWithRoleAsync(
                projectId,
                cancellationToken);

            return Ok(ResponseModel<List<ProjectMemberRoleResponse>>.Ok(
                data: result,
                message: ResponseMessageHelper.FormatMessage(
                    ResponseMessages.SUCCESS,
                    "Successfully retrieved project members with roles")
            ));
        }
        [HttpGet("charts/{projectId}")]
        public async Task<IActionResult> GetCharts(Guid projectId, CancellationToken ct)
        {
            var data = await _projectMemberService.GetProjectMemberChartsAsync(projectId, ct);
            return Ok(data);
        }
        [HttpPost]
        [HasPermission("PROJECT_INVITE_MEMBER")]
        public async Task<IActionResult> AddMember(
    [FromBody] ProjectMemberCreateRequest request,
    CancellationToken cancellationToken)
        {
            var result = await _projectMemberService.AddMemberAsync(request, cancellationToken);

            return Ok(ResponseModel<ProjectMemberResponseV2>.Ok(
                data: result,
                message: ResponseMessageHelper.FormatMessage(ResponseMessages.SUCCESS, "Member assigned to project successfully")
            ));
        }
        [HasPermission("PROJECT_KICK_MEMBER")]
        [HttpDelete("project/{projectId:guid}/member/{memberId:guid}")]
        public async Task<IActionResult> RemoveMember(
    Guid projectId,
    Guid memberId,
    CancellationToken cancellationToken)
        {
            await _projectMemberService.RemoveMemberAsync(projectId, memberId, cancellationToken);

            return Ok(ResponseModel<bool>.Ok(
                data: true,
                message: ResponseMessageHelper.FormatMessage(ResponseMessages.SUCCESS, "Member removed from project")
            ));
        }

    }
}
