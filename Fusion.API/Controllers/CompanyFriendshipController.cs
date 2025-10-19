using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Fusion.Repository.Bases.Exceptions;
using Fusion.Repository.Bases.Page;
using Fusion.Repository.Bases.Page.Partner;
using Fusion.Repository.Bases.Responses;
using Fusion.Service.Commons.BaseResponses;
using Fusion.Service.IServices;
using Fusion.Service.ViewModels.Companies.Requests;
using Fusion.Service.ViewModels.Companies.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Fusion.API.Controllers
{
    [Route("api/partners")]
    [ApiController]
    [Authorize]
    public class CompanyFriendshipController : ControllerBase
    {
        private readonly ICompanyFriendshipService _companyFriendshipService;
        private readonly ICompanyService _companyService;
        public CompanyFriendshipController(ICompanyFriendshipService companyFriendshipService, ICompanyService companyService)
        {
            _companyFriendshipService = companyFriendshipService;
            _companyService = companyService;
        }

        /// <summary>
        /// Get company partnerships by status
        /// </summary>
        [HttpGet("{status}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseModel<List<CompanyFriendshipResponse>>))]
        public async Task<IActionResult> GetPartnerByStatus(string status, [FromQuery] PagedRequest request)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized(ResponseModel<string>.Error(
                    StatusCodes.Status401Unauthorized,
                    "Don't find token!"));
            }
            var result = await _companyFriendshipService.GetCompanyFriendshipByStatus(userId,status, request);


            return Ok(ResponseModel<PagedResult<CompanyFriendshipResponse>>.Ok(
                data: result,
                message: ResponseMessageHelper.FormatMessage(ResponseMessages.GET_SUCCESS, "company friendships by status")));
        }

        /// <summary>
        /// Get company friendships by Owner User ID
        /// <summary></summary>
        [HttpGet("owner")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseModel<PagedResult<CompanyFriendshipResponse>>))]
        public async Task<IActionResult> GetCompanyFriendshipByOwnerUserID([FromQuery] CompanyFriendshipSearchRequest request)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized(ResponseModel<string>.Error(
                    StatusCodes.Status401Unauthorized,
                    "Don't find token!"));
            }

            var result = await _companyFriendshipService.GetCompanyFriendshipByOwnerUserID(userId, request);

            return Ok(ResponseModel<PagedResult<CompanyFriendshipResponse>>.Ok(
                data: result,
                message: ResponseMessageHelper.FormatMessage(ResponseMessages.GET_SUCCESS, "company friendships by owner user")));
        }


        /// <summary>
        /// Invite company partnership
        /// </summary>
        [HttpPost("invite")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseModel<CompanyFriendshipResponse>))]
        public async Task<IActionResult> InviteCompanyFriendship([FromBody] InviteCompanyRequest inviteCompanyRequest)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized(ResponseModel<string>.Error(
                    StatusCodes.Status401Unauthorized,
                    "Don't find token!"));
            }

            //var CompanyAID = await _companyService.GetCompanyIdByUserId(userId);

            var result = await _companyFriendshipService.InviteCompanyFriendship(inviteCompanyRequest.CompanyAID, inviteCompanyRequest.CompanyBID, userId,inviteCompanyRequest.Note);

            return Ok(ResponseModel<CompanyFriendshipResponse>.Ok(
                data: result,
                message: ResponseMessageHelper.FormatMessage(ResponseMessages.INVITE_SUCESS, $"company friendship")));
        }


        /// <summary>
        /// Accept company partnership
        /// </summary>
        [HttpGet("accept/{id:long}")]
        [AllowAnonymous] // để công ty B có thể click link trong mail
        public async Task<IActionResult> AcceptCompanyFriendship(long id)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null)
                return Unauthorized();

            var userId = Guid.Parse(userIdClaim);

            var result = await _companyFriendshipService.AcceptCompanyFriendship(id, userId);
            return Ok(ResponseModel<CompanyFriendshipResponse>.Ok(
                data: result,
                message: "Friendship accepted successfully"));
        }

        /// <summary>
        /// Cancel company partnership
        /// </summary>
        [HttpGet("cancel/{id:long}")]
        [AllowAnonymous]
        public async Task<IActionResult> CancelCompanyFriendship(long id)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null)
                return Unauthorized();

            var userId = Guid.Parse(userIdClaim);
            var result = await _companyFriendshipService.CancelCompanyFriendship(id, userId);
            return Ok(ResponseModel<CompanyFriendshipResponse>.Ok(
                data: result,
                message: "Friendship rejected successfully"));
        }

        /// <summary>
        /// Get count of company friendships by status (Pending, Active, Inactive)
        /// </summary>
        [HttpGet("status-summary")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseModel<object>))]
        public async Task<IActionResult> GetCompanyFriendshipStatusSummary(CancellationToken cancellationToken)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized(ResponseModel<string>.Error(
                    StatusCodes.Status401Unauthorized,
                    "Don't find token!"));
            }

            var summary = await _companyFriendshipService.GetCompanyFriendshipStatusSummary(userId);

            return Ok(ResponseModel<object>.Ok(
                data: summary,
                message: ResponseMessageHelper.FormatMessage(ResponseMessages.GET_SUCCESS, "friendship status summary")));
        }

        /// <summary>
        /// Get list of partners by a specific company ID
        /// </summary>
        [HttpGet("by-company/{companyId:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseModel<List<CompanyFriendshipResponse>>))]
        public async Task<IActionResult> GetCompanyFriendshipByCompanyID(Guid companyId)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized(ResponseModel<string>.Error(
                    StatusCodes.Status401Unauthorized,
                    "Don't find token!"));
            }

            var result = await _companyFriendshipService.GetCompanyFriendshipByCompanyID(userId, companyId);

            return Ok(ResponseModel<List<CompanyFriendshipResponse>>.Ok(
                data: result,
                message: ResponseMessageHelper.FormatMessage(ResponseMessages.GET_SUCCESS, "company friendships by company ID")));
        }
    }
}

