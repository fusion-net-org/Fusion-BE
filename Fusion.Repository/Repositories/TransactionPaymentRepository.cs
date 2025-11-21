

using Fusion.Repository.Bases.Page.TransactionPayment;
using Fusion.Repository.Data;
using Fusion.Repository.Entities;
using Fusion.Repository.Enums;
using Fusion.Repository.IRepositories;
using Fusion.Repository.ViewModels.SubscriptionPlan;
using Fusion.Repository.ViewModels.Transactions;
using Microsoft.EntityFrameworkCore;
using System.Reflection;
using System.Runtime.Serialization;
using System.Transactions;

namespace Fusion.Repository.Repositories;

public class TransactionPaymentRepository : GenericRepository<TransactionPayment>, ITransactionPaymentRepository
{
    private readonly FusionDbContext _context;
    public TransactionPaymentRepository(FusionDbContext context) : base(context)
    {
        _context = context;
    }

    /* ================== Helpers ================== */
    private static bool TryParsePaymentStatus(string? input, out PaymentStatus value)
    {
        value = default;
        if (string.IsNullOrWhiteSpace(input)) return false;

        // 1) Try enum name (Success, Pending, ...)
        if (Enum.TryParse<PaymentStatus>(input, true, out value)) return true;

        // 2) Try EnumMember value ("success", "pending", ...)
        foreach (var name in Enum.GetNames(typeof(PaymentStatus)))
        {
            var fi = typeof(PaymentStatus).GetField(name);
            var em = fi?.GetCustomAttribute<EnumMemberAttribute>();
            if (em?.Value != null && em.Value.Equals(input, StringComparison.OrdinalIgnoreCase))
            {
                value = (PaymentStatus)Enum.Parse(typeof(PaymentStatus), name);
                return true;
            }
        }
        return false;
    }

    /* ================== Gets ================== */
    public Task<TransactionPayment?> GetByIdWithNavAsync(Guid id, CancellationToken ct = default)
        => _context.TransactionPayments
                   .Include(x => x.User)
                   .Include(x => x.SubscriptionPlan)
                       .ThenInclude(p => p.Price)
                   .Include(x => x.SubscriptionPlan)
                       .ThenInclude(p => p.Features)
                               .ThenInclude(pf => pf.Feature)
                   .FirstOrDefaultAsync(x => x.Id == id, ct);

    public Task<TransactionPayment?> GetByOrderCodeAsync(long orderCode, CancellationToken ct = default)
    => _context.TransactionPayments.AsNoTracking()
               .FirstOrDefaultAsync(x => x.OrderCode == orderCode, ct);

    public Task<TransactionPayment?> GetByPaymentLinkIdAsync(string paymentLinkId, CancellationToken ct = default)
        => _context.TransactionPayments.AsNoTracking()
                   .FirstOrDefaultAsync(x => x.PaymentLinkId == paymentLinkId, ct);

    public Task<bool> ExistsOrderCodeAsync(long orderCode, CancellationToken ct = default)
        => _context.TransactionPayments.AnyAsync(x => x.OrderCode == orderCode, ct);

    public Task<bool> ExistsPaymentLinkIdAsync(string paymentLinkId, CancellationToken ct = default)
        => _context.TransactionPayments.AnyAsync(x => x.PaymentLinkId == paymentLinkId, ct);

    /* ================== Create ================== */
    public async Task<TransactionPayment> CreateDraftChargeAsync(TransactionPayment draft, CancellationToken ct = default)
    {
        // Guard: must be draft (Pending + Charge), PaidAt null
        draft.Id = draft.Id == Guid.Empty ? Guid.NewGuid() : draft.Id;
        draft.Status = PaymentStatus.Pending;
        draft.Type = TransactionType.Charge;
        draft.PaidAt = null;
        draft.TransactionDateTime = null;

        _context.TransactionPayments.Add(draft);
        await _context.SaveChangesAsync(ct);
        return draft;
    }

    public async Task<int> BulkCreateAsync(IEnumerable<TransactionPayment> rows, CancellationToken ct = default)
    {
        var list = rows.ToList();
        foreach (var r in list)
        {
            r.Id = r.Id == Guid.Empty ? Guid.NewGuid() : r.Id;
            r.Status = PaymentStatus.Pending;
            r.Type = TransactionType.Charge;
            r.PaidAt = null;
            r.TransactionDateTime = null;
        }
        _context.TransactionPayments.AddRange(list);
        return await _context.SaveChangesAsync(ct);
    }

    /* ================== Transitions ================== */
    public async Task<bool> AttachPaymentLinkAsync(Guid id, long orderCode, string paymentLinkId, string? provider, CancellationToken ct = default)
    {
        var tx = await _context.TransactionPayments.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (tx == null) return false;

        if (tx.Status != PaymentStatus.Pending) return false; // only pending can attach
        tx.OrderCode = orderCode;
        tx.PaymentLinkId = paymentLinkId;
        tx.Provider = provider;
        await _context.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> MarkSuccessAsync(Guid id, decimal? amount, DateTimeOffset paidAt, string? reference, CancellationToken ct = default)
    {
        var tx = await _context.TransactionPayments.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (tx == null) return false;
        if (tx.Status != PaymentStatus.Pending) return true; // idempotent: already set -> no-op true

        tx.Status = PaymentStatus.Success;
        if (amount.HasValue) tx.Amount = amount.Value;
        tx.PaidAt = paidAt;
        tx.TransactionDateTime = tx.TransactionDateTime ?? paidAt; // keep if gateway provided earlier
        tx.Reference = reference;

        //await _context.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> MarkFailedAsync(Guid id, string? description, string? reference, CancellationToken ct = default)
    {
        var tx = await _context.TransactionPayments.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (tx == null) return false;
        if (tx.Status != PaymentStatus.Pending) return true; // idempotent

        tx.Status = PaymentStatus.Failed;
        if (!string.IsNullOrWhiteSpace(description)) tx.Description = description;
        if (!string.IsNullOrWhiteSpace(reference)) tx.Reference = reference;

        //await _context.SaveChangesAsync(ct);
        return true;
    }

    /* ================== Scheduled ================== */
    public async Task<List<TransactionPayment>> GetDueAsync(DateTimeOffset asOf, int take = 100, CancellationToken ct = default)
    {
        return await _context.TransactionPayments
            .AsNoTracking()
            .Where(x => x.Type == TransactionType.Charge
                        && x.Status == PaymentStatus.Pending
                        && x.DueAt != null
                        && x.DueAt <= asOf)
            .OrderBy(x => x.DueAt)
            .ThenBy(x => x.CreatedAt)
            .Take(take)
            .ToListAsync(ct);
    }

    /* ================== Paged ================== */
    private static IQueryable<TransactionPayment> ApplyFilter(
     IQueryable<TransactionPayment> q,
     TransactionPaymentPagedRequest request)
    {
        // --- Filter theo UserName ---
        if (!string.IsNullOrWhiteSpace(request.UserName))
        {
            var pattern = $"%{request.UserName.Trim()}%";
            q = q.Where(tp =>
                tp.User.UserName != null &&
                EF.Functions.Like(tp.User.UserName, pattern));
        }

        // --- Filter theo PlanName ---
        if (!string.IsNullOrWhiteSpace(request.PlanName))
        {
            var pattern = $"%{request.PlanName.Trim()}%";
            q = q.Where(tp =>
                tp.SubscriptionPlan.Name != null &&
                EF.Functions.Like(tp.SubscriptionPlan.Name, pattern));
        }

        // --- Filter theo Status ---
        if (!string.IsNullOrWhiteSpace(request.Status) &&
            TryParsePaymentStatus(request.Status, out var st))
        {
            q = q.Where(tp => tp.Status == st);
        }

        // --- Filter theo khoảng thời gian ---
        if (request.TransactionAt.From.HasValue)
        {
            var from = request.TransactionAt.From.Value;
            q = q.Where(tp => tp.TransactionDateTime >= from);
        }

        if (request.TransactionAt.To.HasValue)
        {
            var to = request.TransactionAt.To.Value;
            q = q.Where(tp => tp.TransactionDateTime <= to);
        }

        // --- Filter theo Keyword đa trường ---
        if (!string.IsNullOrWhiteSpace(request.Keyword))
        {
            var kw = request.Keyword.Trim();
            var pattern = $"%{kw}%";

            q = q.Where(tp =>
                (tp.Reference != null && EF.Functions.Like(tp.Reference, pattern)) ||
                (tp.Description != null && EF.Functions.Like(tp.Description, pattern)) ||
                (tp.PaymentLinkId != null && EF.Functions.Like(tp.PaymentLinkId, pattern)) ||
                (tp.Currency != null && EF.Functions.Like(tp.Currency, pattern)) ||
                (tp.Provider != null && EF.Functions.Like(tp.Provider, pattern)) ||
                (tp.PaymentMethod != null && EF.Functions.Like(tp.PaymentMethod, pattern)) ||
                (tp.User.UserName != null && EF.Functions.Like(tp.User.UserName, pattern)) ||
                (tp.SubscriptionPlan.Name != null && EF.Functions.Like(tp.SubscriptionPlan.Name, pattern)) ||
                (tp.OrderCode.HasValue &&
                    EF.Functions.Like(tp.OrderCode.Value.ToString(), pattern))
            );
        }
        return q;
    }

    private static IQueryable<TransactionPayment> ApplySort(IQueryable<TransactionPayment> q, TransactionPaymentPagedRequest request)
    {
        var desc = request.SortDescending;
        var col = request.SortColumn?.Trim();

        // Default sort: TransactionDateTime desc (fallback CreatedAt)
        if (string.IsNullOrWhiteSpace(col))
        {
            return desc
                ? q.OrderByDescending(x => x.TransactionDateTime ?? x.CreatedAt)
                : q.OrderBy(x => x.TransactionDateTime ?? x.CreatedAt);
        }

        switch (col)
        {
            case nameof(TransactionPayment.Amount):
                q = desc ? q.OrderByDescending(x => x.Amount)
                         : q.OrderBy(x => x.Amount);
                break;

            case nameof(TransactionPayment.Status):
                q = desc ? q.OrderByDescending(x => x.Status)
                         : q.OrderBy(x => x.Status);
                break;

            case nameof(TransactionPayment.OrderCode):
                q = desc ? q.OrderByDescending(x => x.OrderCode)
                         : q.OrderBy(x => x.OrderCode);
                break;

            case nameof(TransactionPayment.CreatedAt):
                q = desc ? q.OrderByDescending(x => x.CreatedAt)
                         : q.OrderBy(x => x.CreatedAt);
                break;

            case nameof(TransactionPayment.TransactionDateTime):
                q = desc ? q.OrderByDescending(x => x.TransactionDateTime)
                         : q.OrderBy(x => x.TransactionDateTime);
                break;

            case "UserName": // sort theo user
                q = desc ? q.OrderByDescending(x => x.User.UserName)
                         : q.OrderBy(x => x.User.UserName);
                break;

            case "PlanName": // sort theo plan
                q = desc ? q.OrderByDescending(x => x.SubscriptionPlan.Name)
                         : q.OrderBy(x => x.SubscriptionPlan.Name);
                break;

            default:
                q = desc
                    ? q.OrderByDescending(x => x.TransactionDateTime ?? x.CreatedAt)
                    : q.OrderBy(x => x.TransactionDateTime ?? x.CreatedAt);
                break;
        }

        return q;
    }

    public async Task<TransactionPaymentPagedRepoResult> GetPagedAsync(TransactionPaymentPagedRequest request,CancellationToken ct = default)
    {
        var baseQuery = _context.TransactionPayments
                                .AsNoTracking()
                                .Include(tp => tp.User)
                                .Include(tp => tp.SubscriptionPlan)
                                .AsQueryable();

        // 1) Apply filter chung
        baseQuery = ApplyFilter(baseQuery, request);

        // 2) Tính thống kê trên toàn bộ tập sau filter (chưa paging)
        var stats = await baseQuery
            .GroupBy(_ => 1)
            .Select(g => new
            {
                TotalTransactions = g.Count(),

                // Revenue: chỉ tính các CHARGE thành công
                TotalRevenue = g.Sum(x =>
                    x.Status == PaymentStatus.Success &&
                    x.Type == TransactionType.Charge
                        ? x.Amount
                        : 0m),

                TotalSuccess = g.Count(x => x.Status == PaymentStatus.Success),
                TotalFailed = g.Count(x => x.Status == PaymentStatus.Failed),
                TotalPending = g.Count(x => x.Status == PaymentStatus.Pending),
            })
            .FirstOrDefaultAsync(ct);

        var totalCount = stats?.TotalTransactions ?? 0;

        // 3) Sort + paging
        var sorted = ApplySort(baseQuery, request);

        var pageNumber = request.PageNumber <= 0 ? 1 : request.PageNumber;
        var pageSize = request.PageSize <= 0 ? 10 : request.PageSize;

        var items = await sorted
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        // 4) Trả về kết quả + stats
        return new TransactionPaymentPagedRepoResult
        {
            Items = items,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount,

            TotalRevenue = stats?.TotalRevenue ?? 0m,
            TotalSuccess = stats?.TotalSuccess ?? 0,
            TotalFailed = stats?.TotalFailed ?? 0,
            TotalPending = stats?.TotalPending ?? 0,
        };
    }

    public async Task<bool> LinkToSubscriptionAsync(Guid transactionId, Guid userSubscriptionId, CancellationToken ct = default)
    {
        var tx = await _context.TransactionPayments.FirstOrDefaultAsync(x => x.Id == transactionId, ct);
        if (tx == null) return false;

        // Đã gắn đúng => idempotent
        if (tx.UserSubscriptionId.HasValue && tx.UserSubscriptionId.Value == userSubscriptionId)
            return true;

        // Lấy subscription để validate
        var us = await _context.Set<UserSubscription>()
                               .AsNoTracking()
                               .FirstOrDefaultAsync(x => x.Id == userSubscriptionId, ct);
        if (us == null) return false;

        // Validate an toàn: Transaction và Subscription phải cùng User & Plan
        if (us.UserId != tx.UserId || us.PlanId != tx.PlanId)
            throw new InvalidOperationException("Transaction and UserSubscription mismatch (User/Plan).");

        // Nếu đã gắn với sub khác => không cho overwrite (tránh ghi sai lịch sử)
        if (tx.UserSubscriptionId.HasValue && tx.UserSubscriptionId.Value != userSubscriptionId)
            throw new InvalidOperationException("Transaction already linked to a different subscription.");

        tx.UserSubscriptionId = userSubscriptionId;
        //await _context.SaveChangesAsync(ct);
        return true;
    }

    public async Task<TransactionPayment?> FindNextPendingInstallmentAsync(Guid userId, Guid planId, Guid? userSubscriptionId, int currentInstallmentIndex, CancellationToken ct = default)
    {
        return await _context.TransactionPayments
       .AsNoTracking()
       .Where(tp =>
           tp.UserId == userId &&
           tp.PlanId == planId &&
           tp.UserSubscriptionId == userSubscriptionId && 
           tp.Type == TransactionType.Charge &&
           tp.PaymentModeSnapshot == PaymentMode.Installments &&
           tp.Status == PaymentStatus.Pending &&
           tp.InstallmentIndex.HasValue &&
           tp.InstallmentIndex.Value > currentInstallmentIndex
       )
       .OrderBy(tp => tp.InstallmentIndex)
       .ThenBy(tp => tp.DueAt ?? DateTimeOffset.MaxValue)
       .FirstOrDefaultAsync(ct);

    }

    public async Task<int> AttachSubscriptionToInstallmentBatchAsync(Guid userId, Guid planId, Guid userSubscriptionId, CancellationToken ct = default)
    {
        var rows = await _context.TransactionPayments
          .Where(tp =>
              tp.UserId == userId &&
              tp.PlanId == planId &&
              tp.Type == TransactionType.Charge &&
              tp.PaymentModeSnapshot == PaymentMode.Installments &&
              tp.UserSubscriptionId == null)        // ⬅️ CHỈ những cái CHƯA có sub
          .ToListAsync(ct);

        foreach (var r in rows)
        {
            r.UserSubscriptionId = userSubscriptionId;
        }

        return await _context.SaveChangesAsync(ct);
    }

    public async Task<TransactionPayment?> FindEarliestPendingInstallmentAsync(Guid userId, Guid planId, Guid? userSubscriptionId, CancellationToken ct = default)
    {
        var q = _context.TransactionPayments
        .AsNoTracking()
        .Where(tp =>
            tp.UserId == userId &&
            tp.PlanId == planId &&
            tp.Type == TransactionType.Charge &&
            tp.PaymentModeSnapshot == PaymentMode.Installments &&
            tp.Status == PaymentStatus.Pending &&
            tp.InstallmentIndex.HasValue);

        // Nếu đã link các installment của cùng subscription lại với nhau,
        // thì luôn nên truyền userSubscriptionId để tránh dính batch cũ
        if (userSubscriptionId.HasValue)
        {
            q = q.Where(tp => tp.UserSubscriptionId == userSubscriptionId.Value);
        }

        return await q
            .OrderBy(tp => tp.InstallmentIndex)                    // kỳ thứ mấy (1,2,3,…)
            .ThenBy(tp => tp.DueAt ?? DateTimeOffset.MaxValue)     // ưu tiên DueAt sớm
            .ThenBy(tp => tp.CreatedAt)                            // tie-break
            .FirstOrDefaultAsync(ct);
    }


    //========================================= OVERVIEW =========================================//
    #region Transaction
    public async Task<List<TransactionMonthlyRevenueRepoItem>> GetMonthlyRevenueAsync(int year, CancellationToken ct = default)
    {
        var query = _context.TransactionPayments
            .AsNoTracking()
            .Where(tp =>
                tp.Status == PaymentStatus.Success &&
                tp.PaidAt.HasValue &&
                tp.PaidAt.Value.Year == year);

        var grouped = await query
           .GroupBy(tp => tp.PaidAt!.Value.Month)
           .Select(g => new TransactionMonthlyRevenueRepoItem
           {
               Month = g.Key,
               TotalAmount = g.Sum(x => x.Amount),
               TransactionCount = g.Count()
           })
           .ToListAsync(ct);

        return grouped;
    }
    public async Task<TransactionMonthlyStatusRepoResult> GetMonthlyStatusAsync(int year, CancellationToken ct = default)
    {
        if (year <= 0)
            year = DateTime.UtcNow.Year;

        var baseQuery = _context.TransactionPayments
            .AsNoTracking()
            .Where(tp => tp.Type == TransactionType.Charge);

        // Group theo tháng + status
        var grouped = await baseQuery
            .Where(tp => (tp.TransactionDateTime ?? tp.CreatedAt).Year == year)
            .GroupBy(tp => new
            {
                Month = (tp.TransactionDateTime ?? tp.CreatedAt).Month,
                tp.Status
            })
            .Select(g => new
            {
                g.Key.Month,
                g.Key.Status,
                Count = g.Count()
            })
            .ToListAsync(ct);

        var items = new List<TransactionMonthlyStatusRepoItem>();

        for (var month = 1; month <= 12; month++)
        {
            var monthRows = grouped.Where(x => x.Month == month);

            var success = monthRows
                .Where(x => x.Status == PaymentStatus.Success)
                .Sum(x => x.Count);

            var pending = monthRows
                .Where(x => x.Status == PaymentStatus.Pending)
                .Sum(x => x.Count);

            // Failed = Failed + Cancelled + Refunded
            var failed = monthRows
                .Where(x =>
                    x.Status == PaymentStatus.Failed ||
                    x.Status == PaymentStatus.Cancelled ||
                    x.Status == PaymentStatus.Refunded)
                .Sum(x => x.Count);

            items.Add(new TransactionMonthlyStatusRepoItem
            {
                Month = month,
                SuccessCount = success,
                PendingCount = pending,
                FailedCount = failed
            });
        }

        return new TransactionMonthlyStatusRepoResult
        {
            Year = year,
            Items = items
        };
    }
    public async Task<List<DailyCashflowAgg>> GetDailyCashflowAggAsync(DateTimeOffset from, DateTimeOffset toExclusive,CancellationToken ct = default)
    {
        return await _context.TransactionPayments
            .AsNoTracking()
            .Where(tp =>
                tp.Type == TransactionType.Charge &&
                tp.Status == PaymentStatus.Success &&
                tp.PaidAt != null &&
                tp.PaidAt >= from &&
                tp.PaidAt < toExclusive)
            .GroupBy(tp => tp.PaidAt!.Value.Date) 
            .Select(g => new DailyCashflowAgg
            {
                Date = DateOnly.FromDateTime(g.Key),
                Revenue = g.Sum(x => x.Amount),
                SuccessCount = g.Count(),
            })
            .ToListAsync(ct);
    }
    public async Task<TransactionInstallmentAgingResult> GetInstallmentAgingAsync( DateTimeOffset? asOf = null,CancellationToken ct = default)
    {
        var now = asOf ?? DateTimeOffset.UtcNow;

        // Chỉ lấy các installment CHARGE, đang Pending, có DueAt
        var q = _context.TransactionPayments
            .AsNoTracking()
            .Where(tp =>
                tp.PaymentModeSnapshot == PaymentMode.Installments &&
                tp.Type == TransactionType.Charge &&
                tp.DueAt != null &&
                tp.Status == PaymentStatus.Pending);

        var raw = await q
            .Select(tp => new
            {
                tp.Amount,
                tp.DueAt
            })
            .ToListAsync(ct);

        var buckets = new List<TransactionInstallmentAgingBucket>();

        if (raw.Count > 0)
        {
            var asOfDate = now.Date;

            foreach (var grp in raw.GroupBy(x =>
            {
                var dueDate = x.DueAt!.Value.Date;
                var days = (asOfDate - dueDate).TotalDays;
                var d = (int)Math.Floor(days);

                // bucket key
                if (d <= 0) return "NotDue"; // chưa đến hạn hoặc đúng ngày
                if (d <= 7) return "1-7";
                if (d <= 14) return "8-14";
                if (d <= 30) return "15-30";
                if (d <= 60) return "31-60";
                return "60+";
            }))
            {
                buckets.Add(new TransactionInstallmentAgingBucket
                {
                    BucketKey = grp.Key,
                    InstallmentCount = grp.Count(),
                    OutstandingAmount = grp.Sum(x => x.Amount),
                });
            }
        }

        var result = new TransactionInstallmentAgingResult
        {
            AsOf = now,
            Items = buckets
                .OrderBy(x => x.BucketKey == "NotDue" ? 0 :
                              x.BucketKey == "1-7" ? 1 :
                              x.BucketKey == "8-14" ? 2 :
                              x.BucketKey == "15-30" ? 3 :
                              x.BucketKey == "31-60" ? 4 : 5)
                .ToList(),
        };

        result.TotalInstallments = result.Items.Sum(x => x.InstallmentCount);
        result.TotalOutstandingAmount = result.Items.Sum(x => x.OutstandingAmount);

        return result;
    }
    public async Task<List<TransactionTopCustomerItemResponse>> GetTopCustomersAsync(int year, int topN,CancellationToken ct = default)
    {
        if (topN <= 0) topN = 5;
        if (topN > 50) topN = 50;

        var start = new DateTimeOffset(new DateTime(year, 1, 1, 0, 0, 0, DateTimeKind.Utc));
        var end = start.AddYears(1);

        var query = _context.TransactionPayments
            .AsNoTracking()
            .Include(tp => tp.User) // nếu có navigation User
            .Where(tp =>
                tp.Type == TransactionType.Charge &&
                tp.Status == PaymentStatus.Success &&
                ((tp.TransactionDateTime ?? tp.CreatedAt) >= start &&
                 (tp.TransactionDateTime ?? tp.CreatedAt) < end));

        var result = await query
            .GroupBy(tp => new
            {
                tp.UserId,
                UserName = tp.User.UserName,
                Email = tp.User != null ? tp.User.Email : null
            })
            .Select(g => new TransactionTopCustomerItemResponse
            {
                UserId = g.Key.UserId,
                UserName = g.Key.UserName,
                Email = g.Key.Email,
                TotalAmount = g.Sum(x => x.Amount),
                SuccessCount = g.Count(),
                MaxPayment = g.Max(x => x.Amount),
                LastPaymentAt = g.Max(x => x.TransactionDateTime ?? x.CreatedAt),
            })
            .OrderByDescending(x => x.TotalAmount)
            .ThenBy(x => x.UserName)
            .Take(topN)
            .ToListAsync(ct);

        return result;
    }
    #endregion

    #region SubscriptionPlan
    public async Task<List<TransactionPaymentModeInsightItemDto>> GetPaymentModeInsightAsync(int year,CancellationToken ct = default)
    {
        var query = _context.TransactionPayments
            .AsNoTracking()
            .Where(tp =>
                tp.Type == TransactionType.Charge &&
                tp.PaidAt.HasValue &&
                tp.PaidAt.Value.Year == year);

        var result = await query
            .GroupBy(tp => tp.PaymentModeSnapshot) // string snapshot: "Prepaid", "Installments"
            .Select(g => new TransactionPaymentModeInsightItemDto
            {
                PaymentMode = g.Key,
                TransactionCount = g.Count(),
                SuccessCount = g.Count(x => x.Status == PaymentStatus.Success),
                PendingCount = g.Count(x => x.Status == PaymentStatus.Pending),
                FailedCount = g.Count(x =>
                    x.Status == PaymentStatus.Failed ||
                    x.Status == PaymentStatus.Cancelled),
                TotalAmount = g
                    .Where(x => x.Status == PaymentStatus.Success)
                    .Sum(x => x.Amount)
            })
            .OrderByDescending(x => x.TotalAmount)
            .ToListAsync(ct);

        return result;
    }
    public async Task<List<TransactionPlanRevenueInsightItem>> GetPlanRevenueInsightAsync(int year, CancellationToken ct)
    {
        if (year <= 0) year = DateTime.UtcNow.Year;

        var query = _context.TransactionPayments
            .AsNoTracking()
            .Where(tp =>
                tp.Type == TransactionType.Charge &&
                tp.TransactionDateTime.HasValue &&
                tp.TransactionDateTime.Value.Year == year);

        var items = await query
            .GroupBy(tp => new
            {
                tp.PlanId,
                PlanName = tp.SubscriptionPlan.Name   // navigation tới SubscriptionPlan
            })
            .Select(g => new TransactionPlanRevenueInsightItem
            {
                PlanId = g.Key.PlanId,
                PlanName = g.Key.PlanName ?? "Unknown",

                // tổng số giao dịch (mọi trạng thái) của plan trong năm
                TransactionCount = g.Count(),

                // số giao dịch thành công
                SuccessCount = g.Count(x => x.Status == PaymentStatus.Success),

                // revenue: chỉ cộng Amount của giao dịch thành công
                TotalAmount = g
                    .Where(x => x.Status == PaymentStatus.Success)
                    .Sum(x => (decimal?)x.Amount) ?? 0m
            })
            .OrderByDescending(x => x.TotalAmount) // Plan “mang tiền nhiều nhất” đứng đầu
            .ToListAsync(ct);

        return items;
    }
}
    #endregion
