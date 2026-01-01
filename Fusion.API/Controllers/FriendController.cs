using Fusion.Repository.Bases.Page;
using Fusion.Repository.Bases.Page.Friend;
using Fusion.Repository.Bases.Responses;
using Fusion.Repository.ViewModels;
using Fusion.Service.Commons.BaseResponses;
using Fusion.Service.IServices;
using Fusion.Service.ViewModels.UserFriend.Requests;
using Fusion.Service.ViewModels.UserFriend.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Fusion.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class FriendController : ControllerBase
{
    private readonly IFriendshipService _friendshipService;

    public FriendController(IFriendshipService friendshipService)
    {
        _friendshipService = friendshipService;
    }

    /// <summary>
    /// Send friend request by email (status = Pending)
    /// </summary>
    [HttpPost("request")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseModel<FriendshipResponse>))]
    public async Task<IActionResult> SendRequest([FromBody] CreateFriendRequest request, CancellationToken cancellationToken)
    {
        var result = await _friendshipService.SendRequestByEmailAsync(request, cancellationToken);

        return Ok(ResponseModel<FriendshipResponse>.Ok(
            data: result,
            message: ResponseMessageHelper.FormatMessage(ResponseMessages.CREATE_SUCCESS, "friend request")));
    }

    /// <summary>
    /// Get list of pending requests that current user RECEIVED
    /// </summary>
    [HttpGet("pending/received")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseModel<List<FriendshipResponse>>))]
    public async Task<IActionResult> GetPendingReceived(CancellationToken cancellationToken)
    {
        var result = await _friendshipService.GetPendingReceivedAsync(cancellationToken);

        return Ok(ResponseModel<List<FriendshipResponse>>.Ok(
            data: result,
            message: ResponseMessageHelper.FormatMessage(ResponseMessages.GET_SUCCESS, "pending received friend requests")));
    }

    /// <summary>
    /// Get list of pending requests that current user SENT
    /// </summary>
    [HttpGet("pending/sent")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseModel<List<FriendshipResponse>>))]
    public async Task<IActionResult> GetPendingSent(CancellationToken cancellationToken)
    {
        var result = await _friendshipService.GetPendingSentAsync(cancellationToken);

        return Ok(ResponseModel<List<FriendshipResponse>>.Ok(
            data: result,
            message: ResponseMessageHelper.FormatMessage(ResponseMessages.GET_SUCCESS, "pending sent friend requests")));
    }

    /// <summary>
    /// Accept friend request (only addressee can accept)
    /// </summary>
    [HttpPost("{friendshipId:guid}/accept")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseModel<FriendshipResponse>))]
    public async Task<IActionResult> Accept([FromRoute] Guid friendshipId, CancellationToken cancellationToken)
    {
        var result = await _friendshipService.AcceptAsync(friendshipId, cancellationToken);

        return Ok(ResponseModel<FriendshipResponse>.Ok(
            data: result,
            message: ResponseMessageHelper.FormatMessage(ResponseMessages.UPDATE_SUCCESS, "friend request")));
    }

    /// <summary>
    /// Reject friend request (only addressee can reject)
    /// </summary>
    [HttpPost("{friendshipId:guid}/reject")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseModel<FriendshipResponse>))]
    public async Task<IActionResult> Reject([FromRoute] Guid friendshipId, CancellationToken cancellationToken)
    {
        var result = await _friendshipService.RejectAsync(friendshipId, cancellationToken);

        return Ok(ResponseModel<FriendshipResponse>.Ok(
            data: result,
            message: ResponseMessageHelper.FormatMessage(ResponseMessages.UPDATE_SUCCESS, "friend request")));
    }
    [HttpGet("/paged")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseModel<PagedResult<FriendLiteResponse>>))]
    public async Task<IActionResult> GetPagedFriends(
           [FromQuery] UserFriendPagedRequest request,
           CancellationToken cancellationToken)
    {
        var result = await _friendshipService.GetPagedFriendsByUserIdAsync(request, cancellationToken);

        return Ok(ResponseModel<PagedResult<FriendLiteResponse>>.Ok(
            data: result,
            message: ResponseMessageHelper.FormatMessage(ResponseMessages.GET_SUCCESS, "friends")));
    }
}
