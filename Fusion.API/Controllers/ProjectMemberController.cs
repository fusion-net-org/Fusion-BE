using Fusion.Repository.Bases.Page;
using Fusion.Repository.Bases.Page.ProjectMember;
using Fusion.Repository.Bases.Responses;
using Fusion.Service.Commons.BaseResponses;
using Fusion.Service.IServices;
using Fusion.Service.ViewModels.ProjectMembers.Responses;
using Microsoft.AspNetCore.Mvc;

namespace Fusion.API.Controllers
{
    [Route("api/projectmember")]
    [ApiController]
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
    }
}
