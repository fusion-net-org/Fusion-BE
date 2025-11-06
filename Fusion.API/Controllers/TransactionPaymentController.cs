
//using Fusion.Repository.Bases.Page;
//using Fusion.Repository.Bases.Page.TransactionPayment;
//using Fusion.Repository.Bases.Responses;
//using Fusion.Service.Commons.BaseResponses;
//using Fusion.Service.IServices;
//using Fusion.Service.ViewModels.TransactionPayment.Requests;
//using Fusion.Service.ViewModels.TransactionPayment.Responses;
//using Fusion.Service.ViewModels.Users.Responses;
//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Mvc;

//namespace Fusion.API.Controllers
//{
//    [Route("api/[controller]")]
//    [ApiController]
//    [Authorize]
//    public class TransactionPaymentController : ControllerBase
//    {
//        private readonly ITransactionPaymentService _transactionPaymentService;

//        public TransactionPaymentController(ITransactionPaymentService transactionPaymentService)
//        {
//            _transactionPaymentService = transactionPaymentService;
//        }
//        /// <summary>
//        /// create transaction
//        /// </summary>
//        [HttpPost]
//        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseModel<TransactionPaymentResponse>))]
//        public async Task<IActionResult> Create([FromBody] CreateTransactionRequest request, CancellationToken cancellationToken)
//        {
//            var result = await _transactionPaymentService.CreateTransactionPaymentAsync(request, cancellationToken);
//            return Ok(ResponseModel<TransactionPaymentResponse>.Ok(
//                data: result,
//                message: ResponseMessageHelper.FormatMessage(ResponseMessages.CREATE_SUCCESS, "Transaction payment")
//            ));
//        }

//        [HttpGet("{code}")]
//        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseModel<TransactionPaymentResponse>))]
//        public async Task<IActionResult> GetByCode(string code, CancellationToken cancellationToken)
//        {
//            var result = await _transactionPaymentService.GetTransactionByCodeAsync(code, cancellationToken);
//            return Ok(ResponseModel<TransactionPaymentResponse>.Ok(
//                data: result,
//                message: ResponseMessageHelper.FormatMessage(ResponseMessages.GET_SUCCESS, "Transaction payment")
//            ));
//        }
//        [HttpGet("latest")]
//        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseModel<Guid>))]
//        public async Task<IActionResult> GetLatestTransactionForCurrentUser(CancellationToken cancellationToken)
//        {
//            var result = await _transactionPaymentService.GetLasterTransactionForUserAsync(cancellationToken);

//            return Ok(ResponseModel<Guid>.Ok(
//                data: result,
//                message: ResponseMessageHelper.FormatMessage(ResponseMessages.GET_SUCCESS, "latest transaction")
//            ));
//        }

//        [Authorize(Roles = "Admin")]
//        [HttpGet("getAllForAdmin")]
//        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseModel<PagedResult<TransactionForAdminResponse>>))]
//        public async Task<IActionResult> GetAllTransactionForAdmin(
//            [FromQuery] AdminTransactionSearch request,
//            CancellationToken cancellationToken = default)
//        {
//            var data = await _transactionPaymentService.GetAllTransactionForAdminAsync(request, cancellationToken);
//            return Ok(ResponseModel<PagedResult<TransactionForAdminResponse>>.Ok(
//                data: data,
//                message: "Get transactions for admin successfully"));
//        }

//        [Authorize(Roles = "Admin")]
//        [HttpGet("stats/packages")]
//        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseModel<PackagePurchaseStatsResponse>))]
//        public async Task<IActionResult> GetPackageStats([FromQuery] AdminTransactionSearch request, CancellationToken ct)
//        {
//            var data = await _transactionPaymentService.GetPackagePurchaseStatsAsync(request, ct);
//            return Ok(ResponseModel<PackagePurchaseStatsResponse>.Ok(
//                data: data,
//                message: "Get package purchase stats successfully"));
//        }

//        [Authorize(Roles = "Admin")]
//        [HttpGet("stats/revenue-monthly")]
//        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseModel<YearlyRevenueResponse>))]
//        public async Task<IActionResult> GetRevenueMonthly([FromQuery] int? year, CancellationToken ct)
//        {
//            var data = await _transactionPaymentService.GetMonthlyRevenueByYearAsync(year ?? DateTime.UtcNow.Year, "Success", ct);
//            return Ok(ResponseModel<YearlyRevenueResponse>.Ok(
//                data: data,
//                message: $"Get monthly revenue for {data.Year} successfully"));
//        }

//        [Authorize(Roles = "Admin")]
//        [HttpGet("stats/status-counts")]
//        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseModel<TransactionStatusCountsResponse>))]
//        public async Task<IActionResult> GetTransactionStatusCounts( CancellationToken ct)
//        {
//            var data = await _transactionPaymentService.CountTransactionByStatusAsync(ct);
//            return Ok(ResponseModel<TransactionStatusCountsResponse>.Ok(
//                data: data,
//                message: $"Transaction status counts retrieved successfully."));
//        }
//    }
//}
