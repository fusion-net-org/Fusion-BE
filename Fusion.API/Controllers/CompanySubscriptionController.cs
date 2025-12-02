using Fusion.Repository.Bases.Page;
using Fusion.Repository.Bases.Page.CompanySubscriptions;
using Fusion.Repository.ViewModels.CompanySubscriptionEntry;
using Fusion.Service.Commons.BaseResponses;
using Fusion.Service.IServices;
using Fusion.Service.Services;
using Fusion.Service.ViewModels.CompanySubscription.Requests;
using Fusion.Service.ViewModels.CompanySubscription.Responses;
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

        /// <summary>
        /// Lấy chi tiết một Company Subscription theo ID
        /// </summary>
        [HttpGet("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseModel<CompanySubscriptionDetailResponse>))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
        {
            var result = await _service.GetDetailAsync(id, cancellationToken);
            return Ok(ResponseModel<CompanySubscriptionDetailResponse>.Ok(
                data: result,
                message: "Get company subscription detail successfully."));
        }

        /// <summary>
        /// Lấy danh sách Company Subscription theo CompanyId
        /// </summary>
        [HttpGet("company/{companyId:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseModel<PagedResult<CompanySubscriptionListResponse>>))]
        public async Task<IActionResult> GetByCompanyId(
            Guid companyId,
            [FromQuery] CompanySubscriptionPagedRequest request,
            CancellationToken cancellationToken)
        {
            var result = await _service.GetAllByCompanyAsync(companyId, request, cancellationToken);
            return Ok(ResponseModel<PagedResult<CompanySubscriptionListResponse>>.Ok(
                data: result,
                 message: "Get list company subscription by companyId successfully."));
        }

        /// <summary>
        /// Lấy tất cả Company Subscriptions đang active của 1 công ty (dùng cho dropdown)
        /// </summary>
        [HttpGet("company/{companyId}/active")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseModel<List<CompanySubscriptionActiveResponse>>))]
        public async Task<IActionResult> GetActiveByCompanyId([FromRoute] Guid companyId, CancellationToken cancellationToken)
        {
            var result = await _service.GetAllActiveByCompanyIdAsync(companyId, cancellationToken);
            return Ok(ResponseModel<List<CompanySubscriptionActiveResponse>>.Ok(
                data: result,
                message: "Get active company subscriptions successfully"));
        }

        [HttpGet("{companySubscriptionId:guid}/user-usage")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseModel<List<CompanySubscriptionUserUsageItem>>))]
        public async Task<IActionResult> GetUserUsageByCompanySubscription(Guid companySubscriptionId,CancellationToken ct = default)
        {
            var data = await _service.GetUserUsageAsync(companySubscriptionId, ct);

            return Ok(ResponseModel<List<CompanySubscriptionUserUsageItem>>.Ok(
                data: data,
                message: "Get users usage for company subscription successfully"));
        }
    }
}
