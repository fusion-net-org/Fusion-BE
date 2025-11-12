
using Fusion.Repository.Bases.Page;
using Fusion.Repository.Bases.Page.SubscriptionPlans;
using Fusion.Repository.Bases.Responses;
using Fusion.Service.Commons.BaseResponses;
using Fusion.Service.IServices;
using Fusion.Service.ViewModels.SubscriptionPlan.Requests;
using Fusion.Service.ViewModels.SubscriptionPlan.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Fusion.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SubscriptionPlanController : ControllerBase
    {
        private readonly ISubscriptionPlanService _subscriptionPlanService;

        public SubscriptionPlanController(ISubscriptionPlanService subscriptionPlanService)
        {
            _subscriptionPlanService = subscriptionPlanService;
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseModel<PagedResult<SubscriptionPlanResponse>>))]
        public async Task<IActionResult> GetPaged(
           [FromQuery] SubscriptionPlanPagedRequest request,
           CancellationToken cancellationToken)
        {
            var result = await _subscriptionPlanService.GetAllAsync(request, cancellationToken);

            return Ok(ResponseModel<PagedResult<SubscriptionPlanResponse>>.Ok(
                data: result,
                message: ResponseMessageHelper.FormatMessage(ResponseMessages.GET_SUCCESS, "subscription plans")));
        }

        [HttpGet("for_customer")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseModel<List<SubscriptionPlanResponse>>))]
        public async Task<IActionResult> GetAllForCustomer(
           CancellationToken cancellationToken)
        {
            var result = await _subscriptionPlanService.GetAllForCusromerAsync(cancellationToken);

            return Ok(ResponseModel<List<SubscriptionPlanResponse>>.Ok(
                data: result,
                message: ResponseMessageHelper.FormatMessage(ResponseMessages.GET_SUCCESS, "subscription plans")));
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("create")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseModel<SubscriptionPlanResponse>))]
        public async Task<IActionResult> Create(
                    [FromBody] SubscriptionPlanCreateRequest request,
                    CancellationToken cancellationToken)
        {
            var result = await _subscriptionPlanService.CreatePlanAsync(request, cancellationToken);

            return Ok(ResponseModel<SubscriptionPlanResponse>.Ok(
                data: result,
                message: ResponseMessageHelper.FormatMessage(ResponseMessages.CREATE_SUCCESS, "subscription plan")));
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("update")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseModel<SubscriptionPlanResponse>))]
        public async Task<IActionResult> Update(
         [FromBody] SubscriptionPlanUpdateRequest request,
         CancellationToken cancellationToken)
        {
            var result = await _subscriptionPlanService.UpdatePlanAsync(request, cancellationToken);

            return Ok(ResponseModel<SubscriptionPlanResponse>.Ok(
                data: result,
                message: ResponseMessageHelper.FormatMessage(ResponseMessages.UPDATE_SUCCESS, "subscription plan")));
        }

        [HttpGet("{planId:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseModel<SubscriptionPlanDetailResponse>))]
        public async Task<IActionResult> GetById(Guid planId, CancellationToken cancellationToken)
        {
            var result = await _subscriptionPlanService.GetPlanByIdAsync(planId, cancellationToken);

            return Ok(ResponseModel<SubscriptionPlanDetailResponse>.Ok(
                data: result,
                message: ResponseMessageHelper.FormatMessage(ResponseMessages.GET_SUCCESS, "subscription plan")));
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{planId:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseModel<bool>))]
        public async Task<IActionResult> Delete(Guid planId, CancellationToken cancellationToken)
        {
            var ok = await _subscriptionPlanService.DeletePlanAsync(planId, cancellationToken);

            return Ok(ResponseModel<bool>.Ok(
                data: ok,
                message: ResponseMessageHelper.FormatMessage(ResponseMessages.DELETE_SUCCESS, "subscription plan")));
        }
    }
}
