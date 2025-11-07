

using Fusion.Repository.Bases.Exceptions;
using Fusion.Repository.Bases.Responses;
using Fusion.Repository.Data;
using Fusion.Repository.Entities;
using Fusion.Repository.Enums;
using Fusion.Service.IServices;
using Fusion.Service.ViewModels.TransactionPayment.Requests;
using Net.payOS;
using Net.payOS.Types;

namespace Fusion.Service.Services;


public class PayOSService : IPayOSService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly PayOS _payOS;
    private readonly ITransactionPaymentService _transactionPaymentService;
    private readonly ISubscriptionPlanService _subscriptionPlanService;
    public PayOSService(IUnitOfWork unitOfWork, PayOS payOS, ITransactionPaymentService transactionPaymentService, ISubscriptionPlanService subscriptionPlanService)
    {
        _unitOfWork = unitOfWork;
        _payOS = payOS;
        _transactionPaymentService = transactionPaymentService;
        _subscriptionPlanService = subscriptionPlanService;
    }

    private static string MapPayOsStatus(string payosStatus, bool webhookSuccess)
    {
        // Tùy tài liệu PayOS, ví dụ: "PAID", "PENDING", "CANCELLED", "EXPIRED"
        return payosStatus?.ToUpperInvariant() switch
        {
            "PAID" => PaymentStatus.Success.ToString(),
            "CANCELLED" => PaymentStatus.Cancelled.ToString(),
            "EXPIRED" => PaymentStatus.Failed.ToString(),
            "PENDING" => PaymentStatus.Pending.ToString(),
            _ => webhookSuccess ? PaymentStatus.Success.ToString() : PaymentStatus.Pending.ToString()
        };
    }

    /// <summary>
    /// Confirm webhook register with PayOS
    /// </summary>
    public async Task<string> ConfirmWebHook(string url)
    {
        var result = await _payOS.confirmWebhook(url);
        return result ?? "OK";
    }


    /// <summary>
    /// Create link payment PayOS
    /// </summary>
    public async Task<string> CreatePaymentLink(Guid transactionId, CancellationToken cancellationToken = default)
    {

        var tx = await _transactionPaymentService.GetDetailAsync(transactionId);
        if (tx == null)
            throw CustomExceptionFactory.CreateNotFoundError(ResponseMessages.NOT_FOUND.FormatMessage("Transaction"));

        if (tx.Amount <= 0)
            throw CustomExceptionFactory.CreateBadRequestError(ResponseMessages.BAD_REQUEST.FormatMessage("Amount <= 0"));

        if (!tx.OrderCode.HasValue)
        {
            var baseCode = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            var rnd = Random.Shared.Next(100, 999);
            tx.OrderCode = baseCode * 1000 + rnd;
        }

        var plan = await _subscriptionPlanService.GetPlanByIdAsync(tx.PlanId);
        if (plan == null)
            throw CustomExceptionFactory.CreateNotFoundError(ResponseMessages.NOT_FOUND.FormatMessage("subscription plan"));

        var amountVnd = Convert.ToInt32(Math.Round(tx.Amount, MidpointRounding.AwayFromZero));
        var description = (tx.OrderCode ?? 0).ToString();


        var returnUrl = $"http://localhost:5173/payment-success?transactionId={tx.Id}&orderCode={tx.OrderCode}";
        var cancelUrl = $"http://localhost:5173/payment-failed?transactionId={tx.Id}&orderCode={tx.OrderCode}";

        var items = new List<ItemData> {
            new ItemData(plan.Name, 1, amountVnd)
                                           };

        var paymentData = new PaymentData(
                         orderCode: tx.OrderCode.Value,
                         amount: amountVnd,
                         description: description,
                         returnUrl: returnUrl,
                         cancelUrl: cancelUrl,
                         items: items
                                          );

        var res = await _payOS.createPaymentLink(paymentData);

        await _transactionPaymentService.UpdateAsync(tx.Id, new TransactionPaymentUpdateRequest
        {
            OrderCode = tx.OrderCode,                 // ensure persisted
            PaymentLinkId = res.paymentLinkId,
            Description = res.description,  // PayOS có thể sửa; vẫn cắt 25
            Currency = res.currency

        }, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return res.checkoutUrl;

    }

    public async Task<string> RefreshStatusByGateway(long? orderCode, string? paymentLinkId, CancellationToken ct = default)
    {
        var repo = _unitOfWork.Repository<TransactionPayment>();

        TransactionPayment? tx = null;
        if (!string.IsNullOrWhiteSpace(paymentLinkId))
            tx = await repo.FindAsync(x => x.PaymentLinkId == paymentLinkId, ct);

        if (tx == null && orderCode.HasValue && orderCode.Value > 0)
            tx = await repo.FindAsync(x => x.OrderCode == orderCode.Value, ct);

        if (tx == null)
            throw new InvalidOperationException("Transaction not found to refresh.");

        await RefreshStatusFromGateway(tx, ct);
        await _unitOfWork.SaveChangesAsync(ct);
        return tx.Status;
    }

    /// <summary>
    /// User/admin cancel link payment
    /// </summary>
    public async Task<bool> CancelPaymentLink(Guid transactionId, string? reason = null, CancellationToken ct = default)
    {

        var tx = await _unitOfWork.Repository<TransactionPayment>().FindAsync(x => x.Id == transactionId, ct);
        if (tx == null) throw new InvalidOperationException("Transaction not found.");
        if (!tx.OrderCode.HasValue && string.IsNullOrWhiteSpace(tx.PaymentLinkId))
            throw new InvalidOperationException("Transaction has no payment link information.");


        if (tx.OrderCode.HasValue)
            await _payOS.cancelPaymentLink(tx.OrderCode.Value, reason ?? "User cancelled");

        // Sau khi huỷ trên gateway, refresh lại thông tin để chắc chắn
        await RefreshStatusFromGateway(tx, ct);
        await _unitOfWork.SaveChangesAsync(ct);
        return true;
    }

    private async Task RefreshStatusFromGateway(TransactionPayment tx, CancellationToken ct)
    {
        // Lấy thông tin từ PayOS
        dynamic info;
        if (tx.OrderCode.HasValue)
            info = await _payOS.getPaymentLinkInformation(tx.OrderCode.Value);
        else
            throw new InvalidOperationException("No orderCode or paymentLinkId to refresh.");

        // info.status, info.amount, info.currency ...
        var newStatus = MapPayOsStatus((string)info.status, webhookSuccess: false);
        tx.Status = newStatus;
        tx.Currency = info.currency;

        // Optionally sync amount/currency nếu có chênh
        if (info.amount is int gatewayAmount)
        {
            var dec = Convert.ToDecimal(gatewayAmount);
            if (tx.Amount != dec) tx.Amount = dec;
        }

    }

    /// <summary>
    /// Handle callback from PayOS
    /// </summary>
    public async Task HandlePaymentWebHook(WebhookType webhookData, CancellationToken cancellationToken = default)
    {

        var payload = webhookData.data;
        if (payload == null) return;


        TransactionPayment? tx = null;
        if (!string.IsNullOrWhiteSpace(payload.paymentLinkId))
        {
            tx = await _unitOfWork.Repository<TransactionPayment>().FindAsync(t => t.PaymentLinkId == payload.paymentLinkId, cancellationToken);
        }
        if (tx == null && payload.orderCode > 0)
        {
            tx = await _unitOfWork.Repository<TransactionPayment>().FindAsync(t => t.OrderCode == payload.orderCode, cancellationToken);
        }
        if (tx == null)
            return;

        DateTimeOffset? txTime = null;
        if (!string.IsNullOrWhiteSpace(payload.transactionDateTime) &&
            DateTimeOffset.TryParse(payload.transactionDateTime, out var parsed))
        {
            txTime = parsed;
        }

        tx.OrderCode = payload.orderCode;
        tx.Amount = payload.amount;
        tx.Description = payload.description;
        tx.AccountNumber = payload.accountNumber;
        tx.Reference = payload.reference;
        tx.TransactionDateTime = txTime;
        tx.Currency = payload.currency;
        tx.PaymentLinkId = payload.paymentLinkId;
        tx.CounterAccountBankId = payload.counterAccountBankId;
        tx.CounterAccountBankName = payload.counterAccountBankName;
        tx.CounterAccountName = payload.counterAccountName;
        tx.CounterAccountNumber = payload.counterAccountNumber;

        // status từ webhook
        tx.Status = MapPayOsStatus(payload.code, webhookData.success);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}