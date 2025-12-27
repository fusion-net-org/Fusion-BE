using Azure;
using Fusion.API.Auth;
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
using Fusion.Service.ViewModels.UserRole.Requests;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

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
        [HasPermission("MEMBER_INVITE")]
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

        [HttpGet("admin/paged")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseModel<PagedResult<CompanyMemberResponse>>))]
        public async Task<IActionResult> GetPaged([FromQuery] CompanyMemberPagedSearchAdminRequest request, CancellationToken token)
        {
            var emailClaim = User.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Email || c.Type == ClaimTypes.Email || c.Type == "email");
            var email = emailClaim?.Value; if (email == null)
            {
                return Unauthorized(ResponseModel<string>.Error(
                    statusCode: StatusCodes.Status401Unauthorized,
                    message: "Unauthorized: User identity not found"
                ));
            }

            var result = await _companyMemberService.GetPagedCompanyMemberAdminAsync(request, email, token);
            return Ok(ResponseModel<PagedResult<CompanyMemberResponse>>.Ok(
                data: result,
                message: "Get paged companies member successfully"));
        }


        [HttpPut("fired")]
        [HasPermission("MEMBER_REMOVE")]
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

        [HttpGet("status/{companyId}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseModel<List<CompanyMemberResponse>>))]
        public async Task<IActionResult> GetMembersByStatus(Guid companyId, [FromQuery] string status, CancellationToken token)
        {
            if (string.IsNullOrEmpty(status))
            {
                return BadRequest(ResponseModel<string>.Error(
                    statusCode: StatusCodes.Status400BadRequest,
                    message: "Status query parameter is required"
                ));
            }

            var result = await _companyMemberService.GetMembersByStatus(companyId, status, token);

            return Ok(ResponseModel<List<CompanyMemberResponse>>.Ok(
                data: result,
                message: $"Get members with status '{status}' successfully"
            ));
        }
        [HttpGet("summary/{companyId}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseModel<Dictionary<string, int>>))]
        public async Task<IActionResult> GetSummaryStatusByCompanyId(Guid companyId, CancellationToken token)
        {
            var result = await _companyMemberService.GetSummaryStatusByCompanyId(companyId, token);

            return Ok(ResponseModel<Dictionary<string, int>>.Ok(
                data: result,
                message: "Get summary status successfully"
            ));
        }

        [HttpDelete("{companyId:guid}/users/roles")]
        [HasPermission("MEMBER_REMOVE_ROLE")]
        public async Task<IActionResult> RemoveUserRolesFromCompany(
               Guid companyId,
               [FromBody] RemoveUserRoleFromCompanyRequest request,
               CancellationToken token)
        {
            var emailClaim = User.Claims.FirstOrDefault(
                c => c.Type == JwtRegisteredClaimNames.Email || c.Type == ClaimTypes.Email || c.Type == "email"
            );
            var requesterEmail = emailClaim?.Value;

            if (requesterEmail == null)
            {
                return Unauthorized(ResponseModel<string>.Error(
                    statusCode: StatusCodes.Status401Unauthorized,
                    message: "Unauthorized: User identity not found"
                ));
            }

            var result = await _companyMemberService.RemoveRoleForMemberInCompany(
                companyId,
                request.RoleIds,
                request.UserId,
                requesterEmail,
                token
            );

            return Ok(result);
        }


        [HttpPost("{companyId:guid}/users/roles")]
        [HasPermission("MEMBER_ASSIGN_ROLE")]
        public async Task<IActionResult> AddUserRolesToCompany(Guid companyId, [FromBody] AddUserRoleToCompanyRequest request, CancellationToken token)
        {
            var emailClaim = User.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Email || c.Type == ClaimTypes.Email || c.Type == "email");
            var inviterEmail = emailClaim?.Value; if (inviterEmail == null)
            {
                return Unauthorized(ResponseModel<string>.Error(
                    statusCode: StatusCodes.Status401Unauthorized,
                    message: "Unauthorized: User identity not found"
                ));
            }


            var result = await _companyMemberService.AddRoleForMemberInCompany(
                companyId,
                request.RoleIds,
                request.UserId,
                inviterEmail,
                token
            );

            return Ok(result);
        }

        //[HttpDelete("{companyId:guid}/users/roles")]
        //public async Task<IActionResult> RemoveUserRolesFromCompany(
        //    Guid companyId,
        //    [FromBody] RemoveUserRoleFromCompanyRequest request,
        //    CancellationToken token)
        //{
        //    var emailClaim = User.Claims.FirstOrDefault(
        //        c => c.Type == JwtRegisteredClaimNames.Email || c.Type == ClaimTypes.Email || c.Type == "email"
        //    );
        //    var requesterEmail = emailClaim?.Value;

        //    if (requesterEmail == null)
        //    {
        //        return Unauthorized(ResponseModel<string>.Error(
        //            statusCode: StatusCodes.Status401Unauthorized,
        //            message: "Unauthorized: User identity not found"
        //        ));
        //    }

        //    var result = await _companyMemberService.RemoveRoleForMemberInCompany(
        //        companyId,
        //        request.RoleIds,
        //        request.UserId,
        //        requesterEmail,
        //        token
        //    );

        //    return Ok(result);
        //}

        /// <summary>
        /// Get company member detail by companyId + userId
        /// </summary>
        [HttpGet("member/{companyId:guid}/{userId:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseModel<CompanyMemberResponse>))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetCompanyMemberByCompanyAndUser(Guid companyId, Guid userId, CancellationToken token = default)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var tokenUserId))
            {
                return Unauthorized(ResponseModel<string>.Error(
                    StatusCodes.Status401Unauthorized,
                    "Don't find token!"));
            }

            var result = await _companyMemberService.GetCompanyMemberByCompanyIdAndUserIdAsync(companyId, userId, token);

            if (result == null)
            {
                return NotFound(ResponseModel<string>.Error(
                    StatusCodes.Status404NotFound,
                    "Company member not found"));
            }

            return Ok(ResponseModel<CompanyMemberResponse>.Ok(
                data: result,
                message: ResponseMessageHelper.FormatMessage(ResponseMessages.GET_SUCCESS, "company member by companyId & userId")));
        }
        // 1. GET company member by userId — search + filter + date range + paging
        [HttpGet("by-user")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseModel<PagedResult<CompanyMemberResponseV2>>))]
        public async Task<IActionResult> GetCompanyMemberByUserId(
            [FromQuery] CompanyMemberPagedRequest request,
            CancellationToken token)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized(ResponseModel<string>.Error(
                    StatusCodes.Status401Unauthorized,
                    "User is not authenticated"));
            }

            var result = await _companyMemberService.GetCompanyMemberByUserIdAsync(userId, request, token);

            return Ok(ResponseModel<PagedResult<CompanyMemberResponseV2>>.Ok(
                data: result,
                message: "Get company members by userId successfully"));
        }
        [HttpPut("{memberId:long}/accept")]
        public async Task<IActionResult> AcceptJoinMember(long memberId, CancellationToken token)
        {
            try
            {
                var result = await _companyMemberService.AcceptJoinMemberById(memberId, token);

                return Ok(ResponseModel<CompanyMemberResponse?>.Ok(
                    data: result,
                    message: ResponseMessageHelper.FormatMessage("Member successfully accepted to join the company")
                ));
            }
            catch (Exception ex)
            {
                return BadRequest(ResponseModel<string>.Error(
                    statusCode: StatusCodes.Status400BadRequest,
                    message: ex.Message
                ));
            }
        }

        [HttpPut("{memberId:long}/reject")]
        public async Task<IActionResult> RejectJoinMember(long memberId, CancellationToken token)
        {
            try
            {
                var result = await _companyMemberService.RejectJoinMemberById(memberId, token);

                return Ok(ResponseModel<CompanyMemberResponse?>.Ok(
                    data: result,
                    message: ResponseMessageHelper.FormatMessage("Member rejected to join the company")
                ));
            }
            catch (Exception ex)
            {
                return BadRequest(ResponseModel<string>.Error(
                    statusCode: StatusCodes.Status400BadRequest,
                    message: ex.Message
                ));
            }
        }


    }
}
