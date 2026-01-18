using Fusion.Repository.Bases.Page;
using Fusion.Repository.Bases.Responses;
using Fusion.Repository.IRepositories;
using Fusion.Service.Commons.BaseResponses;
using Fusion.Service.IServices;
using Fusion.Service.ViewModels.Tickets.Responses;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Fusion.API.Controllers
{
    [Route("api/ticket-history")]
    [ApiController]
    public class TicketHistoryController : ControllerBase
    {
        private readonly ITicketHistoryService _ticketHistoryService;

        public TicketHistoryController(ITicketHistoryService ticketHistoryService)
        {
            _ticketHistoryService = ticketHistoryService;
        }

        [HttpGet("paged")]
        [ProducesResponseType(
    StatusCodes.Status200OK,
    Type = typeof(ResponseModel<PagedResult<TicketHistoryResponse>>)
)]
        public async Task<IActionResult> GetPagedTicketHistories(
    [FromQuery] TicketHistoryPagedRequest request,
    CancellationToken cancellationToken)
        {
            var emailClaim = User.Claims.FirstOrDefault(c =>
                c.Type == JwtRegisteredClaimNames.Email ||
                c.Type == ClaimTypes.Email ||
                c.Type == "email");

            var email = emailClaim?.Value;
            if (email == null)
            {
                return Unauthorized(ResponseModel<PagedResult<TicketHistoryResponse>>.Error(
                    statusCode: StatusCodes.Status401Unauthorized,
                    message: "Unauthorized: User identity not found"
                ));
            }

            try
            {
                var result = await _ticketHistoryService
                    .GetTicketHistoryByTicketIdAsync(request, cancellationToken);

                return Ok(ResponseModel<PagedResult<TicketHistoryResponse>>.Ok(
                    data: result,
                    message: "Get paged ticket histories successfully"
                ));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ResponseModel<PagedResult<TicketHistoryResponse>>.Error(
                    statusCode: StatusCodes.Status404NotFound,
                    message: ex.Message
                ));
            }
        }

    }
}
