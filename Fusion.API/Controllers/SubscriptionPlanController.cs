
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
        private readonly ISubscriptionPlanService _service;

        public SubscriptionPlanController(ISubscriptionPlanService service)
        {
            _service = service;
        }

        /// <summary>Create a new subscription plan</summary>
        [HttpPost("create")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseModel<SubscriptionPlanDetailResponse>))]
        public async Task<IActionResult> Create(
            [FromBody] SubscriptionPlanCreateRequest request,
            CancellationToken cancellationToken)
        {
            var result = await _service.CreatePlanAsync(request, cancellationToken);
            return Ok(ResponseModel<SubscriptionPlanDetailResponse>.Ok(
                data: result,
                message: ResponseMessageHelper.FormatMessage(ResponseMessages.CREATE_SUCCESS, "subscription plan")));
        }

        [HttpPut("update")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseModel<SubscriptionPlanDetailResponse>))]
        public async Task<IActionResult> Update(
          [FromBody] SubscriptionPlanUpdateRequest request,
          CancellationToken cancellationToken)
        {
            var result = await _service.UpdatePlanAsync(request, cancellationToken);
            return Ok(ResponseModel<SubscriptionPlanDetailResponse>.Ok(
                data: result,
                message: ResponseMessageHelper.FormatMessage(ResponseMessages.UPDATE_SUCCESS, "subscription plan")));
        }

        /// <summary>Delete subscription plan</summary>
        [HttpDelete("{id:guid}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseModel<bool>))]
        public async Task<IActionResult> Delete([FromRoute] Guid id, CancellationToken cancellationToken)
        {
            var ok = await _service.DeletePlanAsync(id, cancellationToken);
            return Ok(ResponseModel<bool>.Ok(
                data: ok,
                message: ResponseMessageHelper.FormatMessage(
                    ok ? ResponseMessages.DELETE_SUCCESS : ResponseMessages.NOT_FOUND,
                    "subscription plan")));
        }

        /// <summary>Get subscription plans (paged)</summary>
        [HttpGet]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseModel<PagedResult<SubscriptionPlanListItemResponse>>))]
        public async Task<IActionResult> GetAll(
            [FromQuery] SubscriptionPlanPagedRequest request,
            CancellationToken cancellationToken)
        {
            var paged = await _service.GetAllAsync(request, cancellationToken);
            return Ok(ResponseModel<PagedResult<SubscriptionPlanListItemResponse>>.Ok(
                data: paged,
                message: ResponseMessageHelper.FormatMessage(ResponseMessages.GET_SUCCESS, "subscription list plan")));
        }

        /// <summary>Get plan detail by id</summary>
        [HttpGet("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseModel<SubscriptionPlanDetailResponse>))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ResponseModel<object>))]
        public async Task<IActionResult> GetById([FromRoute] Guid id, CancellationToken cancellationToken)
        {
            var detail = await _service.GetPlanByIdAsync(id, cancellationToken);
            return Ok(ResponseModel<SubscriptionPlanDetailResponse>.Ok(
                data: detail,
                message: ResponseMessageHelper.FormatMessage(ResponseMessages.GET_SUCCESS, "subscription plan detail")));
        }

        [HttpGet("public")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseModel<List<SubscriptionPlanCustomerResponse>>))]
        public async Task<IActionResult> GetPublic(CancellationToken cancellationToken)
        {
            var data = await _service.GetAllForCusromerAsync(cancellationToken);
            return Ok(ResponseModel<List<SubscriptionPlanCustomerResponse>>.Ok(
                data: data,
                message: ResponseMessageHelper.FormatMessage(ResponseMessages.GET_SUCCESS, "subscription plan for customer")));
        }
    }
}
