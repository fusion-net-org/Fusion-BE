using Azure;
using Fusion.Repository.Bases.Responses;
using Fusion.Service.Commons.BaseResponses;
using Fusion.Service.IServices;
using Fusion.Service.Services;
using Fusion.Service.ViewModels.Companies.Requests;
using Fusion.Service.ViewModels.Companies.Responses;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Travelogue.Repository.Caching;

namespace Fusion.API.Controllers
{
    [Route("api/companymember")]
    [ApiController]
    public class CompanyMemberController : ControllerBase
    {
        private readonly ICompanyMemberService _companyMemberService;

        public CompanyMemberController(ICompanyMemberService companyMemberService)
        {
            _companyMemberService = companyMemberService;
        }

        [HttpPost("invite")]
        public async Task<IActionResult> InviteMember([FromBody] InviteMemberRequest request, CancellationToken token)
        {

            var emailClaim = User.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Email || c.Type == ClaimTypes.Email || c.Type == "email");
            var inviterEmail = emailClaim?.Value; if (inviterEmail == null)
            {
                return Unauthorized(ResponseModel<CompanyResponse>.Error(
                    statusCode: StatusCodes.Status401Unauthorized,
                    message: "Unauthorized: User identity not found"
                ));
            }

            var result = await _companyMemberService.InviteMemberToCompany(
                inviterEmail,
                request.InviteeMemberId,
                request.CompanyId,
                token
            );

            return Ok(ResponseModel<bool?>.Ok(
                 data: result,
                 message: ResponseMessageHelper.FormatMessage(ResponseMessages.SUCCESS, "Send Mail to Member ")));
        }

        [HttpGet("join")]
        public async Task<IActionResult> JoinCompany([FromQuery] string tokenConfirm, CancellationToken tokenCts)
        {
            var result = await _companyMemberService.JoinMemberToCompany(tokenConfirm, tokenCts);

            return Ok(ResponseModel<CompanyMemberResponse?>.Ok(
                data: result,
                message: ResponseMessageHelper.FormatMessage(ResponseMessages.SAVE_SUCCESS, "Member successfully join the company")));
        }
    }
}
