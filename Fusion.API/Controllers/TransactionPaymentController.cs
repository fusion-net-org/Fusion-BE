
using Fusion.Repository.Bases.Page;
using Fusion.Repository.Bases.Page.TransactionPayment;
using Fusion.Repository.Bases.Responses;
using Fusion.Service.Commons.BaseResponses;
using Fusion.Service.IServices;
using Fusion.Service.ViewModels.TransactionPayment.Requests;
using Fusion.Service.ViewModels.TransactionPayment.Responses;
using Fusion.Service.ViewModels.Users.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Fusion.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class TransactionPaymentController : ControllerBase
    {
        private readonly ITransactionPaymentService _service;

        public TransactionPaymentController(ITransactionPaymentService service)
        {
            _service = service;
        }

        // ====== CREATE (Draft) ======
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
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseModel<PagedResult<TransactionPaymentResponse>>))]
        public async Task<IActionResult> GetPaged(
            [FromQuery] TransactionPaymentPagedRequest request,
            CancellationToken cancellationToken)
        {
            var result = await _service.GetPagedAsync(request, cancellationToken);
            return Ok(ResponseModel<PagedResult<TransactionPaymentResponse>>.Ok(
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
    }
}
