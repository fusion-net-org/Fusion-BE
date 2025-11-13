using System.Security.Claims;
using Fusion.Repository.Bases.Page;
using Fusion.Repository.Bases.Page.TicketComment;
using Fusion.Repository.Entities;
using Fusion.Service.Commons.BaseResponses;
using Fusion.Service.IServices;
using Fusion.Service.ViewModels.TicketComment;
using Microsoft.AspNetCore.Mvc;

[Route("api/ticket-comment")]
[ApiController]
public class TicketCommentController : ControllerBase
{
    private readonly ITicketCommentService _ticketCommentService;

    public TicketCommentController(ITicketCommentService ticketCommentService)
    {
        _ticketCommentService = ticketCommentService;
    }

    private Guid GetUserIdFromClaims()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            throw new UnauthorizedAccessException("Unauthorized: User identity not found");

        return userId;
    }

    [HttpGet("paged")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseModel<PagedResult<TicketCommentResponse>>))]
    public async Task<IActionResult> GetPaged([FromQuery] TicketCommentPagedRequest request, CancellationToken cancellationToken)
    {
        var userId = GetUserIdFromClaims();
        var result = await _ticketCommentService.GetCommentsByTicketIdAsync(userId, request, cancellationToken);

        return Ok(ResponseModel<PagedResult<TicketCommentResponse>>.Ok(
            data: result,
            message: "Get paged ticket comments successfully"));
    }

    [HttpGet("{id:long}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseModel<TicketCommentResponse>))]
    public async Task<IActionResult> GetById(long id, CancellationToken cancellationToken)
    {
        var userId = GetUserIdFromClaims();
        var result = await _ticketCommentService.GetByIdAsync(userId, id);
        if (result == null)
            return NotFound(ResponseModel<TicketCommentResponse>.Error(
                statusCode: StatusCodes.Status404NotFound,
                message: "Ticket comment not found"));

        return Ok(ResponseModel<TicketCommentResponse>.Ok(
            data: result,
            message: "Get ticket comment by id successfully"));
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(ResponseModel<TicketCommentResponse>))]
    public async Task<IActionResult> CreateComment(TicketCommentRequest request, CancellationToken cancellationToken)
    {
        var userId = GetUserIdFromClaims();

        var comment = new Fusion.Repository.Entities.TicketComment
        {
            TicketId = request.TicketId,
            AuthorUserId = userId, // chỉ định author
            Body = request.Body
        };

        var result = await _ticketCommentService.AddCommentAsync(comment, cancellationToken);

        return Ok(ResponseModel<TicketCommentResponse>.Ok(
            data: result,
            message: "Create ticket comment successfully"));
    }

    [HttpPut("{id:long}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseModel<TicketCommentResponse>))]
    public async Task<IActionResult> UpdateComment(long id, TicketCommentRequestUpdate request, CancellationToken cancellationToken)
    {
        var userId = GetUserIdFromClaims();

        var existingComment = await _ticketCommentService.GetByIdAsync(userId, id);
        if (existingComment == null)
            return NotFound(ResponseModel<TicketCommentResponse>.Error(
                statusCode: StatusCodes.Status404NotFound,
                message: "Ticket comment not found"));

        var comment = new Fusion.Repository.Entities.TicketComment
        {
            Id = id,
            TicketId = existingComment.TicketId,
            AuthorUserId = existingComment.AuthorUserId,
            Body = request.Body ?? existingComment.Body,
            CreateAt = existingComment.CreateAt
        };

        var result = await _ticketCommentService.UpdateCommentAsync(userId, comment, cancellationToken);

        return Ok(ResponseModel<TicketCommentResponse>.Ok(
            data: result,
            message: "Update ticket comment successfully"));
    }

    [HttpDelete("{id:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> DeleteComment(long id, CancellationToken cancellationToken)
    {
        var userId = GetUserIdFromClaims();

        var result = await _ticketCommentService.DeleteCommentAsync(userId, id, cancellationToken);
        if (!result.HasValue || !result.Value)
            return NotFound(ResponseModel<bool>.Error(
                statusCode: StatusCodes.Status404NotFound,
                message: "Ticket comment not found"));

        return Ok(ResponseModel<bool>.Ok(
            data: true,
            message: "Delete ticket comment successfully"));
    }
}
