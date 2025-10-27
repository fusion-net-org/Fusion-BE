
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
        public async Task<IActionResult> Create([FromBody] CreateTransactionRequest request, CancellationToken cancellationToken)
        {
            var result = await _transactionPaymentService.CreateTransactionPaymentAsync(request, cancellationToken);
            return Ok(ResponseModel<TransactionPaymentResponse>.Ok(
                data: result,
                message: ResponseMessageHelper.FormatMessage(ResponseMessages.CREATE_SUCCESS, "Transaction payment")
            ));
        }

        /// <summary>
        ///Get transaction by code
        /// </summary>
        [HttpGet("{code}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseModel<TransactionPaymentResponse>))]
        public async Task<IActionResult> GetByCode(string code, CancellationToken cancellationToken)
        {
            var result = await _transactionPaymentService.GetTransactionByCodeAsync(code, cancellationToken);
            return Ok(ResponseModel<TransactionPaymentResponse>.Ok(
                data: result,
                message: ResponseMessageHelper.FormatMessage(ResponseMessages.GET_SUCCESS, "Transaction payment")
            ));
        }
        [HttpGet("latest")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseModel<Guid>))]
        public async Task<IActionResult> GetLatestTransactionForCurrentUser(CancellationToken cancellationToken)
        {
            var result = await _transactionPaymentService.GetLasterTransactionForUserAsync(cancellationToken);

            return Ok(ResponseModel<Guid>.Ok(
                data: result,
                message: ResponseMessageHelper.FormatMessage(ResponseMessages.GET_SUCCESS, "latest transaction")
            ));
        }
    }
}
