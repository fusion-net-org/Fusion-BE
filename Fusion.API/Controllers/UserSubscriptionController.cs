
using Fusion.Repository.Bases.Page;
using Fusion.Repository.Bases.Page.UserSubscriptions;
using Fusion.Repository.Bases.Responses;
using Fusion.Service.Commons.BaseResponses;
using Fusion.Service.IServices;
using Fusion.Service.ViewModels.UserSubscription.Requests;
using Fusion.Service.ViewModels.UserSubscription.Responses;
using Microsoft.AspNetCore.Mvc;

namespace Fusion.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserSubscriptionController : ControllerBase
    {
        private readonly IUserSubscriptionService _userSubscriptionService;

        public UserSubscriptionController(IUserSubscriptionService userSubscriptionService)
        {
            _userSubscriptionService = userSubscriptionService;
        }


        /// <summary>Create a UserSubscription from a successful Transaction.</summary>
        [HttpPost]
        public async Task<ActionResult<UserSubscriptionDetailResponse>> Create(
            [FromBody] UserSubscriptionCreateRequest req,
            CancellationToken ct)
        {
            var created = await _userSubscriptionService.CreateAsync(req, ct);
            return CreatedAtAction(nameof(GetDetail), new { id = created.Id }, created);
        }

        /// <summary>Get subscription detail by Id.</summary>
        [HttpGet("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseModel<UserSubscriptionDetailResponse>))]
        public async Task<ActionResult> GetDetail(
            Guid id, CancellationToken ct)
        {
            var dto = await _userSubscriptionService.GetDetailAsync(id, ct);
            if (dto == null) return NotFound();

            return Ok(ResponseModel< UserSubscriptionDetailResponse>.Ok(
                   data: dto,
                   message: ResponseMessageHelper.FormatMessage(ResponseMessages.GET_SUCCESS, "transactions")));
        }

        [HttpGet("userpage")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseModel<PagedResult<UserSubscriptionResponse>>))]
        public async Task<IActionResult> GetPagedByUserId(
          [FromQuery] UserSubscriptionPagedRequest request,
          CancellationToken cancellationToken)
        {
            var result = await _userSubscriptionService.GetPagedByUserIdAsync(request, cancellationToken);
            return Ok(ResponseModel<PagedResult<UserSubscriptionResponse>>.Ok(
                data: result,
                message: ResponseMessageHelper.FormatMessage(ResponseMessages.GET_SUCCESS, "transactions")));
        }


        [HttpGet("allActive")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseModel<List<UserSubscriptionActiveResponse>>))]
        public async Task<IActionResult> GetAllActiveByUserId( CancellationToken cancellationToken)
        {
            var result = await _userSubscriptionService.GetAllActiveByUserIdAsync(cancellationToken);
            return Ok(ResponseModel<List<UserSubscriptionActiveResponse>>.Ok(
                data: result,
                message: ResponseMessageHelper.FormatMessage(ResponseMessages.GET_SUCCESS, "transactions")));
        }

        /// <summary>Get active subscription of a user.</summary>
        [HttpGet("active/{userId:guid}")]
        public async Task<ActionResult<UserSubscriptionDetailResponse>> GetActiveByUser(
            Guid userId, CancellationToken ct)
        {
            var dto = await _userSubscriptionService.GetActiveByUserAsync(userId, ct);
            if (dto == null) return NotFound();
            return Ok(dto);
        }

        /// <summary>Cancel a subscription (soft-cancel: sets status & timestamp).</summary>
        [HttpPost("{id:guid}/cancel")]
        public async Task<IActionResult> Cancel(Guid id, CancellationToken ct)
        {
            var ok = await _userSubscriptionService.CancelAsync(id, ct);
            return ok ? NoContent() : NotFound();
        }

        /// <summary>Pause an active subscription.</summary>
        [HttpPost("{id:guid}/pause")]
        public async Task<IActionResult> Pause(Guid id, CancellationToken ct)
        {
            var ok = await _userSubscriptionService.PauseAsync(id, ct);
            return ok ? NoContent() : NotFound();
        }

        /// <summary>Resume a paused subscription.</summary>
        [HttpPost("{id:guid}/resume")]
        public async Task<IActionResult> Resume(Guid id, CancellationToken ct)
        {
            var ok = await _userSubscriptionService.ResumeAsync(id, ct);
            return ok ? NoContent() : NotFound();
        }



    }
}
