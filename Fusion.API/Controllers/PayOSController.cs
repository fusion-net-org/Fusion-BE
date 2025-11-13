//using Fusion.Repository.Bases.Responses;
//using Fusion.Service.Commons.BaseResponses;
//using Fusion.Service.IServices;
//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Mvc;
//using Net.payOS.Types;

//namespace Fusion.API.Controllers
//{
//    [Route("api/[controller]")]
//    [ApiController]
//    public class PayOSController : ControllerBase
//    {

//        private readonly IPayOSService _payOSService;

//        public PayOSController(IPayOSService payOSService)
//        {
//            _payOSService = payOSService;
//        }

//        /// <summary>
//        /// Confirm webhook (dùng khi đăng ký webhook với PayOS)
//        /// </summary>
//        [HttpPost("confirm-webhook")]
//        public async Task<IActionResult> ConfirmWebHook([FromQuery] string url)
//        {
//            try
//            {
//                var result = await _payOSService.ConfirmWebHook(url);
//                return Ok(new
//                {
//                    succeeded = true,
//                    message = "Webhook confirmed successfully!",
//                    data = result
//                });
//            }
//            catch (Exception ex)
//            {
//                return BadRequest(new
//                {
//                    succeeded = false,
//                    message = $"Confirm webhook failed: {ex.Message}"
//                });
//            }
//        }

//        /// <summary>
//        /// create payment link for transaction
//        /// </summary>
//        [Authorize]
//        [HttpPost("{transactionId:guid}/create-link")]
//        public async Task<IActionResult> CreatePaymentLink(Guid transactionId)
//        {
//            var checkoutUrl = await _payOSService.CreatePaymentLink(transactionId);
//            return Ok(ResponseModel<string>.Ok(
//                data: checkoutUrl,
//                message: ResponseMessageHelper.FormatMessage(ResponseMessages.CREATE_SUCCESS, "Payment link")
//            ));
//        }

//        /// <summary>
//        /// Webhook callback from PayOS
//        /// </summary>
//        [HttpPost("webhook")]
//        public async Task<IActionResult> HandleWebHook([FromBody] WebhookType webhook)
//        {
//            await _payOSService.HandlePaymentWebHook(webhook);

//            return Ok(ResponseModel<string>.Ok(
//                data: "Webhook processed",
//                message: ResponseMessageHelper.FormatMessage(ResponseMessages.UPDATE_SUCCESS, "Transaction payment status")
//        ));
//        }

//        [AllowAnonymous]
//        [HttpPost("refresh-status")]
//        public async Task<IActionResult> RefreshStatus([FromQuery] long? orderCode, [FromQuery] string? paymentLinkId)
//        {
//            var status = await _payOSService.RefreshStatusByGateway(orderCode, paymentLinkId);
//            return Ok(ResponseModel<string>.Ok(
//                data: status,
//                message: ResponseMessageHelper.FormatMessage(ResponseMessages.UPDATE_SUCCESS, "Transaction payment status")
//            ));
//        }
//    }
//}
