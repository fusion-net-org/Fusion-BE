using Fusion.Repository.Bases.Page;
using Fusion.Repository.Bases.Page.Ticket;
using Fusion.Repository.Bases.Responses;
using Fusion.Service.Commons.BaseResponses;
using Fusion.Service.IServices;
using Fusion.Service.ViewModels.Tickets.Requests;
using Fusion.Service.ViewModels.Tickets.Responses;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

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
		[ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseModel<PagedResult<TicketResponse>>))]
		public async Task<IActionResult> GetPaged([FromQuery] TicketPagedSearchRequest request, CancellationToken cancellationToken)
		{
			var result = await _ticketService.GetPageTicketshAsync(request, cancellationToken);
			return Ok(ResponseModel<PagedResult<TicketResponse>>.Ok(
				data: result,
				message: "Get paged tickets successfully"));
		}

		[HttpPost]
		[ProducesResponseType(StatusCodes.Status201Created, Type = typeof(ResponseModel<TicketResponse>))]
		public async Task<IActionResult> CreateTicket(TicketRequest request, CancellationToken cancellationToken)
		{
			// Nếu bạn muốn lấy email từ JWT như CompanyController thì giữ lại phần này
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

			var result = await _ticketService.CreateTicketAsync(request, cancellationToken);

			return Ok(ResponseModel<TicketResponse>.Ok(
				data: result,
				message: ResponseMessageHelper.FormatMessage(ResponseMessages.CREATE_SUCCESS, "ticket")));
		}

		[HttpGet("{id:guid}")]
		[ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseModel<TicketResponse>))]
		public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
		{
			var result = await _ticketService.GetTicketByIdAsync(id);
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
		public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
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

			var result = await _ticketService.DeleteTicketAsync(id, cancellationToken);
			return Ok(ResponseModel<bool>.Ok(
				data: result ?? false,
				message: "Delete ticket successfully"));
		}
	}
}
