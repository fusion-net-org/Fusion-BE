using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Fusion.Repository.Bases.Exceptions;
using Fusion.Repository.Bases.Responses;
using Fusion.Service.Commons.BaseResponses;
using Fusion.Service.IServices;
using Fusion.Service.ViewModels.Users.Requests;
using Fusion.Service.ViewModels.Users.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Fusion.API.Controllers
{
    [Route("api/[controller]")]
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
        /// Invite company partnership
        /// </summary>
        [HttpPost("invite")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseModel<CompanyFriendshipResponse>))]
        public async Task<IActionResult> InviteCompanyFriendship([FromBody] InviteCompanyRequest inviteCompanyRequest)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized(ResponseModel<string>.ErrorResponseModel(
                    StatusCodes.Status401Unauthorized,
                    "Don't find token!"));
            }

            var CompanyAID = await _companyService.GetCompanyIdByUserId(userId);

            var result = await _companyFriendshipService.InviteCompanyFriendship((Guid)CompanyAID, inviteCompanyRequest.CompanyBID, userId);

            return Ok(ResponseModel<CompanyFriendshipResponse>.OkResponseModel(
                data: result,
                message: ResponseMessageHelper.FormatMessage(ResponseMessages.INVITE_SUCESS, $"company friendship")));
        }
    }
}

