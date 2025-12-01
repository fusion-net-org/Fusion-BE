using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Fusion.Repository.Bases.Page;
using Fusion.Repository.Bases.Page.Ticket;
using Fusion.Repository.Bases.Responses;
using Fusion.Repository.Entities;
using Fusion.Service.Commons.BaseResponses;
using Fusion.Service.IServices;
using Fusion.Service.Services;
using Fusion.Service.ViewModels.Tickets.Requests;
using Fusion.Service.ViewModels.Tickets.Responses;
using Microsoft.AspNetCore.Mvc;

namespace Fusion.API.Controllers
{
    [Route("api/ticket")]
    [ApiController]
    public class TicketController : ControllerBase
    {
        private readonly ITicketService _ticketService;

        public TicketController(ITicketService ticketService)
        {
            _ticketService = ticketService;
        }

        [HttpGet("paged")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseModel<TicketPagedResponse>))]
        public async Task<IActionResult> GetPaged(
         [FromQuery] TicketPagedSearchRequest request,
         CancellationToken cancellationToken)
        {
            var result = await _ticketService.GetPageTicketshAsync(request, cancellationToken);

            return Ok(ResponseModel<TicketPagedResponse>.Ok(
                data: result,
                message: "Get paged tickets successfully"));
        }

        [HttpGet("paged/admin")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseModel<TicketPagedResponse>))]
        public async Task<IActionResult> GetPagedTicketByAdmin(
         [FromQuery] TicketPagedSearchRequest request,
         CancellationToken cancellationToken)
        {

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized(new { Message = "Invalid or missing token" });
            }

            var result = await _ticketService.GetPageTicketAdminAsync(request, userId, cancellationToken);

            return Ok(ResponseModel<TicketPagedResponse>.Ok(
                data: result,
                message: "Get paged tickets successfully"));
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(ResponseModel<TicketResponse>))]
        public async Task<IActionResult> CreateTicket(TicketRequest request, CancellationToken cancellationToken)
        {
            var emailClaim = User.Claims.FirstOrDefault(c =>
                c.Type == JwtRegisteredClaimNames.Email ||
                c.Type == ClaimTypes.Email ||
                c.Type == "email");

            var email = emailClaim?.Value;
            if (email == null)
            {
                return Unauthorized(ResponseModel<TicketResponse>.Error(
                    statusCode: StatusCodes.Status401Unauthorized,
                    message: "Unauthorized: User identity not found"
                ));
            }
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized(ResponseModel<string>.Error(
                    StatusCodes.Status401Unauthorized,
                    "Don't find token!"));
            }
            request.SubmittedBy = userId;

            var result = await _ticketService.CreateTicketAsync(request, cancellationToken);

            return Ok(ResponseModel<TicketResponse>.Ok(
                data: result,
                message: ResponseMessageHelper.FormatMessage(ResponseMessages.CREATE_SUCCESS, "ticket")));
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseModel<TicketResponse>))]
        public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
        {
            var result = await _ticketService.GetTicketByIdAsync(id, cancellationToken);
            return Ok(ResponseModel<TicketResponse>.Ok(
                data: result,
                message: "Get ticket by id successfully"));
        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseModel<TicketResponse>))]
        public async Task<IActionResult> Update(Guid id, TicketRequest request, CancellationToken cancellationToken)
        {
            var emailClaim = User.Claims.FirstOrDefault(c =>
                c.Type == JwtRegisteredClaimNames.Email ||
                c.Type == ClaimTypes.Email ||
                c.Type == "email");

            var email = emailClaim?.Value;
            if (email == null)
            {
                return Unauthorized(ResponseModel<TicketResponse>.Error(
                    statusCode: StatusCodes.Status401Unauthorized,
                    message: "Unauthorized: User identity not found"
                ));
            }

            var result = await _ticketService.UpdateTicketAsync(request, id, cancellationToken);
            return Ok(ResponseModel<TicketResponse>.Ok(
                data: result,
                message: "Update ticket successfully"));
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> Delete(Guid id, [FromQuery] string reason, CancellationToken cancellationToken)
        {
            var emailClaim = User.Claims.FirstOrDefault(c =>
                c.Type == JwtRegisteredClaimNames.Email ||
                c.Type == ClaimTypes.Email ||
                c.Type == "email");

            var email = emailClaim?.Value;
            if (email == null)
            {
                return Unauthorized(ResponseModel<TicketResponse>.Error(
                    statusCode: StatusCodes.Status401Unauthorized,
                    message: "Unauthorized: User identity not found"
                ));
            }

            try
            {
                var result = await _ticketService.DeleteTicketAsync(id, reason, cancellationToken);
                return Ok(ResponseModel<bool>.Ok(
                    data: result ?? false,
                    message: "Delete ticket successfully"));
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(
                    StatusCodes.Status403Forbidden,
                    ResponseModel<bool>.Error(
                        statusCode: StatusCodes.Status403Forbidden,
                        message: ex.Message
                    )
                );
            }

            catch (KeyNotFoundException ex)
            {
                return NotFound(ResponseModel<bool>.Error(
                    statusCode: StatusCodes.Status404NotFound,
                    message: ex.Message
                ));
            }
        }

        [HttpPut("{id:guid}/restore")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> Restore(Guid id, CancellationToken cancellationToken)
        {
            var emailClaim = User.Claims.FirstOrDefault(c =>
                c.Type == JwtRegisteredClaimNames.Email ||
                c.Type == ClaimTypes.Email ||
                c.Type == "email");

            var email = emailClaim?.Value;
            if (email == null)
            {
                return Unauthorized(ResponseModel<TicketResponse>.Error(
                    statusCode: StatusCodes.Status401Unauthorized,
                    message: "Unauthorized: User identity not found"
                ));
            }

            try
            {
                var result = await _ticketService.RestoreTicketAsync(id, cancellationToken);
                return Ok(ResponseModel<bool>.Ok(
                    data: result ?? false,
                    message: "Restore ticket successfully"));
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(
                    StatusCodes.Status403Forbidden,
                    ResponseModel<bool>.Error(
                        statusCode: StatusCodes.Status403Forbidden,
                        message: ex.Message
                    )
                );
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ResponseModel<bool>.Error(
                    statusCode: StatusCodes.Status400BadRequest,
                    message: ex.Message
                ));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ResponseModel<bool>.Error(
                    statusCode: StatusCodes.Status404NotFound,
                    message: ex.Message
                ));
            }
        }


        [HttpGet("by-project")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseModel<PagedResult<TicketResponse>>))]
        public async Task<IActionResult> GetTicketsByProject([FromQuery] TicketByProjectPagedRequest request, CancellationToken cancellationToken)
        {
            var emailClaim = User.Claims.FirstOrDefault(c =>
                c.Type == JwtRegisteredClaimNames.Email ||
                c.Type == ClaimTypes.Email ||
                c.Type == "email");

            var email = emailClaim?.Value;
            if (email == null)
            {
                return Unauthorized(ResponseModel<PagedResult<TicketResponse>>.Error(
                    statusCode: StatusCodes.Status401Unauthorized,
                    message: "Unauthorized: User identity not found"
                ));
            }

            var result = await _ticketService.GetTicketsByProjectIdAsync(request, cancellationToken);

            return Ok(ResponseModel<PagedResult<TicketResponse>>.Ok(
                data: result,
                message: "Get tickets by project successfully"));
        }
        [HttpGet("dashboard")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseModel<TicketDashboardResponse>))]
        public async Task<IActionResult> GetDashboard([FromQuery] Guid projectId, CancellationToken cancellationToken)
        {
            var dashboard = await _ticketService.GetTicketDashboardAsync(projectId, cancellationToken);
            return Ok(ResponseModel<TicketDashboardResponse>.Ok(
                data: dashboard,
                message: "Get ticket dashboard successfully"));
        }
        [HttpGet("status-count")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseModel<TicketStatusCountResponse>))]
        public async Task<IActionResult> GetTicketStatusCount(
         [FromQuery] Guid? projectId,
         [FromQuery] Guid? companyRequestId,
         [FromQuery] Guid? companyExecutorId,
         CancellationToken cancellationToken)
        {
            var emailClaim = User.Claims.FirstOrDefault(c =>
                c.Type == JwtRegisteredClaimNames.Email ||
                c.Type == ClaimTypes.Email ||
                c.Type == "email");

            var email = emailClaim?.Value;
            if (email == null)
            {
                return Unauthorized(ResponseModel<TicketStatusCountResponse>.Error(
                    statusCode: StatusCodes.Status401Unauthorized,
                    message: "Unauthorized: User identity not found"
                ));
            }

            var result = await _ticketService.GetTicketStatusCountAsync(projectId, companyRequestId, companyExecutorId, cancellationToken);

            return Ok(ResponseModel<TicketStatusCountResponse>.Ok(
                data: result,
                message: "Get ticket status count successfully"
            ));
        }

        [HttpPut("{id:guid}/accept")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseModel<TicketResponse>))]
        public async Task<IActionResult> AcceptTicket(Guid id, CancellationToken cancellationToken)
        {
            var emailClaim = User.Claims.FirstOrDefault(c =>
                c.Type == JwtRegisteredClaimNames.Email ||
                c.Type == ClaimTypes.Email ||
                c.Type == "email");

            var email = emailClaim?.Value;
            if (email == null)
            {
                return Unauthorized(ResponseModel<TicketResponse>.Error(
                    statusCode: StatusCodes.Status401Unauthorized,
                    message: "Unauthorized: User identity not found"
                ));
            }

            try
            {
                var result = await _ticketService.AcceptTicketAsync(id, cancellationToken);
                return Ok(ResponseModel<TicketResponse>.Ok(
                    data: result,
                    message: "Ticket accepted successfully"));
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(
                    StatusCodes.Status403Forbidden,
                    ResponseModel<TicketResponse>.Error(
                        statusCode: StatusCodes.Status403Forbidden,
                        message: ex.Message
                    )
                );
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ResponseModel<TicketResponse>.Error(
                    statusCode: StatusCodes.Status404NotFound,
                    message: ex.Message
                ));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ResponseModel<TicketResponse>.Error(StatusCodes.Status400BadRequest, ex.Message));
            }
        }

        [HttpPut("{id:guid}/reject")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseModel<TicketResponse>))]
        public async Task<IActionResult> RejectTicket(Guid id, [FromQuery] string? reason, CancellationToken cancellationToken)
        {
            var emailClaim = User.Claims.FirstOrDefault(c =>
                c.Type == JwtRegisteredClaimNames.Email ||
                c.Type == ClaimTypes.Email ||
                c.Type == "email");

            var email = emailClaim?.Value;
            if (email == null)
            {
                return Unauthorized(ResponseModel<TicketResponse>.Error(
                    statusCode: StatusCodes.Status401Unauthorized,
                    message: "Unauthorized: User identity not found"
                ));
            }

            try
            {
                var result = await _ticketService.RejectTicketAsync(id, reason, cancellationToken);
                return Ok(ResponseModel<TicketResponse>.Ok(
                    data: result,
                    message: "Ticket rejected successfully"));
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(
                    StatusCodes.Status403Forbidden,
                    ResponseModel<TicketResponse>.Error(
                        statusCode: StatusCodes.Status403Forbidden,
                        message: ex.Message
                    )
                );
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ResponseModel<TicketResponse>.Error(
                    statusCode: StatusCodes.Status404NotFound,
                    message: ex.Message
                ));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ResponseModel<TicketResponse>.Error(StatusCodes.Status400BadRequest, ex.Message));
            }
        }

    }
}
