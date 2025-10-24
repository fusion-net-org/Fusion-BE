using Azure;
using Fusion.Repository.Bases.Page;
using Fusion.Repository.Bases.Page.Company;
using Fusion.Repository.Bases.Page.Company_Member;
using Fusion.Repository.Bases.Responses;
using Fusion.Repository.Entities;
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
        public async Task<IActionResult> InviteMemberToCompany([FromBody] InviteMemberRequest request, CancellationToken token)
        {

            var emailClaim = User.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Email || c.Type == ClaimTypes.Email || c.Type == "email");
            var inviterEmail = emailClaim?.Value; if (inviterEmail == null)
            {
                return Unauthorized(ResponseModel<string>.Error(
                    statusCode: StatusCodes.Status401Unauthorized,
                    message: "Unauthorized: User identity not found"
                ));
            }

            var result = await _companyMemberService.InviteMemberToCompany(
                inviterEmail,
                request.InviteeMemberMail,
                request.CompanyId,
                token
            );

            return Ok(ResponseModel<CompanyMemberResponse?>.Ok(
                 data: result,
                 message: ResponseMessageHelper.FormatMessage(ResponseMessages.SUCCESS, "Added Member to Company Successfully")));
        }

        [HttpGet("paged/{companyId}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseModel<PagedResult<CompanyMemberResponse>>))]
        public async Task<IActionResult> GetPagedByCompanyId(Guid companyId, [FromQuery] CompanyMemberPagedSearchRequest request,  CancellationToken cancellationToken)
        {
            var emailClaim = User.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Email || c.Type == ClaimTypes.Email || c.Type == "email");
            var email = emailClaim?.Value; if (email == null)
            {
                return Unauthorized(ResponseModel<string>.Error(
                    statusCode: StatusCodes.Status401Unauthorized,
                    message: "Unauthorized: User identity not found"
                ));
            }

            var result = await _companyMemberService.GetPagedCompanyMemberByCompanyIdAsync(companyId, email, request, cancellationToken);
            return Ok(ResponseModel<PagedResult<CompanyMemberResponse>>.Ok(
                data: result,
                message: "Get paged companies member successfully"));
        }

        [HttpGet("paged")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseModel<PagedResult<CompanyMemberResponse>>))]
        public async Task<IActionResult> GetPaged([FromQuery] CompanyMemberPagedSearchAdminRequest request, CancellationToken token)
        {
            //var emailClaim = User.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Email || c.Type == ClaimTypes.Email || c.Type == "email");
            //var email = emailClaim?.Value; if (email == null)
            //{
            //    return Unauthorized(ResponseModel<string>.Error(
            //        statusCode: StatusCodes.Status401Unauthorized,
            //        message: "Unauthorized: User identity not found"
            //    ));
            //}

            var result = await _companyMemberService.GetPagedCompanyMemberAsync(request, token);
            return Ok(ResponseModel<PagedResult<CompanyMemberResponse>>.Ok(
                data: result,
                message: "Get paged companies member successfully"));
        }


        [HttpPost("fired")]
        public async Task<IActionResult> FiredMemberFromCompany([FromBody] FiredMemberRequest request, CancellationToken token)
        {

            var emailClaim = User.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Email || c.Type == ClaimTypes.Email || c.Type == "email");
            var terminatorEmail = emailClaim?.Value; if (terminatorEmail == null)
            {
                return Unauthorized(ResponseModel<string>.Error(
                    statusCode: StatusCodes.Status401Unauthorized,
                    message: "Unauthorized: User identity not found"
                ));
            }

            var result = await _companyMemberService.FiredMemberFromCompany(
               terminatorEmail,
               request.FiredMemberMail,
               request.Reason,
               request.CompanyId,
               token
            );

            return Ok(ResponseModel<CompanyMemberResponse?>.Ok(
                 data: result,
                 message: ResponseMessageHelper.FormatMessage("Fired Member from Company Successfully")));
        }

        [HttpDelete("{removeId:Guid}")]
        public async Task<IActionResult> RemoveMemberFromCompany(Guid removeId, Guid companyId, CancellationToken token)
        {

            var emailClaim = User.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Email || c.Type == ClaimTypes.Email || c.Type == "email");
            var terminatorEmail = emailClaim?.Value; if (terminatorEmail == null)
            {
                return Unauthorized(ResponseModel<string>.Error(
                    statusCode: StatusCodes.Status401Unauthorized,
                    message: "Unauthorized: User identity not found"
                ));
            }

            var result = await _companyMemberService.RemoveMemberFromCompany(
               terminatorEmail,
               removeId,
               companyId,
               token
            );

            return Ok(ResponseModel<CompanyMemberResponse?>.Ok(
                 data: result,
                 message: ResponseMessageHelper.FormatMessage("Remove Member from Company Successfully")));
        }

        [HttpGet("accept")]
        public async Task<IActionResult> AcceptJoinFromCompany([FromQuery] string tokenConfirm, CancellationToken tokenCts)
        {
            var result = await _companyMemberService.AcceptJoinMemberToCompany(tokenConfirm, tokenCts);

            return Ok(ResponseModel<CompanyMemberResponse?>.Ok(
                data: result,
                message: ResponseMessageHelper.FormatMessage("Member successfully join the company")));
        }

        [HttpGet("reject")]
        public async Task<IActionResult> RejectJoinFromCompany([FromQuery] string tokenConfirm, CancellationToken tokenCts)
        {
            var result = await _companyMemberService.RejectJoinMemberToCompany(tokenConfirm, tokenCts);

            return Ok(ResponseModel<CompanyMemberResponse?>.Ok(
                data: result,
                message: ResponseMessageHelper.FormatMessage("Member reject to join the company")));
        }
    }
}
