using Fusion.Repository.Bases.Responses;
using Fusion.Repository.Entities;
using Fusion.Service.Commons.BaseResponses;
using Fusion.Service.IServices;
using Fusion.Service.ViewModels.SubscriptionPackage.Requests;
using Fusion.Service.ViewModels.SubscriptionPackage.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Fusion.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class SubscriptionPackageController : ControllerBase
    {
        private readonly ISubscriptionPackageService _subService;

        public SubscriptionPackageController(ISubscriptionPackageService subService)
        {
            _subService = subService;
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseModel<SubscriptionPackage?>))]
        public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
        {
            var result = await _subService.GetSubscriptionByIdAsync(id, cancellationToken);
            return Ok(ResponseModel<SubscriptionPackage>.Ok(
                data: result,
                message: ResponseMessageHelper.FormatMessage(ResponseMessages.GET_SUCCESS, "Subscription package")
                ));
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("GetSubscriptionForAdmin")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseModel<List<SubscriptionAdminResponse>?>))]
        public async Task<IActionResult> GetSubscriptionForAdmin( CancellationToken cancellationToken)
        {
            var result = await _subService.GetAllSubscriptionForAdminAsync(cancellationToken);
            return Ok(ResponseModel<List<SubscriptionAdminResponse>>.Ok(
                data: result,
                message: ResponseMessageHelper.FormatMessage(ResponseMessages.GET_SUCCESS, "Subscription package")
                ));
        }

        [HttpGet("GetSubscriptionForCustomer")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseModel<List<SubscriptionResponse>?>))]
        public async Task<IActionResult> GetSubscriptionForCustomer(CancellationToken cancellationToken)
        {
            var result = await _subService.GetAllSubscriptionForCustomerAsync(cancellationToken);
            return Ok(ResponseModel<List<SubscriptionResponse>>.Ok(
                data: result,
                message: ResponseMessageHelper.FormatMessage(ResponseMessages.GET_SUCCESS, "Subscription package")
                ));
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseModel<SubscriptionAdminResponse?>))]
        public async Task<ActionResult> Create([FromBody] SubscriptionRequest request, CancellationToken cancellationToken)
        {
            var result = await _subService.CreateSubscriptionAsync(request, cancellationToken);
            return Ok(ResponseModel<SubscriptionAdminResponse>.Ok(
                data: result,
                message: ResponseMessageHelper.FormatMessage(ResponseMessages.CREATE_SUCCESS, "Subscription package")
                ));
        }

        [HttpPut("{id:guid}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseModel<SubscriptionAdminResponse?>))]
        public async Task<ActionResult> Update(Guid id, [FromBody] SubscriptionRequest request, CancellationToken cancellationToken)
        {
            if (id == Guid.Empty)
                return BadRequest(ResponseModel<string>.Error(
                    statusCode: StatusCodes.Status400BadRequest,
                    "Id in URL and request boby not match"));

            var result = await _subService.UpdateSubscriptionAsync(id, request, cancellationToken);
            return Ok(ResponseModel<SubscriptionAdminResponse>.Ok(
                data: result,
                message: ResponseMessageHelper.FormatMessage(ResponseMessages.UPDATE_SUCCESS, "Subscription package")
                ));
        }

        [HttpDelete("{id:guid}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseModel<bool>))]
        public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
        {
            var success = await _subService.DeleteSubscriptionAsync(id, cancellationToken);
            if (!success)
            {
                return NotFound(ResponseModel<bool>.Error(
                    statusCode: StatusCodes.Status404NotFound,
                    message: ResponseMessageHelper.FormatMessage(ResponseMessages.NOT_FOUND, "Subscription package")
                ));
            }

            return NoContent();
        }
    }
}
