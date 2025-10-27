
using Fusion.Repository.Bases.Page;
using Fusion.Service.Commons.BaseResponses;
using Fusion.Service.IServices;
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

        [HttpGet("my-subscriptions")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseModel<PagedResult<UserSubscriptionResponse>>))]
        public async Task<IActionResult> GetAllUserSubscriptionsByUserIdAsync(
           [FromQuery] PagedRequest request,
           CancellationToken cancellationToken)
        {
            var result = await _userSubscriptionService.GetAllUserSubscrptionByUserIdAsync(request, cancellationToken);

            return Ok(ResponseModel<PagedResult<UserSubscriptionResponse>>.Ok(
                data: result,
                message: "Lấy danh sách gói đăng ký thành công."));
        }
    }
}
