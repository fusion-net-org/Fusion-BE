using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Fusion.Repository.Bases.Exceptions;
using Fusion.Repository.Bases.Page;
using Fusion.Repository.Bases.Page.Company;
using Fusion.Repository.Bases.Page.Partner;
using Fusion.Repository.Bases.Responses;
using Fusion.Repository.ViewModels.Companies;
using Fusion.Service.Commons.BaseResponses;
using Fusion.Service.IServices;
using Fusion.Service.ViewModels.Companies.Requests;
using Fusion.Service.ViewModels.Companies.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

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
        [HttpGet("status")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseModel<List<CompanyFriendshipResponse>>))]
        public async Task<IActionResult> GetPartnerByStatus([FromQuery] Guid companyId, [FromQuery] string status, [FromQuery] PagedRequest request)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized(ResponseModel<string>.Error(
                    StatusCodes.Status401Unauthorized,
                    "Don't find token!"));
            }
            var result = await _companyFriendshipService.GetCompanyFriendshipByStatus(userId, companyId, status, request);


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

            var result = await _companyFriendshipService.InviteCompanyFriendship(inviteCompanyRequest.CompanyAID, inviteCompanyRequest.CompanyBID, userId, inviteCompanyRequest.Note);

            return Ok(ResponseModel<CompanyFriendshipResponse>.Ok(
                data: result,
                message: ResponseMessageHelper.FormatMessage(ResponseMessages.INVITE_SUCCESS, $"company friendship")));
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
        public async Task<IActionResult> GetCompanyFriendshipStatusSummary([FromQuery] Guid? companyId, CancellationToken cancellationToken)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized(ResponseModel<string>.Error(
                    StatusCodes.Status401Unauthorized,
                    "Don't find token!"));
            }

            var summary = await _companyFriendshipService.GetCompanyFriendshipStatusSummary(userId, companyId);

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
        /// <summary>
        /// Get paged & searchable company friendships by a specific company ID
        /// </summary>
        [HttpGet("by-company/v2")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseModel<PagedResult<CompanyFriendshipResponse>>))]
        public async Task<IActionResult> GetCompanyFriendshipByCompanyIDVersion2(
            [FromQuery] Guid companyId,
            [FromQuery] CompanyFriendshipSearchRequest request,
            CancellationToken cancellationToken = default)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized(ResponseModel<string>.Error(
                    StatusCodes.Status401Unauthorized,
                    "Don't find token!"));
            }
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            var result = await _companyFriendshipService
                .GetCompanyFriendshipByCompanyIDVersion2(userId, companyId, request, cancellationToken);

            stopwatch.Stop();
            var elapsedMs = stopwatch.ElapsedMilliseconds;
            Console.WriteLine($"[DEBUG] GetCompanyFriendshipByCompanyIDVersion2 took {elapsedMs} ms");

            return Ok(ResponseModel<PagedResult<CompanyFriendshipResponse>>.Ok(
                data: result,
                message: ResponseMessageHelper.FormatMessage(ResponseMessages.GET_SUCCESS, "company friendships by company ID (v2)")));
        }

        /// <summary>
        /// Get friendship status between two companies
        /// </summary>
        [HttpGet("between/{companyAId:guid}/{companyBId:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseModel<CompanyFriendshipResponse>))]
        public async Task<IActionResult> GetFriendshipBetweenCompanies(
            Guid companyAId,
            Guid companyBId,
            [FromQuery] long? friendshipId = null,
            CancellationToken token = default)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized(ResponseModel<string>.Error(
                    StatusCodes.Status401Unauthorized,
                    "Don't find token!"));
            }

            var result = await _companyFriendshipService
                .GetCompanyFriendshipBetweenCompaniesAsync(companyAId, companyBId, friendshipId, token);

            if (result == null)
            {
                return NotFound(ResponseModel<string>.Error(
                    StatusCodes.Status404NotFound,
                    $"No friendship found between CompanyA ({companyAId}) and CompanyB ({companyBId})."));
            }

            return Ok(ResponseModel<CompanyFriendshipResponse>.Ok(
                data: result,
                message: ResponseMessageHelper.FormatMessage(ResponseMessages.GET_SUCCESS, "company friendship between companies")));
        }


        /// <summary>
        /// Delete (Unfriend) company partnership
        /// </summary>
        [HttpDelete("delete/{id:long}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseModel<CompanyFriendshipResponse>))]
        public async Task<IActionResult> DeleteCompanyFriendship(long id)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized(ResponseModel<string>.Error(
                    StatusCodes.Status401Unauthorized,
                    "Don't find token!"));
            }

            var result = await _companyFriendshipService.DeleteCompanyFriendship(id, userId);

            return Ok(ResponseModel<CompanyFriendshipResponse>.Ok(
                data: result,
                message: ResponseMessageHelper.FormatMessage(ResponseMessages.DELETE_SUCCESS, "company friendship")));
        }

        /***************************************************Mobile*********************************************/

        [HttpGet("paged/{companyId:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseModel<PagedResult<PartnerResponse>>))]
        public async Task<IActionResult> GetCompanyFriendshipByCompanyID(Guid companyId, [FromQuery] CompanyFriendshipSearchRequest request, CancellationToken token = default)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized(ResponseModel<string>.Error(
                    StatusCodes.Status401Unauthorized,
                    "Don't find token!"));
            }

            var result = await _companyFriendshipService.GetCompanyFriendshipByCompanyID(userId, companyId, request, token);

            return Ok(ResponseModel<PagedResult<PartnerResponse>>.Ok(
                data: result,
                message: ResponseMessageHelper.FormatMessage(ResponseMessages.GET_SUCCESS, "company friendships by company ID")));
        }

        [HttpGet("status-summary/{companyId:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseModel<object>))]
        public async Task<IActionResult> GetCompanyFriendshipStatusSummary(Guid companyId, CancellationToken cancellationToken)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized(ResponseModel<string>.Error(
                    StatusCodes.Status401Unauthorized,
                    "Don't find token!"));
            }

            var summary = await _companyFriendshipService.GetCompanyFriendshipStatusSummary(userId, companyId);

            return Ok(ResponseModel<object>.Ok(
                data: summary,
                message: ResponseMessageHelper.FormatMessage(ResponseMessages.GET_SUCCESS, "friendship status summary")));
        }

        [HttpGet("task-stats")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseModel<CompanyTaskStatsRequest>))]
        public async Task<IActionResult> GetPartnerTaskStats([FromQuery] CompanyTaskStatsRequest request, CancellationToken token)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized(ResponseModel<string>.Error(
                    StatusCodes.Status401Unauthorized,
                    "Don't find token!"));
            }

            var result = await _companyService.GetTaskStatsAsync(
                request.PartnerCompanyId,
                request.MyCompanyId,
                userId,
                token
            );

            return Ok(ResponseModel<CompanyTaskStatsResponse>.Ok(
                           data: result,
                           message: ResponseMessageHelper.FormatMessage(ResponseMessages.GET_SUCCESS, "friendship task status")));
        }

        [HttpGet("all-partners/{companyId:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseModel<List<CompanyResponseVersion2>>))]
        public async Task<IActionResult> GetAllPartnersOfCompany(
              Guid companyId,
              [FromQuery] string? companyName,
              CancellationToken cancellationToken)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized(ResponseModel<string>.Error(
                    StatusCodes.Status401Unauthorized,
                    "Don't find token!"));
            }

            var partners = await _companyFriendshipService.GetAllPartnersOfCompanyAsync(companyId, companyName, cancellationToken);

            return Ok(ResponseModel<List<CompanyResponseVersion2>>.Ok(
                data: partners,
                message: partners.Count > 0
                    ? $"Found {partners.Count} partners for the company."
                    : "No partners found for this company."));
        }



    }

}

