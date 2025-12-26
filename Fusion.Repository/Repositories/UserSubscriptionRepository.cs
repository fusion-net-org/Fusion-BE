
using Fusion.Repository.Bases.Exceptions;
using Fusion.Repository.Bases.Page;
using Fusion.Repository.Bases.Page.UserSubscriptions;
using Fusion.Repository.Data;
using Fusion.Repository.Entities;
using Fusion.Repository.Enums;
using Fusion.Repository.IRepositories;
using Microsoft.EntityFrameworkCore;

namespace Fusion.Repository.Repositories
{
    public class UserSubscriptionRepository : GenericRepository<UserSubscription>, IUserSubscriptionRepository
    {
        private readonly FusionDbContext _context;
        public UserSubscriptionRepository(FusionDbContext context) : base(context)
        {
            _context = context;
        }
        public async Task<PagedResult<UserSubscription>> GetPagedByUserIdAsync(Guid id, UserSubscriptionPagedRequest request, CancellationToken ct = default)
        {
            var q = _context.Set<UserSubscription>()
              .AsNoTracking()
              .Include(x => x.User)
              .Include(x => x.Plan).ThenInclude(p => p.Price)
              .Include(x => x.Entitlements).ThenInclude(e => e.Feature)
              .Where(x => x.UserId == id);

            if (!string.IsNullOrWhiteSpace(request.Status))
            {
                var statusStr = request.Status.Trim();
                if (Enum.TryParse<SubscriptionStatus>(statusStr, ignoreCase: true, out var st))
                {
                    q = q.Where(x => x.Status == st);
                }
            }


            if (!string.IsNullOrWhiteSpace(request.Keyword))
            {
                var kw = request.Keyword.Trim();
                var like = $"%{kw}%";

                q = q.Where(x =>

                    (x.Plan.Name != null && EF.Functions.Like(x.Plan.Name, like)) ||
                    EF.Functions.Like(x.CurrencySnapshot, like)
                );
            }

            var ordered = q.OrderByDescending(x => x.CreatedByTransactionId == null);

            // Nếu không có sort column -> default: trong từng nhóm sort theo CreatedAt desc
            if (string.IsNullOrWhiteSpace(request.SortColumn))
            {
                q = q.OrderByDescending(x => x.CreatedAt);

                // để ToPagedResultAsync khỏi tự sort lại
                request.SortColumn = null;
            }
            return await q.ToPagedResultAsync(request, ct);
        }
        public Task<UserSubscription?> GetByIdAsync(Guid id, CancellationToken ct = default)
     => _context.Set<UserSubscription>()
                .AsNoTracking()
                .Include(x => x.User)
                .Include(us => us.Plan)
           .ThenInclude(p => p.Price)
       .Include(us => us.Entitlements)
           .ThenInclude(e => e.Feature)
       .FirstOrDefaultAsync(us => us.Id == id, ct);
        public Task<UserSubscription?> GetByIdWithNavAsync(Guid id, CancellationToken ct = default)
      => _context.Set<UserSubscription>()
                 .AsNoTracking()
                 .Include(x => x.User)
                 .Include(us => us.Plan)
            .ThenInclude(p => p.Price)
        .Include(us => us.Entitlements
            .Where(e => e.Feature.Category == "User")) // chỉ lấy entitlement có Feature.Category = "User"
            .ThenInclude(e => e.Feature)
        .FirstOrDefaultAsync(us => us.Id == id, ct);
        public Task<UserSubscription?> GetActiveByUserAsync(Guid userId, CancellationToken ct = default)
    => _context.Set<UserSubscription>()
               .AsNoTracking()
               .FirstOrDefaultAsync(x => x.UserId == userId && x.Status == Enums.SubscriptionStatus.Active, ct);
        public async Task<List<UserSubscription>> GetAllActiveByUserIdAsync(Guid userId, CancellationToken ct = default)
        {
            return await _context.UserSubscriptions
                                 
            .Include(cs => cs.Plan)                         // để map NameSubscription
            .Include(cs => cs.Entitlements)
                .ThenInclude(e => e.Feature)               // để map FeatureName
            .Where(cs => cs.Status == SubscriptionStatus.Active &&
                        cs.UserId == userId)
            .OrderBy(cs => cs.CreatedAt)
            .ToListAsync(ct);
        }
        public Task<UserSubscription?> GetByTransactionAsync(Guid txId, CancellationToken ct = default)
          => _context.Set<UserSubscription>()
                     .AsNoTracking()
                     .FirstOrDefaultAsync(x => x.CreatedByTransactionId == txId, ct);
        public async Task<UserSubscription> CreateAsync(UserSubscription entity, CancellationToken ct = default)
        {
            _context.UserSubscriptions.Add(entity);
            await _context.SaveChangesAsync(ct);
            return entity;
        }
        public async Task<bool> UpdateAsync(UserSubscription entity, CancellationToken ct = default)
        {
            var exist = await _context.UserSubscriptions.FirstOrDefaultAsync(x => x.Id == entity.Id, ct);
            if (exist == null) return false;

            // chỉ update các field thay đổi phổ biến; snapshots không đổi
            exist.Status = entity.Status;
            exist.TermStart = entity.TermStart;
            exist.TermEnd = entity.TermEnd;
            exist.NextPaymentDueAt = entity.NextPaymentDueAt;
            exist.UpdatedAt = DateTimeOffset.UtcNow;
            exist.CanceledAt = entity.CanceledAt;

            await _context.SaveChangesAsync(ct);
            return true;
        }
        //============= Background service======================
        #region
        public async Task<List<UserSubscription>> GetByUserAndPlanIdsAsync( Guid userId,IEnumerable<Guid> planIds, CancellationToken ct = default)
        {
            var ids = (planIds ?? Enumerable.Empty<Guid>())
                .Where(id => id != Guid.Empty)
                .Distinct()
                .ToList();

            if (ids.Count == 0)
                return new List<UserSubscription>();

            return await _context.UserSubscriptions
                .AsNoTracking()
                .Where(us => us.UserId == userId && ids.Contains(us.PlanId))
                .ToListAsync(ct);
        }
        public async Task<List<UserSubscription>> GetAllActiveByPlanIdsWithEntitlementsAsync(IEnumerable<Guid> planIds,CancellationToken ct = default)
        {
            var ids = (planIds ?? Enumerable.Empty<Guid>())
                .Where(id => id != Guid.Empty)
                .Distinct()
                .ToList();

            if (ids.Count == 0)
                return new List<UserSubscription>();

            return await _context.UserSubscriptions
                .Include(us => us.Entitlements)
                    .ThenInclude(e => e.Feature)
                .Where(us =>
                    us.Status == SubscriptionStatus.Active &&
                    ids.Contains(us.PlanId))
                .ToListAsync(ct);
        }
        public async Task<List<UserSubscription>> GetExpiredByTermCandidatesAsync(DateTimeOffset now, CancellationToken ct = default)
        {
            return await _context.UserSubscriptions
                .Where(us =>
                    us.Status == SubscriptionStatus.Active &&
                    us.TermEnd.HasValue &&
                    us.TermEnd < now)
                .ToListAsync(ct);
        }
        public async Task<List<UserSubscription>> GetInstallmentPendingCandidatesAsync(DateTimeOffset now, CancellationToken ct = default)
        {
            return await _context.UserSubscriptions
                .Where(us =>
                    us.PaymentModeSnapshot == PaymentMode.Installments &&
                    us.Status == SubscriptionStatus.Active &&
                    us.NextPaymentDueAt.HasValue &&
                    us.NextPaymentDueAt < now)
                .ToListAsync(ct);
        }
        public async Task<List<UserSubscription>> GetAutoMonthlyForUserWithEntitlementsAsync(Guid userId, CancellationToken ct = default)
        {
            if (userId == Guid.Empty)
                return new List<UserSubscription>();

            return await _context.UserSubscriptions
                .Include(us => us.Plan)
                .Include(us => us.Entitlements)
                    .ThenInclude(e => e.Feature)
                .Where(us =>
                    us.UserId == userId &&
                    us.Status == SubscriptionStatus.Active &&
                    us.CreatedByTransactionId == null)   // auto-month (free / system created)
                .OrderBy(us => us.CreatedAt)
                .ToListAsync(ct);
        }

        #endregion
        public async Task BulkAddEntitlementsAsync(IEnumerable<UserSubscriptionEntitlement> ents, CancellationToken ct = default)
        {
            if (ents == null) return;
            await _context.UserSubscriptionEntitlements.AddRangeAsync(ents, ct);
            //await _context.SaveChangesAsync(ct);
        }
        public Task<List<UserSubscription>> GetExpiringAsync(DateTimeOffset until, int take = 100, CancellationToken ct = default)
     => _context.UserSubscriptions
                .AsNoTracking()
                .Where(x => x.Status == Enums.SubscriptionStatus.Active
                         && x.TermEnd != null
                         && x.TermEnd <= until)
                .OrderBy(x => x.TermEnd)
                .Take(take)
                .ToListAsync(ct);
        public async Task UpdateNextDueAsync(Guid subId, DateTimeOffset? nextDueAt, CancellationToken ct = default)
        {
            var sub = await _context.UserSubscriptions
                                    .FirstOrDefaultAsync(x => x.Id == subId, ct);

            sub.NextPaymentDueAt = nextDueAt;
            sub.UpdatedAt = DateTimeOffset.UtcNow;

            await _context.SaveChangesAsync(ct);
        }
        public async Task DecreaseCompanyShareLimitAsync(Guid userSubscriptionId, int amount = 1,CancellationToken ct = default)
        {
            if (amount <= 0)
                return;

            var sub = await _context.UserSubscriptions
                                    .FirstOrDefaultAsync(x => x.Id == userSubscriptionId, ct);

            if (sub == null)
                throw CustomExceptionFactory.CreateNotFoundError("User subscription.");

            // Nếu null = unlimited -> không trừ, coi như luôn còn
            if (!sub.CompanyShareLimitSnapshot.HasValue)
                return;

            if (sub.CompanyShareLimitSnapshot.Value < amount)
                throw CustomExceptionFactory.CreateBadRequestError("No remaining company shares for this subscription.");

            sub.CompanyShareLimitSnapshot -= amount;
            sub.UpdatedAt = DateTimeOffset.UtcNow;

            await _context.SaveChangesAsync(ct);
        }
        public async Task<int> UpdateEnabledByFeatureIdAsync(Guid featureId, bool newStatus, CancellationToken ct = default)
        {
            var ents = await _context.CompanySubscriptionEntitlements
                       .Where(e => e.FeatureId == featureId)
                       .ToListAsync(ct);

            if (ents.Count == 0)
                return 0;

            foreach (var e in ents)
            {
                if (e.Enabled != newStatus)
                {
                    e.Enabled = newStatus;
                }
            }


            return ents.Count;
        }
        public async Task UseFeatureInUserAutoAsync(Guid userId, string featureName, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(featureName))
                throw CustomExceptionFactory.CreateBadRequestError("Feature is required.");

            featureName = featureName.Trim();
            var now = DateTimeOffset.UtcNow;

            // Lấy tất cả gói active + chưa hết hạn của user, sort từ cũ -> mới
            var subs = await _context.UserSubscriptions
                .Include(us => us.Entitlements)
                    .ThenInclude(e => e.Feature)
                .Where(us =>
                    us.UserId == userId &&
                    us.Status == SubscriptionStatus.Active &&
                    (!us.TermEnd.HasValue || us.TermEnd >= now))
                .OrderBy(us => us.CreatedAt)
                .ToListAsync(ct);

            if (subs.Count == 0)
                throw CustomExceptionFactory.CreateBadRequestError("User has no active subscription.");

            // Duyệt theo thứ tự cũ -> mới và kiếm gói đầu tiên có entitlement chứa feature
            foreach (var sub in subs)
            {
                var entitlement = sub.Entitlements
                    .FirstOrDefault(e =>
                        e.Enabled &&
                        e.Feature != null &&
                        e.Feature.Name != null &&
                        e.Feature.Name.Equals(featureName, StringComparison.OrdinalIgnoreCase));

                if (entitlement != null)
                {
                    return;
                }
            }

            // Không có entitlement nào trong toàn bộ các gói active
            throw CustomExceptionFactory.CreateBadRequestError(
                $"Feature '{featureName}' is not enabled in any active user subscription.");
        }
        public async Task<List<UserSubscription>> GetInstallmentReactivateCandidatesAsync(DateTimeOffset now, CancellationToken ct = default)
        {
            return await _context.UserSubscriptions
                .Where(us =>
                    us.PaymentModeSnapshot == PaymentMode.Installments &&
                    us.Status == SubscriptionStatus.Pending &&            // đang bị pending
                    us.NextPaymentDueAt.HasValue &&
                    us.NextPaymentDueAt >= now &&                         // đã được cập nhật lại kỳ mới (>= hiện tại)
                    (!us.TermEnd.HasValue || us.TermEnd >= now))          // chưa hết hạn gói
                .ToListAsync(ct);
        }
        public async Task<List<UserSubscription>> GetByPlanIdAsync(Guid planId,CancellationToken ct = default)
        {
            if (planId == Guid.Empty)
                return new List<UserSubscription>();

            return await _context.UserSubscriptions
                .Where(us => us.PlanId == planId)
                .ToListAsync(ct);
        }
    }
}
