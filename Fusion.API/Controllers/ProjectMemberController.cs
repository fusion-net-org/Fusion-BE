using Fusion.Repository.Bases.Responses;
using Fusion.Service.Commons.BaseResponses;
using Fusion.Service.IServices;
using Fusion.Service.Services;
using Fusion.Service.ViewModels.ProjectMembers.Responses;
using Microsoft.AspNetCore.Http;
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

        [HttpGet("{companyId:guid}/{memberId:guid}")]
        public async Task<IActionResult> GetProjectsByMember(Guid companyId, Guid memberId)
        {
            var result = await _projectMemberService.GetProjectsByMemberAsync(companyId, memberId);
            return Ok(ResponseModel<MemberProjectListResponse>.Ok(
                data: result,
                message: ResponseMessageHelper.FormatMessage(ResponseMessages.SUCCESS, "Successfully retrieved member's projects")
            ));
        }
    }
}
