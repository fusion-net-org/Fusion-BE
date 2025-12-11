using Fusion.Repository.Bases.Exceptions;
using Fusion.Repository.Bases.Responses;
using Fusion.Repository.Data;
using Fusion.Repository.Entities;
using Fusion.Repository.Enums;
using Fusion.Repository.IRepositories;
using Fusion.Service.IServices;
using Fusion.Service.ViewModels.UserSubscription.Requests;
using Net.payOS;
using Net.payOS.Types;

namespace Fusion.Service.Services;


public class PayOSService : IPayOSService
{
    private readonly IUnitOfWork _uow;
    private readonly PayOS _payOS;
    private readonly ITransactionPaymentRepository _txRepo;
    private readonly ISubscriptionPlanService _planService;
    private readonly IUserSubscriptionService _userSubService;
    public PayOSService(IUnitOfWork unitOfWork, PayOS payOS, ITransactionPaymentRepository txRepo,
        ISubscriptionPlanService subscriptionPlanService, IUserSubscriptionService userSubService)
    {
        _uow = unitOfWork;
        _payOS = payOS;
        _txRepo = txRepo;
        _planService = subscriptionPlanService;
        _userSubService = userSubService;
    }

    private static PaymentStatus MapPayOsStatus(string? status)
    {
        switch (status?.ToUpperInvariant())
        {
            case "PAID": return PaymentStatus.Success;
            case "CANCELLED": return PaymentStatus.Cancelled;
            case "EXPIRED": return PaymentStatus.Failed;
            case "PENDING":
            default: return PaymentStatus.Pending;
        }
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

        var tx = await _txRepo.GetByIdWithNavAsync(transactionId);
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

        var returnUrl = $"https://www.fusion.info.vn/payment-success?transactionId={tx.Id}&orderCode={tx.OrderCode}";
        var cancelUrl = $"https://www.fusion.info.vn/payment-failed?transactionId={tx.Id}&orderCode={tx.OrderCode}";

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

        await _txRepo.AttachPaymentLinkAsync(tx.Id, tx.OrderCode.Value, res.paymentLinkId, "PayOS", cancellationToken);

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
            await _txRepo.MarkSuccessAsync(
                tx.Id,
                amount: paidAmount,     // hoặc null nếu không muốn override
                paidAt: paidAt,
                reference: reference,
                ct: ct
            );
        }
        else if (mapped == PaymentStatus.Cancelled || mapped == PaymentStatus.Failed)
        {
            await _txRepo.MarkFailedAsync(
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

        // 1) Tìm transaction theo paymentLinkId trước, rồi fallback orderCode
        var repo = _uow.Repository<TransactionPayment>();
        TransactionPayment? tx = null;

        if (!string.IsNullOrWhiteSpace(payload.paymentLinkId))
            tx = await repo.FindAsync(t => t.PaymentLinkId == payload.paymentLinkId, ct);

        if (tx == null && payload.orderCode > 0)
            tx = await repo.FindAsync(t => t.OrderCode == payload.orderCode, ct);

        if (tx == null) return;

        // 2) Lấy PaymentLinkInformation làm nguồn sự thật
        long oc = tx.OrderCode ?? payload.orderCode;
        if (oc <= 0) return;

        try
        {
            var info = await _payOS.getPaymentLinkInformation(oc);
            var mapped = MapPayOsStatus((string)info.status);

            // 3) Lấy transaction item gần nhất để rút meta (paidAt/desc/reference/account…)
            Transaction? lastTxn = null;
            if (info.transactions != null && info.transactions.Count > 0)
            {
                lastTxn = info.transactions
                             .OrderByDescending(t =>
                                 DateTimeOffset.TryParse(t.transactionDateTime, out var ts) ? ts : DateTimeOffset.MinValue)
                             .FirstOrDefault();
            }

            // paidAt: ưu tiên từ item; nếu không parse được thì dùng UtcNow
            var paidAt = DateTimeOffset.UtcNow;
            if (lastTxn != null && DateTimeOffset.TryParse(lastTxn.transactionDateTime, out var parsedPaidAt))
                paidAt = parsedPaidAt;

            // amount theo giao dịch gần nhất (không dùng amountPaid tổng)
            decimal? paidAmount = lastTxn != null ? Convert.ToDecimal(lastTxn.amount) : (decimal?)null;

            // Lý do/ghi chú khi fail/cancel
            string? reason = info.cancellationReason ?? lastTxn?.description ?? payload.description;

            // 4) Chuyển trạng thái giao dịch theo gateway
            if (mapped == PaymentStatus.Success)
            {
                // 4.a Đánh dấu success + cập nhật meta ngân hàng
                await _txRepo.MarkSuccessAsync(
                    tx.Id,
                    amount: paidAmount,         
                    paidAt: paidAt,
                    reference: lastTxn?.reference,
                    ct: ct
                );

                if (lastTxn != null)
                {
                    tx.AccountNumber = lastTxn.accountNumber ?? tx.AccountNumber;
                    tx.Reference = lastTxn.reference ?? tx.Reference;
                    tx.Description = lastTxn.description ?? tx.Description;
                    tx.CounterAccountBankId = lastTxn.counterAccountBankId ?? tx.CounterAccountBankId;
                    tx.CounterAccountBankName = lastTxn.counterAccountBankName ?? tx.CounterAccountBankName;
                    tx.CounterAccountName = lastTxn.counterAccountName ?? tx.CounterAccountName;
                    tx.CounterAccountNumber = lastTxn.counterAccountNumber ?? tx.CounterAccountNumber;
                    // Currency không có trong PaymentLinkInformation → giữ nguyên tx.Currency
                }

                // 4.b Phân nhánh logic theo PaymentModeSnapshot
                if (tx.PaymentModeSnapshot == PaymentMode.Prepaid)
                {
                        // Tạo subscription mới
                        var created = await _userSubService.CreateAsync(new UserSubscriptionCreateRequest
                        {
                            TransactionId = tx.Id
                        }, ct);

                        tx.UserSubscriptionId = created.Id;
                    //  sẽ pause các UserSub cũ + cascade sang CompanySub liên quan
                    await _userSubService.PauseOtherActiveByUserAsync(tx.UserId, created.Id, ct);

                }
                else // INSTALLMENTS
                {
                    // Kỳ 1: thanh toán đầu tiên -> tạo subscription + gắn cho toàn bộ installment cùng batch
                    if (tx.InstallmentIndex == null || tx.InstallmentIndex == 1)
                    {
                        var created = await _userSubService.CreateAsync(new UserSubscriptionCreateRequest
                        {
                            TransactionId = tx.Id
                        }, ct);

                        //  Gắn UserSubscriptionId cho transaction hiện tại
                        tx.UserSubscriptionId = created.Id;

                        await _userSubService.PauseOtherActiveByUserAsync(tx.UserId, created.Id, ct);
                        // Gắn luôn subscription này cho toàn bộ installment charge cùng user + plan
                        await _txRepo.AttachSubscriptionToInstallmentBatchAsync(
                            tx.UserId,
                            tx.PlanId,
                            created.Id,
                            ct);

                        var next = await _txRepo.FindNextPendingInstallmentAsync(
                             tx.UserId,
                             tx.PlanId,
                             created.Id,                 
                             tx.InstallmentIndex ?? 1,
                             ct);

                        await _userSubService.UpdateNextDueAsync(created.Id, next?.DueAt, ct);
                    }
                    else
                    {
                        // Kỳ 2..N: chỉ cần update NextDue cho subscription hiện hữu
                        Guid? subId = tx.UserSubscriptionId;

                        if (!subId.HasValue)
                        {
                            // Fallback: tìm Active cùng plan
                            var subRepo = _uow.Repository<UserSubscription>();
                            var sub = await subRepo.FindAsync(s =>
                                s.UserId == tx.UserId &&
                                s.PlanId == tx.PlanId &&
                                s.Id == tx.UserSubscriptionId &&
                                s.Status == SubscriptionStatus.Active, ct);

                            if (sub != null)
                            {
                                subId = sub.Id;

                                // Đảm bảo transaction này cũng được link về sub đó
                                tx.UserSubscriptionId = sub.Id;
                            }
                        }

                        if (subId.HasValue)
                        {
                            var next = await _txRepo.FindNextPendingInstallmentAsync(
                                tx.UserId,
                                tx.PlanId,
                                subId,
                                tx.InstallmentIndex  ?? 1,
                                ct);

                            await _userSubService.UpdateNextDueAsync(subId.Value, next?.DueAt, ct);
                        }
                    }
                }
            }
            else if (mapped == PaymentStatus.Cancelled || mapped == PaymentStatus.Failed)
            {
                // 4.c Đánh dấu failed/cancel + ghi meta (để điều tra)
                await _txRepo.MarkFailedAsync(
                    tx.Id,
                    description: reason,
                    reference: lastTxn?.reference,
                    ct: ct
                );

                if (lastTxn != null)
                {
                    tx.Reference = lastTxn.reference ?? tx.Reference;
                    tx.Description = lastTxn.description ?? tx.Description;
                    tx.AccountNumber = lastTxn.accountNumber ?? tx.AccountNumber;

                    tx.CounterAccountBankId = lastTxn.counterAccountBankId ?? tx.CounterAccountBankId;
                    tx.CounterAccountBankName = lastTxn.counterAccountBankName ?? tx.CounterAccountBankName;
                    tx.CounterAccountName = lastTxn.counterAccountName ?? tx.CounterAccountName;
                    tx.CounterAccountNumber = lastTxn.counterAccountNumber ?? tx.CounterAccountNumber;
                }
                // mapped == Pending => không đổi gì
            }

            // 5) Lưu
            await _uow.SaveChangesAsync(ct);
        }
        catch (Exception ex)
        {

            throw;
        }
        // ===== Local helper =====
        static DateTimeOffset AddInterval(DateTimeOffset start, BillingPeriod period, int count)
            => period switch
            {
                BillingPeriod.Week => start.AddDays(7 * count),
                BillingPeriod.Month => start.AddMonths(count),
                BillingPeriod.Year => start.AddYears(count),
                _ => start
            };
    }
}