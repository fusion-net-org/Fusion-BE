using Fusion.Repository.Bases.Exceptions;
using Fusion.Repository.Bases.Responses;
using Fusion.Repository.Data;
using Fusion.Repository.Entities;
using Fusion.Repository.Enums;
using Fusion.Service.IServices;
using Fusion.Service.ViewModels.TransactionPayment.Requests;
using Fusion.Service.ViewModels.UserSubscription.Requests;
using Net.payOS;
using Net.payOS.Types;

namespace Fusion.Service.Services;


public class PayOSService : IPayOSService
{
    private readonly IUnitOfWork _uow;
    private readonly PayOS _payOS;
    private readonly ITransactionPaymentService _txService;
    private readonly ISubscriptionPlanService _planService;
    //private readonly IUserSubscriptionService _userSubscriptionService;
    public PayOSService(IUnitOfWork unitOfWork, PayOS payOS, ITransactionPaymentService transactionPaymentService, ISubscriptionPlanService subscriptionPlanService)
    {
        _uow = unitOfWork;
        _payOS = payOS;
        _txService = transactionPaymentService;
        _planService = subscriptionPlanService;
        //_userSubscriptionService = userSubscriptionService;
    }

    private static PaymentStatus MapPayOsStatus(string? status) => status?.ToUpperInvariant() switch
    {
        "PAID" => PaymentStatus.Success,
        "CANCELLED" => PaymentStatus.Cancelled,
        "EXPIRED" => PaymentStatus.Failed,
        "PENDING" => PaymentStatus.Pending,
        _ => PaymentStatus.Pending
    };
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

        var tx = await _txService.GetDetailAsync(transactionId);
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

        // Lấy plan để đặt item name (không dùng để tính amount vì amount đã snapshot)
        var plan = await _planService.GetPlanByIdAsync(tx.PlanId, cancellationToken);
        if (plan == null)
            throw CustomExceptionFactory.CreateNotFoundError(ResponseMessages.NOT_FOUND.FormatMessage("subscription plan"));

        var amountVnd = Convert.ToInt32(Math.Round(tx.Amount, MidpointRounding.AwayFromZero));
        var description = (tx.OrderCode ?? 0).ToString(); // PayOS description ngắn/gọn

        var returnUrl = $"http://localhost:5173/payment-success?transactionId={tx.Id}&orderCode={tx.OrderCode}";
        var cancelUrl = $"http://localhost:5173/payment-failed?transactionId={tx.Id}&orderCode={tx.OrderCode}";

        var items = new List<ItemData> { new ItemData(plan.Name, 1, amountVnd) };

        var paymentData = new PaymentData(
            orderCode: tx.OrderCode!.Value,
            amount: amountVnd,
            description: description,
            returnUrl: returnUrl,
            cancelUrl: cancelUrl,
            items: items
        );

        var res = await _payOS.createPaymentLink(paymentData);

        await _txService.AttachPaymentLinkAsync(tx.Id, tx.OrderCode.Value, res.paymentLinkId, "PayOS", cancellationToken);

        return res.checkoutUrl;

    }

    /// <summary>Refresh trạng thái bằng cách gọi PayOS theo orderCode / paymentLinkId.</summary>
    public async Task<string> RefreshStatusByGateway(long? orderCode, string? paymentLinkId, CancellationToken ct = default)
    {
        // Tìm transaction bằng orderCode hoặc paymentLinkId — dùng UnitOfWork để đọc nhanh
        var repo = _uow.Repository<TransactionPayment>();
        TransactionPayment? tx = null;

        if (!string.IsNullOrWhiteSpace(paymentLinkId))
            tx = await repo.FindAsync(x => x.PaymentLinkId == paymentLinkId, ct);

        if (tx == null && orderCode.HasValue && orderCode.Value > 0)
            tx = await repo.FindAsync(x => x.OrderCode == orderCode.Value, ct);

        if (tx == null)
            throw new InvalidOperationException("Transaction not found to refresh.");

        await RefreshStatusFromGateway(tx, ct);
        await _uow.SaveChangesAsync(ct);
        return tx.Status.ToString();
    }


    private async Task RefreshStatusFromGateway(TransactionPayment tx, CancellationToken ct)
    {
        if (!tx.OrderCode.HasValue)
            throw new InvalidOperationException("No orderCode to refresh.");

        var info = await _payOS.getPaymentLinkInformation(tx.OrderCode.Value);
        var mapped = MapPayOsStatus((string)info.status);

        // Lấy transaction mới nhất (nếu có)
        var last = info.transactions != null && info.transactions.Count > 0
            ? info.transactions[^1] // phần tử cuối danh sách
            : null;

        // Parse thời điểm thanh toán nếu có, fallback UtcNow
        DateTimeOffset paidAt = DateTimeOffset.UtcNow;
        if (last?.transactionDateTime is string txTimeStr &&
            DateTimeOffset.TryParse(txTimeStr, out var parsedTime))
        {
            paidAt = parsedTime;
        }

        // Thường PayOS trả amount là int (VND) -> ép sang decimal cho model của bạn
        decimal? paidAmount = null;
        if (last != null)
        {
            try { paidAmount = Convert.ToDecimal(last.amount); } catch { /* ignore */ }
        }

        var reference = last?.reference;
        var description = last?.description;

        if (mapped == PaymentStatus.Success)
        {
            await _txService.MarkSuccessAsync(
                tx.Id,
                amount: paidAmount,     // hoặc null nếu không muốn override
                paidAt: paidAt,
                reference: reference,
                ct: ct
            );
        }
        else if (mapped == PaymentStatus.Cancelled || mapped == PaymentStatus.Failed)
        {
            await _txService.MarkFailedAsync(
                tx.Id,
                description: description,
                reference: reference,
                ct: ct
            );
        }
        // Pending => không đổi trạng thái
    }

    /// <summary>
    /// Handle callback from PayOS
    /// </summary>
    public async Task HandlePaymentWebHook(WebhookType webhookData, CancellationToken ct = default)
    {

        var payload = webhookData.data;
        if (payload == null) return;


        // Tìm transaction
        var repo = _uow.Repository<TransactionPayment>();
        TransactionPayment? tx = null;

        if (!string.IsNullOrWhiteSpace(payload.paymentLinkId))
            tx = await repo.FindAsync(t => t.PaymentLinkId == payload.paymentLinkId, ct);

        if (tx == null && payload.orderCode > 0)
            tx = await repo.FindAsync(t => t.OrderCode == payload.orderCode, ct);

        if (tx == null) return;


        // Lấy trạng thái từ gateway làm nguồn sự thật
        var info = await _payOS.getPaymentLinkInformation(payload.orderCode);
        var mapped = MapPayOsStatus((string)info.status);
        DateTimeOffset paidAt = DateTimeOffset.UtcNow;

        if (!string.IsNullOrWhiteSpace(payload.transactionDateTime) &&
            DateTimeOffset.TryParse(payload.transactionDateTime, out var parsed))
        {
            paidAt = parsed;
        }

        if (mapped == PaymentStatus.Success)
        {
            await _txService.MarkSuccessAsync(tx.Id, amount: payload.amount, paidAt: paidAt, reference: payload.reference, ct);

            // Tạo UserSubscription chỉ khi:
            // - Prepaid (InstallmentIndex == null) HOẶC
            // - Installments kỳ đầu tiên (InstallmentIndex == 1)
            if (tx.InstallmentIndex == null || tx.InstallmentIndex == 1)
            {
                var subReq = new UserSubscriptionCreateRequest
                {
                    TransactionId = tx.Id
                };
                //await _userSubService.CreateAsync(subReq, ct);
            }
        }
        else if (mapped == PaymentStatus.Cancelled || mapped == PaymentStatus.Failed)
        {
            await _txService.MarkFailedAsync(tx.Id, description: payload.description, reference: payload.reference, ct);
        }

        await _uow.SaveChangesAsync(ct);
    }
}