

using Fusion.Repository.Bases.Exceptions;
using Fusion.Repository.Bases.Page;
using Fusion.Repository.Bases.Page.CompanySubscriptions;
using Fusion.Repository.Data;
using Fusion.Repository.Entities;
using Fusion.Repository.Enums;
using Fusion.Repository.IRepositories;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.Design;

namespace Fusion.Repository.Repositories
{
    public class CompanySubscriptionRepository : GenericRepository<CompanySubscription>, ICompanySubscriptionRepository
    {
        private readonly FusionDbContext _context;
        private readonly IUserSubscriptionRepository _userSubscriptionRepository;
        private readonly ICompanySubscriptionEntryRepository _entry;

        public CompanySubscriptionRepository(FusionDbContext context, IUserSubscriptionRepository userSubscriptionRepository
            , ICompanySubscriptionEntryRepository entry) : base(context)
        {
            _context = context;
            _userSubscriptionRepository = userSubscriptionRepository;
            _entry = entry;
        }
        private async Task<bool> TryUseFeatureFromSubscriptionsAsync(IList<UserSubscription> subs,string featureName,CancellationToken ct)
        {
            foreach (var sub in subs)
            {
                var entitlement = sub.Entitlements
                    .FirstOrDefault(e =>
                        e.Enabled &&
                        e.Feature != null &&
                        !string.IsNullOrEmpty(e.Feature.Name) &&
                        e.Feature.Name.Equals(featureName, StringComparison.OrdinalIgnoreCase));

                if (entitlement == null)
                    continue;

                // 1) Unlimited: MonthlyLimit == null => cho dùng, không trừ gì
                if (!entitlement.MonthlyLimit.HasValue)
                {
                    return true;
                }

                // 2) Có giới hạn/tháng: dùng MonthlyLimit như "số lượt còn lại"
                if (entitlement.MonthlyLimit.Value > 0)
                {
                    entitlement.MonthlyLimit -= 1; // trừ 1 lượt

                    await _context.SaveChangesAsync(ct);
                    return true;
                }

                // 3) MonthlyLimit <= 0 => hết lượt, thử subscription tiếp theo
            }

            return false;
        }
        private static bool IsAutoMonthlySubscription(UserSubscription sub)
        {
            return sub.CreatedByTransactionId == null;
        }
        public async Task UseFeatureInCompanyAutoAsync(Guid ActorUserId, Guid companyId,string featureName,CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(featureName))
                throw CustomExceptionFactory.CreateBadRequestError("Feature code is required.");

            featureName = featureName.Trim();
            var now = DateTimeOffset.UtcNow;

            // Lấy tất cả gói active + chưa hết hạn của company
            // ƯU TIÊN auto-month: ExpiredAt == null sẽ có HasValue = false -> đứng trước
            var subs = await _context.CompanySubscriptions
                .Include(cs => cs.Entitlements)
                    .ThenInclude(e => e.Feature)
                .Where(cs =>
                    cs.CompanyId == companyId &&
                    cs.Status == SubscriptionStatus.Active &&
                    (!cs.ExpiredAt.HasValue || cs.ExpiredAt >= now))
                .OrderBy(cs => cs.ExpiredAt.HasValue) // auto-month (null) trước, có hạn sau
                .ThenBy(cs => cs.SharedOn)            // trong mỗi nhóm: cũ -> mới
                .ToListAsync(ct);

            if (subs.Count == 0)
                throw CustomExceptionFactory.CreateBadRequestError("Company has no active subscription.");

            // Duyệt theo thứ tự (auto-month trước, sau đó mới tới các gói thường)
            foreach (var sub in subs)
            {
                var entitlement = sub.Entitlements
                    .FirstOrDefault(e =>
                        e.Enabled &&
                        e.Feature != null &&
                        e.Feature.Name != null &&
                        e.Feature.Name.Equals(featureName, StringComparison.OrdinalIgnoreCase));

                if (entitlement == null)
                    continue;

                // Có MonthlyLimit => phải còn quota mới dùng được
                if (entitlement.MonthlyLimit.HasValue)
                {
                    if (entitlement.MonthlyLimit.Value <= 0)
                    {
                        // hết quota ở sub này, thử qua sub khác
                        continue;
                    }

                    // trừ quota 1 lần dùng
                    entitlement.MonthlyLimit -= 1;
                }
                // MonthlyLimit == null => vô cực, không trừ

                // Ghi usage entry cho lần dùng này
                await _entry.CreateAsync(sub.Id, ActorUserId, companyId, ct);

                return;
            }

            // Không có entitlement nào trong toàn bộ các gói active
            throw CustomExceptionFactory.CreateBadRequestError(
                $"Feature '{featureName}' is not enabled in any active company subscription.");
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

            var autoMonthSubs = subs
                           .Where(IsAutoMonthlySubscription) // TODO: chỉnh điều kiện cho đúng model của anh
                           .ToList();

            // 2. Các gói còn lại
            var otherSubs = subs.Except(autoMonthSubs).ToList();

            // Thử dùng từ nhóm auto-month trước
            if (await TryUseFeatureFromSubscriptionsAsync(autoMonthSubs, featureName, ct))
                return;

            // Nếu không dùng được (không có feature hoặc hết limit) thì mới dùng các gói khác
            if (await TryUseFeatureFromSubscriptionsAsync(otherSubs, featureName, ct))
                return;

            // Không có entitlement nào trong toàn bộ các gói active
            throw CustomExceptionFactory.CreateBadRequestError(
                $"Feature '{featureName}' is not enabled in any active user subscription.");
        }
        public async Task<CompanySubscription> CreateAsync( CompanySubscription companySubscription, CancellationToken cancellationToken = default)
        {
            // 1. Load UserSubscription + Entitlements
            var userSub = await _userSubscriptionRepository
                .GetByIdAsync(companySubscription.UserSubscriptionId, cancellationToken);

            if (userSub == null)
                throw CustomExceptionFactory.CreateNotFoundError("User subscription not found");

            if (userSub.Status != SubscriptionStatus.Active)
                throw CustomExceptionFactory.CreateBadRequestError("User subscription is not active.");

            if (userSub.TermEnd.HasValue && userSub.TermEnd.Value < DateTimeOffset.UtcNow)
                throw CustomExceptionFactory.CreateBadRequestError("User subscription has expired.");

            // === PHÂN BIỆT AUTO-MONTH vs GÓI MUA ===
            // Giả định: auto-month = CreatedByTransactionId == null
            var isAutoMonthly = userSub.CreatedByTransactionId == null;

            // 2. Check share limit - CHỈ áp dụng cho gói mua (không phải auto-month)
            if (!isAutoMonthly && userSub.CompanyShareLimitSnapshot.HasValue)
            {
                if (userSub.CompanyShareLimitSnapshot.Value <= 0)
                    throw CustomExceptionFactory.CreateBadRequestError(
                        "No remaining company shares for this subscription.");
            }

            // 3. Kiểm tra share trùng (cái này vẫn giữ cho mọi loại)
            var exists = await _context.CompanySubscriptions
                .AnyAsync(cs =>
                    cs.UserSubscriptionId == companySubscription.UserSubscriptionId &&
                    cs.CompanyId == companySubscription.CompanyId,
                    cancellationToken);

            if (exists)
                throw CustomExceptionFactory.CreateBadRequestError(
                    "This subscription has already been shared to the specified company.");

            using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                // 4. Trừ hạn mức share NẾU là gói mua
                if (!isAutoMonthly && userSub.CompanyShareLimitSnapshot.HasValue)
                {
                    await _userSubscriptionRepository
                        .DecreaseCompanyShareLimitAsync(userSub.Id, 1, cancellationToken);
                }

                // 5. Gán thông tin CompanySubscription từ UserSubscription
                companySubscription.Status = SubscriptionStatus.Active;
                companySubscription.SharedOn = DateTimeOffset.UtcNow;
                companySubscription.UpdatedAt = DateTimeOffset.UtcNow;
                companySubscription.ExpiredAt = userSub.TermEnd; // auto-month có thể null

                // Nếu OwnerUserId chưa set (Guid.Empty) thì dùng chủ sở hữu userSub
                if (companySubscription.OwnerUserId == Guid.Empty)
                {
                    companySubscription.OwnerUserId = userSub.UserId;
                }

                companySubscription.SeatsLimitSnapshot = userSub.SeatsPerCompanyLimitSnapshot;
                // SeatsLimitUnit hiện chưa dùng, để null

                await _context.CompanySubscriptions.AddAsync(companySubscription, cancellationToken);

                // ===== COPY ENTITLEMENTS: LUÔN copy MonthlyLimit + LimitUnit =====
                var userEntitlements = userSub.Entitlements
                    .Where(e => e.Enabled)
                    .ToList();

                if (userEntitlements.Count > 0)
                {
                    IEnumerable<UserSubscriptionEntitlement> sourceEnts = userEntitlements;

                    // Với auto-month: chỉ giữ feature Company-level
                    if (isAutoMonthly)
                    {
                        sourceEnts = sourceEnts.Where(e =>
                            e.Feature != null &&
                            e.Feature.IsActive &&
                            !string.IsNullOrWhiteSpace(e.Feature.Category) &&
                            e.Feature.Category.Trim()
                                .Equals("Company", StringComparison.OrdinalIgnoreCase));
                    }

                    var companyEntitlements = sourceEnts.Select(e => new CompanySubscriptionEntitlement
                    {
                        Id = Guid.NewGuid(),
                        CompanySubscriptionId = companySubscription.Id,
                        FeatureId = e.FeatureId,
                        Enabled = true,

                        // >>> FIX 3: COPY MONTHLY LIMIT + UNIT <<<
                        MonthlyLimit = e.MonthlyLimit,
                        LimitUnit = e.LimitUnit
                    }).ToList();

                    if (companyEntitlements.Count > 0)
                    {
                        await _context.CompanySubscriptionEntitlements
                            .AddRangeAsync(companyEntitlements, cancellationToken);
                    }
                }

                // 6. Save & Commit
                await _context.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                return companySubscription;
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        }
        public async Task<List<CompanySubscription>> GetAllActiveByCompanyIdAsync(Guid companyId, CancellationToken ct = default)
        {
            return await _context.CompanySubscriptions
                     .AsNoTracking()
        .Include(cs => cs.UserSubscription)
            .ThenInclude(us => us.Plan)
        .Include(cs => cs.Entitlements
            .Where(e => e.Feature.Category != "User"))
            .ThenInclude(e => e.Feature)
        .Where(cs => cs.CompanyId == companyId &&
                     cs.Status == SubscriptionStatus.Active)
        .ToListAsync(ct);
        }
        public async Task<PagedResult<CompanySubscription>> GetAllByCompanyIdAsync(Guid companyId, CompanySubscriptionPagedRequest request, CancellationToken ct = default)
        {
            var q = _context.CompanySubscriptions
                            .AsNoTracking()
                            .Include(cs => cs.Company)
                            .Include(cs => cs.UserSubscription)
                              .ThenInclude(us => us.Plan)

                            .Include(cs => cs.UserSubscription)
                              .ThenInclude(us => us.User)
                            .Where(cs => cs.CompanyId == companyId);


            if (request.Status.HasValue)
            {
                q = q.Where(x => x.Status == request.Status.Value);
            }

            if (!string.IsNullOrWhiteSpace(request.Keyword))
            {
                var kw = request.Keyword.Trim();
                var like = $"%{kw}%";

                q = q.Where(x =>
                    (x.Company.Name != null && EF.Functions.Like(x.Company.Name, like)) ||
                    (x.UserSubscription.Plan.Name != null && EF.Functions.Like(x.UserSubscription.Plan.Name, like)));
            }

            if (!string.IsNullOrWhiteSpace(request.SortColumn) &&
                CompanySubscriptionPagedRequest.SortMap.TryGetValue(request.SortColumn, out var mapped))
            {
                request.SortColumn = mapped;
            }
            else if (string.IsNullOrWhiteSpace(request.SortColumn))
            {
                request.SortColumn = nameof(CompanySubscription.SharedOn);
                request.SortDescending = true;
            }

            return await q.ToPagedResultAsync(request, ct);

        }
        public async Task<CompanySubscription?> GetByIdWithNavAsync(Guid id, CancellationToken ct = default)
        {
            return await _context.CompanySubscriptions
                .AsNoTracking()
                .Include(cs => cs.Company)
                .Include(cs => cs.UserSubscription)
                    .ThenInclude(us => us.Plan)
                .Include(cs => cs.UserSubscription)
                    .ThenInclude(us => us.User)
                .Include(cs => cs.Entitlements
                    .Where(e => e.Feature.Category != "User"))
                    .ThenInclude(e => e.Feature)
                .FirstOrDefaultAsync(cs => cs.Id == id, ct);
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
        public async Task<List<CompanySubscription>> GetByUserSubscriptionIdsAsync( IEnumerable<Guid> userSubscriptionIds,CancellationToken ct = default)
        {
            var ids = (userSubscriptionIds ?? Enumerable.Empty<Guid>())
                .Where(x => x != Guid.Empty)
                .Distinct()
                .ToList();

            if (ids.Count == 0)
                return new List<CompanySubscription>();

            return await _context.CompanySubscriptions
                .Where(cs => ids.Contains(cs.UserSubscriptionId))
                .ToListAsync(ct);
        }
        public async Task<CompanySubscription?> FindByCompanyAndUserSubAsync(
             Guid companyId,
             Guid userSubscriptionId,
             CancellationToken ct = default)
        {
            if (companyId == Guid.Empty || userSubscriptionId == Guid.Empty)
                return null;

            return await _context.CompanySubscriptions
                .FirstOrDefaultAsync(cs =>
                    cs.CompanyId == companyId &&
                    cs.UserSubscriptionId == userSubscriptionId,
                    ct);
        }

        public async Task<List<CompanySubscriptionEntitlement>> GetEntitlementsByCompanySubIdAsync(
            Guid companySubscriptionId,
            CancellationToken ct = default)
        {
            if (companySubscriptionId == Guid.Empty)
                return new List<CompanySubscriptionEntitlement>();

            return await _context.CompanySubscriptionEntitlements
                .Where(e => e.CompanySubscriptionId == companySubscriptionId)
                .ToListAsync(ct);
        }

        public async Task BulkAddEntitlementsAsync( IEnumerable<CompanySubscriptionEntitlement> entitlements, CancellationToken ct = default)
        {
            if (entitlements == null)
                return;

            await _context.CompanySubscriptionEntitlements.AddRangeAsync(entitlements, ct);
        }

        public async Task<List<CompanySubscription>> GetAllActiveAutoMonthlyByPlanIdsWithEntitlementsAsync(
            IEnumerable<Guid> planIds,DateTimeOffset now,CancellationToken ct = default)
        {
            var ids = (planIds ?? Enumerable.Empty<Guid>())
                .Where(id => id != Guid.Empty)
                .Distinct()
                .ToList();

            if (ids.Count == 0)
                return new List<CompanySubscription>();

            return await _context.CompanySubscriptions
                .Include(cs => cs.UserSubscription)
                    .ThenInclude(us => us.Plan)
                .Include(cs => cs.Entitlements)
                    .ThenInclude(e => e.Feature)
                .Where(cs =>
                    cs.Status == SubscriptionStatus.Active &&
                    (!cs.ExpiredAt.HasValue || cs.ExpiredAt >= now) &&
                    cs.UserSubscription != null &&
                    cs.UserSubscription.CreatedByTransactionId == null &&   // auto-month user-sub
                    ids.Contains(cs.UserSubscription.PlanId))
                .ToListAsync(ct);
        }

        public Task<List<CompanySubscription>> GetAllActiveByUserSubscriptionAsync(Guid userSubscriptionId, CancellationToken ct = default)
        {
            return _context.CompanySubscriptions
            .Where(x => x.UserSubscriptionId == userSubscriptionId
                     && x.Status != SubscriptionStatus.Paused)
            .ToListAsync(ct);
        }
    }
}
