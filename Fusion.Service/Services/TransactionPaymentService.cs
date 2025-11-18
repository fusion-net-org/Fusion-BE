
using AutoMapper;
using Fusion.Repository.Bases.Exceptions;
using Fusion.Repository.Bases.Page;
using Fusion.Repository.Bases.Page.TransactionPayment;
using Fusion.Repository.Bases.Responses;
using Fusion.Repository.Entities;
using Fusion.Repository.Enums;
using Fusion.Repository.IRepositories;
using Fusion.Service.Commons.Helpers;
using Fusion.Service.IServices;
using Fusion.Service.ViewModels.TransactionPayment.Requests;
using Fusion.Service.ViewModels.TransactionPayment.Responses;

namespace Fusion.Service.Services;

public class TransactionPaymentService : ITransactionPaymentService
{
    private readonly ITransactionPaymentRepository _txRepo;
    private readonly ISubscriptionPlanRepository _planRepo;
    private readonly IMapper _mapper;
    private readonly ICurrentService _current;

    public TransactionPaymentService(ITransactionPaymentRepository transactionPaymentRepository, ISubscriptionPlanRepository subscriptionPlanRepository,
      IMapper mapper, ICurrentService currentService)
    {
        _txRepo = transactionPaymentRepository;
        _planRepo = subscriptionPlanRepository;
        _mapper = mapper;
        _current = currentService;
    }

    public async Task<TransactionPaymentDetailResponse> CreateAsync(TransactionPaymentCreateRequest req, CancellationToken ct = default)
    {
        if (req == null || req.PlanId == Guid.Empty)
            throw CustomExceptionFactory.CreateBadRequestError(ResponseMessages.INVALID_INPUT);

        var plan = await _planRepo.GetByIdWithNavAsync(req.PlanId, ct);
        if (plan == null)
            throw CustomExceptionFactory.CreateNotFoundError("Subscription plan not found.");
        if (plan.Price == null)
            throw CustomExceptionFactory.CreateBadRequestError("Plan has no price.");


        var userId = _current.GetUserId();
        var price = plan.Price;

        // Snapshot fields
        var now = DateTimeOffset.UtcNow;

        if (price.PaymentMode == PaymentMode.Prepaid)
        {
            var draft = new TransactionPayment
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                PlanId = plan.Id,
                Status = PaymentStatus.Pending,
                Type = TransactionType.Charge,
                CreatedAt = now,

                // Snapshot pricing
                ChargeUnitSnapshot = price.ChargeUnit,
                BillingPeriodSnapshot = price.BillingPeriod,
                PeriodCountSnapshot = price.PeriodCount,
                PaymentModeSnapshot = price.PaymentMode,

                Amount = price.Price,
                Currency = price.Currency
            };

            var created = await _txRepo.CreateDraftChargeAsync(draft, ct);
            var withNav = await _txRepo.GetByIdWithNavAsync(created.Id, ct);
            return _mapper.Map<TransactionPaymentDetailResponse>(withNav!);
        }
        else // ================= INSTALLMENTS =================
        {
            var total = price.Price;
            var n = price.InstallmentCount ?? 0;
            if (n <= 1)
                throw CustomExceptionFactory.CreateBadRequestError("InstallmentCount must be > 1 for installments.");

            //  LẤY DANH SÁCH DISCOUNT TỪ PLAN PRICE
            var discounts = price.Discounts?.ToList() ?? new List<SubscriptionPlanPriceDiscount>();

            // 1) Quy toàn bộ thời gian gói về "tháng" hoặc "tuần"
            bool useMonths = price.BillingPeriod == BillingPeriod.Month
                             || price.BillingPeriod == BillingPeriod.Year;

            int totalUnits;
            if (useMonths)
            {
                totalUnits = price.BillingPeriod switch
                {
                    BillingPeriod.Month => price.PeriodCount,
                    BillingPeriod.Year => price.PeriodCount * 12,
                    _ => throw CustomExceptionFactory.CreateBadRequestError("Unsupported BillingPeriod for installments.")
                };
            }
            else
            {
                totalUnits = price.PeriodCount; 
            }

            var unitsPerInstallment = (double)totalUnits / n;

            var rows = new List<TransactionPayment>(capacity: n);

            decimal basePerRaw = total / n;
            decimal sumBase = 0m;

            for (int i = 1; i <= n; i++)
            {
                // Base amount trước giảm
                decimal baseAmount;
                if (i == n)
                {
                    baseAmount = total - sumBase;
                }
                else
                {
                    baseAmount = Math.Round(basePerRaw, 2, MidpointRounding.AwayFromZero);
                    sumBase += baseAmount;
                }

                // 👇 Tìm discount config cho kỳ i (nếu có)
                var discountCfg = discounts.FirstOrDefault(d => d.InstallmentIndex == i);
                decimal finalAmount = baseAmount;

                if (discountCfg != null && discountCfg.DiscountValue > 0)
                {
                    var percent = discountCfg.DiscountValue / 100m; // 10 => 0.10
                    finalAmount = Math.Round(
                        baseAmount * (1 - percent),
                        2,
                        MidpointRounding.AwayFromZero
                    );
                }

                int offsetUnits = (int)Math.Round(unitsPerInstallment * (i - 1));
                DateTimeOffset dueAt;
                if (useMonths)
                {
                    dueAt = now.AddMonths(offsetUnits);
                }
                else
                {
                    dueAt = now.AddDays(offsetUnits * 7);
                }

                rows.Add(new TransactionPayment
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    PlanId = plan.Id,
                    Status = PaymentStatus.Pending,
                    Type = TransactionType.Charge,
                    CreatedAt = now,

                    ChargeUnitSnapshot = price.ChargeUnit,
                    BillingPeriodSnapshot = price.BillingPeriod,
                    PeriodCountSnapshot = price.PeriodCount,
                    PaymentModeSnapshot = price.PaymentMode,
                    InstallmentIndex = i,
                    InstallmentTotal = n,

                    Amount = finalAmount,
                    Currency = price.Currency,
                    DueAt = dueAt
                });
            }

            await _txRepo.BulkCreateAsync(rows, ct);
            var first = rows[0];
            var withNav = await _txRepo.GetByIdWithNavAsync(first.Id, ct);
            return _mapper.Map<TransactionPaymentDetailResponse>(withNav!);
        }
    }
    public Task<TransactionPaymentDetailResponse?> GetDetailAsync(Guid id, CancellationToken ct = default)
      => _txRepo.GetByIdWithNavAsync(id, ct).ContinueWith(t =>
          t.Result == null ? null : _mapper.Map<TransactionPaymentDetailResponse>(t.Result), ct);

    public async Task<PagedResult<TransactionPaymentResponse>> GetPagedAsync(TransactionPaymentPagedRequest request, CancellationToken ct = default)
    {
        var paged = await _txRepo.GetPagedAsync(request, ct);
        return new PagedResult<TransactionPaymentResponse>
        {
            Items = _mapper.Map<List<TransactionPaymentResponse>>(paged.Items),
            PageNumber = paged.PageNumber,
            PageSize = paged.PageSize,
            TotalCount = paged.TotalCount
        };
    }

    public Task<bool> AttachPaymentLinkAsync(Guid transactionId, long orderCode, string paymentLinkId, string? provider, CancellationToken ct = default)
    => _txRepo.AttachPaymentLinkAsync(transactionId, orderCode, paymentLinkId, provider, ct);

    public Task<bool> MarkSuccessAsync(Guid transactionId, decimal? amount, DateTimeOffset paidAt, string? reference, CancellationToken ct = default)
        => _txRepo.MarkSuccessAsync(transactionId, amount, paidAt, reference, ct);

    public Task<bool> MarkFailedAsync(Guid transactionId, string? description, string? reference, CancellationToken ct = default)
        => _txRepo.MarkFailedAsync(transactionId, description, reference, ct);

    public async Task<TransactionPaymentDetailResponse?> FindEarliestPendingInstallmentAsync(
     Guid planId,
     Guid? userSubscriptionId = null,
     CancellationToken ct = default)
    {
        if (planId == Guid.Empty)
            throw CustomExceptionFactory.CreateBadRequestError(ResponseMessages.INVALID_INPUT);

        var userId = _current.GetUserId();

        // Gọi repo để lấy transaction kỳ tiếp theo
        var tx = await _txRepo.FindEarliestPendingInstallmentAsync(
            userId,
            planId,
            userSubscriptionId,
            ct);

        if (tx == null)
            throw CustomExceptionFactory.CreateBadRequestError(
                "Your subscription has been fully paid." );


        var withNav = await _txRepo.GetByIdWithNavAsync(tx.Id, ct) ?? tx;

        return _mapper.Map<TransactionPaymentDetailResponse>(withNav);
    }
    private static DateTimeOffset AddInterval(DateTimeOffset start, BillingPeriod interval, int steps)
    {
        return interval switch
        {
            BillingPeriod.Week => start.AddDays(7 * steps),
            BillingPeriod.Month => start.AddMonths(steps),
            BillingPeriod.Year => start.AddYears(steps),
            _ => start
        };
    }

    public async Task<List<TransactionPaymentResponse>> GetDueAsync(DateTimeOffset asOf, int take = 100, CancellationToken ct = default)
    {
        var items = await _txRepo.GetDueAsync(asOf, take, ct);
        return _mapper.Map<List<TransactionPaymentResponse>>(items);
    }
}
