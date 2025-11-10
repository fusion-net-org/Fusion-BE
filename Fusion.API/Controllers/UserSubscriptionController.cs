
using Fusion.Repository.Bases.Page;
using Fusion.Repository.Bases.Page.UserSubscriptions;
using Fusion.Repository.Enums;
using Fusion.Service.Commons.BaseResponses;
using Fusion.Service.IServices;
using Fusion.Service.ViewModels.UserSubscription.Requests;
using Fusion.Service.ViewModels.UserSubscription.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Fusion.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UserSubscriptionController : ControllerBase
    {
        private readonly IUserSubscriptionService _userSubscriptionService;

        public UserSubscriptionController(IUserSubscriptionService userSubscriptionService)
        {
            _userSubscriptionService = userSubscriptionService;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseModel<PagedResult<UserSubscriptionListItem>>))]
        public async Task<IActionResult> GetAll([FromQuery] UserSubscriptionPagedRequest request, CancellationToken cancellationToken)
        {
            var result = await _userSubscriptionService.GetAllAsync(request, cancellationToken);
            return Ok(ResponseModel<PagedResult<UserSubscriptionListItem>>.Ok(
                data: result,
                message: "Get user subscriptions successfully"));
        }

        [HttpGet("user")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseModel<PagedResult<UserSubscriptionListItem>>))]
        public async Task<IActionResult> GetAllByUserId([FromQuery] UserSubscriptionPagedRequest request, CancellationToken cancellationToken)
        {
            var result = await _userSubscriptionService.GetAllByUserIdAsync(request, cancellationToken);
            return Ok(ResponseModel<PagedResult<UserSubscriptionListItem>>.Ok(
                data: result,
                message: "Get user subscriptions successfully"));
        }
        /// <summary>
        /// Lấy chi tiết 1 UserSubscription theo Id
        /// </summary>
        [HttpGet("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseModel<UserSubscriptionDetailResponse>))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
        {
            var result = await _userSubscriptionService.GetByIdAsync(id, cancellationToken);

            return Ok(ResponseModel<UserSubscriptionDetailResponse>.Ok(
                data: result,
                message: "Get user subscription successfully"));
        }

        /// <summary>
        /// Tạo mới UserSubscription
        /// </summary>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseModel<UserSubscriptionDetailResponse>))]
        public async Task<IActionResult> Create([FromBody] UserSubscriptionCreateRequest request, CancellationToken cancellationToken)
        {
            var result = await _userSubscriptionService.CreateAsync(request, cancellationToken);
            return Ok(ResponseModel<UserSubscriptionDetailResponse>.Ok(
                data: result,
                message: "Create user subscription successfully"));
        }

        /// <summary>
        /// Cập nhật trạng thái (Status) cho UserSubscription
        /// </summary>
        [HttpPut("{id:guid}/status")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseModel<UserSubscriptionDetailResponse>))]
        public async Task<IActionResult> UpdateStatus(Guid id, [FromQuery] SubscriptionStatus status, CancellationToken cancellationToken)
        {
            var result = await _userSubscriptionService.UpdateStatusAsync(id, status, cancellationToken);
            return Ok(ResponseModel<UserSubscriptionDetailResponse>.Ok(
                data: result,
                message: $"Update user subscription status to {status} successfully"));
        }
    }
}
