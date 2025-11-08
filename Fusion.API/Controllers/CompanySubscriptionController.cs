using Fusion.Service.Commons.BaseResponses;
using Fusion.Service.IServices;
using Fusion.Service.Services;
using Fusion.Service.ViewModels.CompanySubscription.Requests;
using Fusion.Service.ViewModels.CompanySubscription.Responses;
using Fusion.Service.ViewModels.UserSubscription.Requests;
using Fusion.Service.ViewModels.UserSubscription.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Fusion.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CompanySubscriptionController : ControllerBase
    {
        private readonly ICompanySubscriptionService _service;

        public CompanySubscriptionController(ICompanySubscriptionService service)
        {
            _service = service;
        }

        /// <summary>
        /// Tạo mới Company subscription
        /// </summary>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseModel<CompanySubscriptionDetailResponse>))]
        public async Task<IActionResult> Create([FromBody] CompanySubscriptionCreateRequest request, CancellationToken cancellationToken)
        {
            var result = await _service.CreateAsync(request, cancellationToken);
            return Ok(ResponseModel<CompanySubscriptionDetailResponse>.Ok(
                data: result,
                message: "Create company subscription successfully"));
        }
    }
}
