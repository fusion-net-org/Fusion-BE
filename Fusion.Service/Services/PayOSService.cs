
using Fusion.Repository.Bases.Exceptions;
using Fusion.Repository.Bases.Responses;
using Fusion.Repository.Data;
using Fusion.Repository.Entities;
using Fusion.Service.IServices;
using Fusion.Service.ViewModels.UserSubscription.Requests;
using Net.payOS;
using Net.payOS.Types;

namespace Fusion.Service.Services
{
    public class PayOSService : IPayOSService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly PayOS _payOS;
        private readonly ITransactionPaymentService _transactionPaymentService;
        private readonly IUserSubscriptionService _userSubscriptionService;

        public PayOSService(IUnitOfWork unitOfWork, PayOS payOS, ITransactionPaymentService transactionPaymentService,
            IUserSubscriptionService userSubscriptionService)
        {
            _unitOfWork = unitOfWork;
            _payOS = payOS;
            _transactionPaymentService = transactionPaymentService;
            _userSubscriptionService = userSubscriptionService;
        }
        /// <summary>
        /// Confirm webhook register with PayOS
        /// </summary>
        public async Task<string> ConfirmWebHook(string url)
        {
            try
            {
                Console.WriteLine($"Confirming webhook for URL: {url}");
                var result = await _payOS.confirmWebhook(url);
                Console.WriteLine($"Webhook confirmation result: {result}");
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Confirm webhook failed: {ex}");
                throw;

            }
        }


        /// <summary>
        /// Create link payment PayOS
        /// </summary>
        public async Task<string> CreatePaymentLink(Guid transactionId, CancellationToken cancellationToken = default)
        {
            try
            {
                var transaction = await _unitOfWork.Repository<TransactionPayment>().FindAsync(x => x.Id == transactionId);
                if (transaction == null)
                    throw CustomExceptionFactory.CreateNotFoundError(
                        string.Format(ResponseMessages.NOT_FOUND, "Transaction"));

                var subscriptionPackage = await _unitOfWork.Repository<SubscriptionPackage>().FindAsync(x => x.Id == transaction.PackageId);
                if (subscriptionPackage == null)
                    throw CustomExceptionFactory.CreateNotFoundError(
                        string.Format(ResponseMessages.NOT_FOUND, "Subscription package"));

                // paper request send to Payos 
                var item = new ItemData(
                    name: subscriptionPackage.Name,
                    quantity: 1,
                    price: (int)transaction.Amount
                    );

                var request = new PaymentData(
                    orderCode: long.Parse(transaction.TransactionCode),
                    amount: (int)transaction.Amount,
                    description: $"Fusion - {transaction.SubscriptionPackage?.Name}",
                    returnUrl: "http://localhost:5173/payment-success",
                    cancelUrl: "http://localhost:5173/payment-failed",
                    items: new List<ItemData>() { item }
                    );

                var response = await _payOS.createPaymentLink(request);

                transaction.PaymentMethod = "PayOS";
                await _unitOfWork.SaveChangesAsync();

                return response.checkoutUrl;
            }

            catch (Exception ex)
            {
                throw CustomExceptionFactory.CreateBadRequestError(
                    string.Format(ResponseMessages.ERROR,
                    "Create payment link failed!"));
            }
        }

        /// <summary>
        /// Handle callback from PayOS
        /// </summary>
        public async Task HandlePaymentWebHook(WebhookType webhookData, CancellationToken cancellationToken = default)
        {
            //1.Get transactionCode from data 

            
            var transctionCode = webhookData.data.orderCode.ToString();

            var transaction = await _unitOfWork.Repository<TransactionPayment>().FindAsync(x => x.TransactionCode == transctionCode);
            if (transaction == null)
            {
                // Đây thường xảy ra khi PayOS gọi test webhook sau confirm
                Console.WriteLine($"[Webhook] Transaction not found for orderCode: {transctionCode}");
                return;
            }
            // 3. Check idempotent
            if (transaction.Status == "Success" )
                return;

            // 4. Update trạng thái
            transaction.Status = webhookData.success ? "Success" : "Failed";
            transaction.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.Repository<TransactionPayment>().Update(transaction);
            await _unitOfWork.SaveChangesAsync();

            // Nếu success thì tạo UserSubscription
            if (webhookData.success)
            {
                var subscriptionPackage = await _unitOfWork.Repository<SubscriptionPackage>()
                    .FindAsync(x => x.Id == transaction.PackageId);

                if (subscriptionPackage == null)
                    return;

                var createRequest = new CreateUserSubscriptionRequest
                {
                    PackageId = subscriptionPackage.Id,
                    PurchaseDate = DateTime.UtcNow,
                    QuotaCompanyAdded = subscriptionPackage.QuotaCompany,
                    QuotaProjectAdded = subscriptionPackage.QuotaProject
                };

                await _userSubscriptionService.CreateUserSubscriptionAsync(transaction.UserId, createRequest, cancellationToken);
            }
        }
    }
}
