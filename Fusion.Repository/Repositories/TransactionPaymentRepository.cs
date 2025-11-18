

using Fusion.Repository.Bases.Page;
using Fusion.Repository.Bases.Page.TransactionPayment;
using Fusion.Repository.Data;
using Fusion.Repository.Entities;
using Fusion.Repository.Enums;
using Fusion.Repository.IRepositories;
using Fusion.Repository.ViewModels;
using Microsoft.EntityFrameworkCore;
using System.Reflection;
using System.Runtime.Serialization;

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
    public async Task<PagedResult<TransactionPayment>> GetPagedAsync(TransactionPaymentPagedRequest request, CancellationToken ct = default)
    {
        var q = _context.TransactionPayments
                        .AsNoTracking()
                        .Include(tp => tp.User)
                        .Include(tp => tp.SubscriptionPlan)
                        .AsQueryable();

        // Filters
        if (!string.IsNullOrWhiteSpace(request.UserName))
        {
            var pattern = $"%{request.UserName.Trim()}%";
            q = q.Where(tp => tp.User.UserName != null && EF.Functions.Like(tp.User.UserName, pattern));
        }

        if (!string.IsNullOrWhiteSpace(request.PlanName))
        {
            var pattern = $"%{request.PlanName.Trim()}%";
            q = q.Where(tp => tp.SubscriptionPlan.Name != null && EF.Functions.Like(tp.SubscriptionPlan.Name, pattern));
        }

        if (!string.IsNullOrWhiteSpace(request.Status))
        {
            if (TryParsePaymentStatus(request.Status, out var st))
            {
                q = q.Where(tp => tp.Status == st);
            }
        }

        if (request.TransactionAt.From.HasValue)
            q = q.Where(tp => tp.TransactionDateTime >= request.TransactionAt.From.Value);

        if (request.TransactionAt.To.HasValue)
            q = q.Where(tp => tp.TransactionDateTime <= request.TransactionAt.To.Value);

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
                (tp.OrderCode.HasValue && EF.Functions.Like(tp.OrderCode.Value.ToString(), $"%{kw}%"))
            );
        }

        // Default sort
        if (string.IsNullOrWhiteSpace(request.SortColumn))
        {
            request.SortColumn = nameof(TransactionPayment.TransactionDateTime);
            request.SortDescending = true;
        }

        return await q.ToPagedResultAsync(request, ct);
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
}

