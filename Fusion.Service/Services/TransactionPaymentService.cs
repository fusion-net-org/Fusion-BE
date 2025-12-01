
using AutoMapper;
using Fusion.Repository.Bases.Exceptions;
using Fusion.Repository.Bases.Page.TransactionPayment;
using Fusion.Repository.Bases.Responses;
using Fusion.Repository.Entities;
using Fusion.Repository.Enums;
using Fusion.Repository.IRepositories;
using Fusion.Repository.ViewModels.SubscriptionPlan;
using Fusion.Repository.ViewModels.Transactions;
using Fusion.Service.Commons.Helpers;
using Fusion.Service.IServices;
using Fusion.Service.ViewModels.SubscriptionPlan.Responses;
using Fusion.Service.ViewModels.TransactionPayment.Requests;
using Fusion.Service.ViewModels.TransactionPayment.Responses;
using Fusion.Service.ViewModels.TransactionPayment.Responses.Overview;

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
            decimal contractAmount = price.NewPrice;
            if (contractAmount <= 0m
           && price.Price > 0m
           && (price.Discounts == null || !price.Discounts.Any(d => d.DiscountValue > 0m)))
            {
                contractAmount = price.Price;
            }


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

                Amount = contractAmount,
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

    public async Task<TransactionPaymentPagedSummaryResponse> GetPagedAsync(TransactionPaymentPagedRequest request, CancellationToken ct = default)
    {
        var paged = await _txRepo.GetPagedAsync(request, ct);

        return new TransactionPaymentPagedSummaryResponse
        {
            Items = _mapper.Map<List<TransactionPaymentResponse>>(paged.Items),
            PageNumber = paged.PageNumber,
            PageSize = paged.PageSize,
            TotalCount = paged.TotalCount,

            TotalTransactions = paged.TotalTransactions,
            TotalRevenue = paged.TotalRevenue,
            TotalSuccess = paged.TotalSuccess,
            TotalFailed = paged.TotalFailed,
            TotalPending = paged.TotalPending,
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

        var plan = await _planRepo.GetByIdWithNavAsync(planId, ct);
        if (plan == null)
        {
            throw CustomExceptionFactory.CreateNotFoundError(
                ResponseMessages.NOT_FOUND.FormatMessage("Subscription plan"));
        }

        if (!plan.IsActive)
        {
            throw CustomExceptionFactory.CreateBadRequestError(
                "This subscription plan is Paused. You cannot continue paying installments for it.");
        }

        var userId = _current.GetUserId();

        // Gọi repo để lấy transaction kỳ tiếp theo
        var tx = await _txRepo.FindEarliestPendingInstallmentAsync(
            userId,
            planId,
            userSubscriptionId,
            ct);

        if (tx == null)
            throw CustomExceptionFactory.CreateBadRequestError(
                "Your subscription has been fully paid.");


        var withNav = await _txRepo.GetByIdWithNavAsync(tx.Id, ct) ?? tx;

        return _mapper.Map<TransactionPaymentDetailResponse>(withNav);
    }

    public async Task<List<TransactionPaymentResponse>> GetDueAsync(DateTimeOffset asOf, int take = 100, CancellationToken ct = default)
    {
        var items = await _txRepo.GetDueAsync(asOf, take, ct);
        return _mapper.Map<List<TransactionPaymentResponse>>(items);
    }

    // =================================OVERVIEW ==================================
    #region Transaction
    public async Task<TransactionMonthlyRevenueResponse> GetMonthlyRevenueAsync(int? year, CancellationToken ct = default)
    {
        var y = year ?? DateTime.UtcNow.Year;

        var repoItems = await _txRepo.GetMonthlyRevenueAsync(y, ct);
        // map sang 12 month (đảm bảo đủ 1..12, thiếu thì = 0)
        var dict = repoItems.ToDictionary(x => x.Month);

        var items = Enumerable.Range(1, 12)
            .Select(m =>
            {
                if (dict.TryGetValue(m, out var v))
                {
                    return new MonthlyRevenuePoint
                    {
                        Month = m,
                        TotalAmount = v.TotalAmount,
                        TransactionCount = v.TransactionCount
                    };
                }

                return new MonthlyRevenuePoint
                {
                    Month = m,
                    TotalAmount = 0m,
                    TransactionCount = 0
                };
            })
            .ToList();

        return new TransactionMonthlyRevenueResponse
        {
            Year = y,
            Items = items
        };
    }
    public async Task<TransactionMonthlyRevenueThreeYearsResponse> GetMonthlyRevenueThreeYearsAsync(int? year, CancellationToken ct = default)
    {
        var baseYear = year ?? DateTime.UtcNow.Year;
        var yMinus1 = baseYear - 1;
        var yMinus2 = baseYear - 2;

        var minus2List = await _txRepo.GetMonthlyRevenueAsync(yMinus2, ct);
        var minus1List = await _txRepo.GetMonthlyRevenueAsync(yMinus1, ct);
        var baseList = await _txRepo.GetMonthlyRevenueAsync(baseYear, ct);

        var dMinus2 = minus2List.ToDictionary(x => x.Month);
        var dMinus1 = minus1List.ToDictionary(x => x.Month);
        var dBase = baseList.ToDictionary(x => x.Month);

        var items = Enumerable.Range(1, 12)
            .Select(m =>
            {
                dMinus2.TryGetValue(m, out var v2);
                dMinus1.TryGetValue(m, out var v1);
                dBase.TryGetValue(m, out var v0);

                return new MonthlyRevenueThreeYearsPoint
                {
                    Month = m,

                    YearMinus2Amount = v2?.TotalAmount ?? 0m,
                    YearMinus2TransactionCount = v2?.TransactionCount ?? 0,

                    YearMinus1Amount = v1?.TotalAmount ?? 0m,
                    YearMinus1TransactionCount = v1?.TransactionCount ?? 0,

                    YearAmount = v0?.TotalAmount ?? 0m,
                    YearTransactionCount = v0?.TransactionCount ?? 0
                };
            })
            .ToList();

        return new TransactionMonthlyRevenueThreeYearsResponse
        {
            Year = baseYear,
            YearMinus1 = yMinus1,
            YearMinus2 = yMinus2,
            Items = items
        };
    }
    public async Task<TransactionMonthlyStatusResponse> GetMonthlyStatusAsync(int year, CancellationToken ct = default)
    {
        var repoResult = await _txRepo.GetMonthlyStatusAsync(year, ct);

        return new TransactionMonthlyStatusResponse
        {
            Year = repoResult.Year,
            Items = repoResult.Items
                .Select(x => new TransactionMonthlyStatusItemResponse
                {
                    Month = x.Month,
                    SuccessCount = x.SuccessCount,
                    PendingCount = x.PendingCount,
                    FailedCount = x.FailedCount
                })
                .ToList()
        };
    }
    public async Task<TransactionDailyCashflowResponse> GetDailyCashflowAsync(int lastDays, CancellationToken ct = default)
    {
        if (lastDays <= 0) lastDays = 30;
        if (lastDays > 365) lastDays = 365;

        var today = DateOnly.FromDateTime(DateTime.UtcNow.Date);
        var from = today.AddDays(-(lastDays - 1));

        var fromStart = new DateTimeOffset(from.Year, from.Month, from.Day, 0, 0, 0, TimeSpan.Zero);
        var toExclusive = new DateTimeOffset(today.Year, today.Month, today.Day, 0, 0, 0, TimeSpan.Zero)
            .AddDays(1);

        // Lấy dữ liệu group theo ngày từ repo
        var raw = await _txRepo.GetDailyCashflowAggAsync(fromStart, toExclusive, ct);
        var dict = raw.ToDictionary(x => x.Date, x => x);

        // Fill đầy đủ lastDays ngày, chỗ nào không có => 0
        var items = new List<DailyCashflowItem>();
        for (var d = from; d <= today; d = d.AddDays(1))
        {
            if (dict.TryGetValue(d, out var v))
            {
                items.Add(new DailyCashflowItem
                {
                    Date = d,
                    Revenue = v.Revenue,
                    SuccessCount = v.SuccessCount,
                });
            }
            else
            {
                items.Add(new DailyCashflowItem
                {
                    Date = d,
                    Revenue = 0,
                    SuccessCount = 0,
                });
            }
        }

        return new TransactionDailyCashflowResponse
        {
            From = from,
            To = today,
            Items = items,
        };
    }
    public async Task<TransactionInstallmentAgingResponse> GetInstallmentAgingAsync(DateTimeOffset? asOf, CancellationToken ct = default)
    {
        var result = await _txRepo.GetInstallmentAgingAsync(asOf, ct);

        return new TransactionInstallmentAgingResponse
        {
            AsOf = result.AsOf,
            TotalInstallments = result.TotalInstallments,
            TotalOutstandingAmount = result.TotalOutstandingAmount,
            Items = result.Items.Select(x => new TransactionInstallmentAgingItemResponse
            {
                BucketKey = x.BucketKey,
                InstallmentCount = x.InstallmentCount,
                OutstandingAmount = x.OutstandingAmount,
            }).ToList()
        };

    }
    public async Task<TransactionTopCustomersResponse> GetTopCustomersAsync(int year,int topN,CancellationToken ct = default)
    {
        if (year <= 0) year = DateTimeOffset.UtcNow.Year;
        if (topN <= 0) topN = 5;

        var items = await _txRepo.GetTopCustomersAsync(year, topN, ct);

        return new TransactionTopCustomersResponse
        {
            Year = year,
            TopN = topN,
            Items = items
        };
    }
    #endregion

    #region SubsciptionPlan
    public async Task<TransactionPaymentModeInsightResponse> GetPaymentModeInsightAsync(int year,CancellationToken ct = default)
    {
        var items = await _txRepo.GetPaymentModeInsightAsync(year, ct);

        return new TransactionPaymentModeInsightResponse
        {
            Year = year,
            Items = items
        };
    }
    public async Task<TransactionPlanRevenueInsightResponse> GetPlanRevenueInsightAsync(int year,CancellationToken ct = default)
    {
        var items = await _txRepo.GetPlanRevenueInsightAsync(year, ct);

        return new TransactionPlanRevenueInsightResponse
        {
            Year = year,
            Items = items
        };
    }
    public async Task<List<SubscriptionPlanPurchaseStatResponse>> GetPlanPurchaseStatsAsync(CancellationToken ct = default)
    {
        var rows = await _txRepo.GetSubscriptionPlanPurchaseStatsAsync(ct);

        var totalCount = rows.Sum(x => x.PurchaseCount);

        if (totalCount == 0)
        {
            return rows
                .Select(x => new SubscriptionPlanPurchaseStatResponse
                {
                    PlanId = x.PlanId,
                    PlanName = x.PlanName ?? string.Empty,
                    PurchaseCount = x.PurchaseCount,
                    TotalAmount = x.TotalAmount,
                    Percentage = 0m
                })
                .ToList();
        }

        return rows
            .Select(x => new SubscriptionPlanPurchaseStatResponse
            {
                PlanId = x.PlanId,
                PlanName = x.PlanName ?? string.Empty,
                PurchaseCount = x.PurchaseCount,
                TotalAmount = x.TotalAmount,
                Percentage = Math.Round(
                    (decimal)x.PurchaseCount * 100m / totalCount,
                    2)
            })
            .OrderByDescending(x => x.PurchaseCount)
            .ToList();
    }
    public async Task<List<SubscriptionPlanPurchaseStatResponse>> GetTopPlanPurchaseStatsAsync(int top = 3,bool includeOther = true,CancellationToken ct = default)
    {
        var all = await GetPlanPurchaseStatsAsync(ct);
        if (all.Count == 0) return all;

        top = top <= 0 ? 3 : top;

        var ordered = all
            .OrderByDescending(x => x.PurchaseCount)
            .ToList();

        var totalCount = ordered.Sum(x => x.PurchaseCount);

        var topPlans = ordered.Take(top).ToList();

        if (!includeOther)
        {
            // Recompute percentage based on totalCount (đề phòng top != all)
            foreach (var p in topPlans)
            {
                p.Percentage = totalCount == 0
                    ? 0m
                    : Math.Round(
                        (decimal)p.PurchaseCount * 100m / totalCount,
                        2);
            }
            return topPlans;
        }

        var others = ordered.Skip(top).ToList();
        var otherCount = others.Sum(x => x.PurchaseCount);
        var otherAmount = others.Sum(x => x.TotalAmount);

        // Cập nhật lại % cho top trước, dựa trên tổng (top + other)
        foreach (var p in topPlans)
        {
            p.Percentage = totalCount == 0
                ? 0m
                : Math.Round(
                    (decimal)p.PurchaseCount * 100m / totalCount,
                    2);
        }

        if (otherCount > 0)
        {
            var otherPercentage = totalCount == 0
                ? 0m
                : Math.Round(
                    (decimal)otherCount * 100m / totalCount,
                    2);

            topPlans.Add(new SubscriptionPlanPurchaseStatResponse
            {
                PlanId = Guid.Empty,
                PlanName = "Other",
                PurchaseCount = otherCount,
                TotalAmount = otherAmount,
                Percentage = otherPercentage
            });
        }

        return topPlans;
    }
    public async Task<List<PlanMonthlyPurchaseCountRow>> GetPlanMonthlyPurchaseStatsAsync(
       int year,
       CancellationToken ct = default)
    {
        var rows = await _txRepo.GetPlanMonthlyPurchaseCountsAsync(year, ct);

        var result = rows
            .Select(x => new PlanMonthlyPurchaseCountRow
            {
                PlanId = x.PlanId,
                PlanName = x.PlanName ?? string.Empty,
                Year = x.Year,
                Month = x.Month,
                PurchaseCount = x.PurchaseCount,
                TotalAmount = x.TotalAmount
            })
            .OrderBy(r => r.Year)
            .ThenBy(r => r.Month)
            .ThenByDescending(r => r.PurchaseCount)
            .ToList();

        return result;
    }

    #endregion
}