
using Fusion.Repository.Bases.Page;
using Fusion.Repository.Bases.Page.TransactionPayment;
using Fusion.Repository.Bases.Responses;
using Fusion.Service.Commons.BaseResponses;
using Fusion.Service.IServices;
using Fusion.Service.ViewModels.TransactionPayment.Requests;
using Fusion.Service.ViewModels.TransactionPayment.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Fusion.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class TransactionPaymentController : ControllerBase
    {
        private readonly ITransactionPaymentService _transactionPaymentService;

        public TransactionPaymentController(ITransactionPaymentService transactionPaymentService)
        {
            _transactionPaymentService = transactionPaymentService;
        }
        /// <summary>
        /// create transaction
        /// </summary>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseModel<TransactionPaymentResponse>))]
        public async Task<IActionResult> Create([FromBody] TransactionPaymentCreateRequest request, CancellationToken cancellationToken)
        {
            var result = await _transactionPaymentService.CreateAsync(request, cancellationToken);
            return Ok(ResponseModel<TransactionPaymentResponse>.Ok(
                data: result,
                message: ResponseMessageHelper.FormatMessage(ResponseMessages.CREATE_SUCCESS, "Transaction payment")
            ));
        }

        [HttpGet("paged")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseModel<PagedResult<TransactionPaymentResponse>>))]
        public async Task<IActionResult> GetPaged(
               [FromQuery] TransactionPaymentPagedRequest request,
               CancellationToken cancellationToken)
        {
            var result = await _transactionPaymentService.GetPagedAsync(request, cancellationToken);
            return Ok(ResponseModel<PagedResult<TransactionPaymentResponse>>.Ok(
                data: result,
                message: ResponseMessageHelper.FormatMessage(ResponseMessages.GET_SUCCESS, "Transactions")));
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseModel<TransactionPaymentDetailResponse>))]
        public async Task<IActionResult> GetDetail(Guid id, CancellationToken cancellationToken)
        {
            var result = await _transactionPaymentService.GetDetailAsync(id, cancellationToken);
            return Ok(ResponseModel<TransactionPaymentDetailResponse>.Ok(
                data: result,
                message: ResponseMessageHelper.FormatMessage(ResponseMessages.GET_SUCCESS, "Transaction")));
        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseModel<bool>))]
        public async Task<IActionResult> Update(
            Guid id,
            [FromBody] TransactionPaymentUpdateRequest request,
            CancellationToken cancellationToken)
        {
            var ok = await _transactionPaymentService.UpdateAsync(id, request, cancellationToken);
            return Ok(ResponseModel<bool>.Ok(
                data: ok,
                message: ResponseMessageHelper.FormatMessage(ResponseMessages.UPDATE_SUCCESS, "Transaction")));
        }

        /// <summary>
        /// Delete a transaction
        /// </summary>
        [HttpDelete("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseModel<bool>))]
        public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
        {
            var ok = await _transactionPaymentService.DeleteAsync(id, cancellationToken);
            return Ok(ResponseModel<bool>.Ok(
                data: ok,
                message: ResponseMessageHelper.FormatMessage(ResponseMessages.DELETE_SUCCESS, "Transaction")));
        }
    }
}
