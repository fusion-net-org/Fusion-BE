
using Fusion.Repository.Bases.Page.TransactionPayment;
using Fusion.Repository.Bases.Responses;
using Fusion.Repository.ViewModels.SubscriptionPlan;
using Fusion.Repository.ViewModels.Transactions;
using Fusion.Service.Commons.BaseResponses;
using Fusion.Service.IServices;
using Fusion.Service.ViewModels.SubscriptionPlan.Responses;
using Fusion.Service.ViewModels.TransactionPayment.Requests;
using Fusion.Service.ViewModels.TransactionPayment.Responses;
using Fusion.Service.ViewModels.TransactionPayment.Responses.Overview;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Fusion.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]

    public class TransactionPaymentController : ControllerBase
    {
        private readonly ITransactionPaymentService _service;

        public TransactionPaymentController(ITransactionPaymentService service)
        {
            _service = service;
        }

        // ====== CREATE (Draft) ======
        [Authorize]
        [HttpPost("create")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseModel<TransactionPaymentDetailResponse>))]
        public async Task<IActionResult> Create(
            [FromBody] TransactionPaymentCreateRequest request,
            CancellationToken cancellationToken)
        {
            var result = await _service.CreateAsync(request, cancellationToken);
            return Ok(ResponseModel<TransactionPaymentDetailResponse>.Ok(
                data: result,
                message: ResponseMessageHelper.FormatMessage(ResponseMessages.CREATE_SUCCESS, "transaction payment")));
        }
        [HttpGet("installments/next")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseModel<TransactionPaymentDetailResponse?>))]
        public async Task<IActionResult> GetNextPendingInstallment(
            [FromQuery] Guid planId,
            [FromQuery] Guid? userSubscriptionId,
            CancellationToken cancellationToken)
        {
            // Service đã tự validate planId và current user
            var result = await _service.FindEarliestPendingInstallmentAsync(
                planId,
                userSubscriptionId,
                cancellationToken);

            return Ok(ResponseModel<TransactionPaymentDetailResponse?>.Ok(
                data: result,
                message: "get next installment transaction success"));
        }

        // ====== DETAIL ======
        [Authorize]
        [HttpGet("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseModel<TransactionPaymentDetailResponse?>))]
        public async Task<IActionResult> GetDetail(Guid id, CancellationToken cancellationToken)
        {
            var result = await _service.GetDetailAsync(id, cancellationToken);
            return Ok(ResponseModel<TransactionPaymentDetailResponse?>.Ok(
                data: result,
                message: ResponseMessageHelper.FormatMessage(ResponseMessages.GET_SUCCESS, "transaction payment")));
        }

        // ====== PAGED LIST ======
        [Authorize(Roles = "Admin")]
        [HttpGet("admin/pagedTransaction")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseModel<TransactionPaymentPagedSummaryResponse>))]
        public async Task<IActionResult> GetPaged(
            [FromQuery] TransactionPaymentPagedRequest request,
            CancellationToken cancellationToken)
        {
            var result = await _service.GetPagedAsync(request, cancellationToken);
            return Ok(ResponseModel<TransactionPaymentPagedSummaryResponse>.Ok(
                data: result,
                message: ResponseMessageHelper.FormatMessage(ResponseMessages.GET_SUCCESS, "transactions")));
        }


        [HttpPatch("{id:guid}/attach-link")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseModel<bool>))]
        public async Task<IActionResult> AttachPaymentLink(
            Guid id,
            [FromBody] AttachPaymentLinkRequest request,
            CancellationToken cancellationToken)
        {
            var ok = await _service.AttachPaymentLinkAsync(id, request.OrderCode, request.PaymentLinkId, request.Provider, cancellationToken);
            return Ok(ResponseModel<bool>.Ok(
                data: ok,
                message: ResponseMessageHelper.FormatMessage(ResponseMessages.UPDATE_SUCCESS, "payment link")));
        }

        [HttpPatch("{id:guid}/mark-success")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseModel<bool>))]
        public async Task<IActionResult> MarkSuccess(
           Guid id,
           [FromBody] MarkSuccessRequest request,
           CancellationToken cancellationToken)
        {
            var ok = await _service.MarkSuccessAsync(id, request.Amount, request.PaidAt, request.Reference, cancellationToken);
            return Ok(ResponseModel<bool>.Ok(
                data: ok,
                message: ResponseMessageHelper.FormatMessage(ResponseMessages.UPDATE_SUCCESS, "transaction status")));
        }

        [HttpPatch("{id:guid}/mark-failed")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseModel<bool>))]
        public async Task<IActionResult> MarkFailed(
           Guid id,
           [FromBody] MarkFailedRequest request,
           CancellationToken cancellationToken)
        {
            var ok = await _service.MarkFailedAsync(id, request.Description, request.Reference, cancellationToken);
            return Ok(ResponseModel<bool>.Ok(
                data: ok,
                message: ResponseMessageHelper.FormatMessage(ResponseMessages.UPDATE_SUCCESS, "transaction status")));
        }

        [HttpGet("due")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseModel<List<TransactionPaymentResponse>>))]
        public async Task<IActionResult> GetDue(
           [FromQuery] DateTimeOffset? asOf,
           [FromQuery] int take = 100,
           CancellationToken cancellationToken = default)
        {
            var cutoff = asOf ?? DateTimeOffset.UtcNow;
            var result = await _service.GetDueAsync(cutoff, take, cancellationToken);
            return Ok(ResponseModel<List<TransactionPaymentResponse>>.Ok(
                data: result,
                message: ResponseMessageHelper.FormatMessage(ResponseMessages.GET_SUCCESS, "due transactions")));
        }



        // ========================== OVERVIEW ==========================
        [Authorize(Roles = "Admin")]
        [HttpGet("admin/revenue/monthly")]
        [ProducesResponseType(
        StatusCodes.Status200OK,Type = typeof(ResponseModel<TransactionMonthlyRevenueResponse>))]
        public async Task<IActionResult> GetMonthlyRevenue( [FromQuery] int? year, CancellationToken cancellationToken)
        {
            var result = await _service.GetMonthlyRevenueAsync(year, cancellationToken);

            return Ok(ResponseModel<TransactionMonthlyRevenueResponse>.Ok(
                data: result,
                message: ResponseMessageHelper.FormatMessage(ResponseMessages.GET_SUCCESS, "monthly revenue")));
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("admin/revenue/monthly-three-years")]
        [ProducesResponseType(StatusCodes.Status200OK,Type = typeof(ResponseModel<TransactionMonthlyRevenueThreeYearsResponse>))]
        public async Task<IActionResult> GetMonthlyRevenueThreeYears([FromQuery] int? year,CancellationToken cancellationToken)
        {
            var result = await _service.GetMonthlyRevenueThreeYearsAsync(year, cancellationToken);

            return Ok(ResponseModel<TransactionMonthlyRevenueThreeYearsResponse>.Ok(
                data: result,
                message: ResponseMessageHelper.FormatMessage( ResponseMessages.GET_SUCCESS,"monthly revenue 3-year comparison")));
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("admin/monthly-status")]
        [ProducesResponseType(StatusCodes.Status200OK,Type = typeof(ResponseModel<TransactionMonthlyStatusResponse>))]
        public async Task<IActionResult> GetMonthlyStatus([FromQuery] int year,CancellationToken cancellationToken)
        {
            var result = await _service.GetMonthlyStatusAsync(year, cancellationToken);

            return Ok(ResponseModel<TransactionMonthlyStatusResponse>.Ok(
                data: result,
                message: ResponseMessageHelper.FormatMessage( ResponseMessages.GET_SUCCESS, "monthly payment status")));
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("admin/analytics/daily-cashflow")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseModel<TransactionDailyCashflowResponse>))]
        public async Task<IActionResult> GetDailyCashflow( [FromQuery] int days = 30,CancellationToken cancellationToken = default)
        {
            var result = await _service.GetDailyCashflowAsync(days, cancellationToken);

            return Ok(ResponseModel<TransactionDailyCashflowResponse>.Ok(
                data: result,
                message: ResponseMessageHelper.FormatMessage( ResponseMessages.GET_SUCCESS, "daily cashflow")));
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("admin/analytics/installments/aging")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseModel<TransactionInstallmentAgingResponse>))]
        public async Task<IActionResult> GetInstallmentAging([FromQuery] DateTimeOffset? asOf,CancellationToken cancellationToken)
        {
            var result = await _service.GetInstallmentAgingAsync(asOf, cancellationToken);

            return Ok(ResponseModel<TransactionInstallmentAgingResponse>.Ok(
                data: result,
                message: ResponseMessageHelper.FormatMessage( ResponseMessages.GET_SUCCESS, "installment aging")));
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("admin/top-customers")]
        [ProducesResponseType(StatusCodes.Status200OK,Type = typeof(ResponseModel<TransactionTopCustomersResponse>))]
        public async Task<IActionResult> GetTopCustomers([FromQuery] int year,[FromQuery] int topN = 5,CancellationToken ct = default)
        {
            var result = await _service.GetTopCustomersAsync(year, topN, ct);

            return Ok(ResponseModel<TransactionTopCustomersResponse>.Ok(
                data: result,
                message: ResponseMessageHelper.FormatMessage(ResponseMessages.GET_SUCCESS, "top customers")));
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("admin/payment-mode-insight")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseModel<TransactionPaymentModeInsightResponse>))]
        public async Task<IActionResult> GetPaymentModeInsight([FromQuery] int year,CancellationToken ct)
        {
            if (year <= 0) year = DateTime.UtcNow.Year;

            var result = await _service.GetPaymentModeInsightAsync(year, ct);
            return Ok(result);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("admin/plan-revenue-insight")]
        [ProducesResponseType(StatusCodes.Status200OK,Type = typeof(ResponseModel<TransactionPlanRevenueInsightResponse>))]
        public async Task<IActionResult> GetPlanRevenueInsight([FromQuery] int year,CancellationToken ct)
        {
            if (year <= 0) year = DateTime.UtcNow.Year;

            var result = await _service.GetPlanRevenueInsightAsync(year, ct);
            return Ok(result);
        }
        [Authorize(Roles = "Admin")]
        [HttpGet("admin/plans/table")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseModel<IEnumerable<SubscriptionPlanPurchaseStatResponse>>))]
        public async Task<IActionResult> GetPlanPurchaseTable(CancellationToken ct)
        {
            var data = await _service.GetPlanPurchaseStatsAsync(ct);

            // Tuỳ bạn có wrapper BaseResponse hay ApiResponse gì thì bung ra ở đây
            return Ok(ResponseModel<IEnumerable<SubscriptionPlanPurchaseStatResponse>>.Ok(
               data: data,
               message: ResponseMessageHelper.FormatMessage(ResponseMessages.GET_SUCCESS, "Get subscription plan purchase stats successfully")));
        }


        [Authorize(Roles = "Admin")]
        [HttpGet("admin/plans/ratio")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseModel<IEnumerable<SubscriptionPlanPurchaseStatResponse>>))]
        public async Task<IActionResult> GetPlanPurchaseRatio([FromQuery] int top = 3, CancellationToken ct = default)
        {
            var data = await _service.GetTopPlanPurchaseStatsAsync(top, includeOther: true, ct);

            // Tuỳ bạn có wrapper BaseResponse hay ApiResponse gì thì bung ra ở đây
            return Ok(ResponseModel<IEnumerable<SubscriptionPlanPurchaseStatResponse>>.Ok(
               data: data,
               message: ResponseMessageHelper.FormatMessage(ResponseMessages.GET_SUCCESS, "Get subscription plan purchase ratio successfully.")));
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("admin/monthly-purchases")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseModel<List<PlanMonthlyPurchaseCountRow>>))]
        public async Task<ActionResult> GetPlanMonthlyPurchases(
          [FromQuery] int year = 2025, CancellationToken ct = default)
        {

            var data = await _service.GetPlanMonthlyPurchaseStatsAsync(year, ct);
            return Ok(ResponseModel<List<PlanMonthlyPurchaseCountRow>>.Ok(
             data: data,
             message: ResponseMessageHelper.FormatMessage(ResponseMessages.GET_SUCCESS, "Retrieved plan monthly purchases successfully.")));

        }
    }
}
