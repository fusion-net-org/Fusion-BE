
using AutoMapper;
using Fusion.Repository.Bases.Exceptions;
using Fusion.Repository.Bases.Page;
using Fusion.Repository.Bases.Page.UserSubscriptions;
using Fusion.Repository.Bases.Responses;
using Fusion.Repository.Data;
using Fusion.Repository.Entities;
using Fusion.Repository.Enums;
using Fusion.Repository.IRepositories;
using Fusion.Service.Commons.Helpers;
using Fusion.Service.IServices;
using Fusion.Service.ViewModels.UserSubscription.Requests;
using Fusion.Service.ViewModels.UserSubscription.Responses;

namespace Fusion.Service.Services;

public class UserSubscriptionService : IUserSubscriptionService
{
    private readonly IUserSubscriptionRepository _repo;
    private readonly ITransactionPaymentRepository _txRepo;
    private readonly ISubscriptionPlanRepository _planRepo;
    private readonly IFeatureCatalogService _featureCatalog;
    private readonly IMapper _mapper;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentService _current;
    private readonly ICompanySubscriptionRepository _companyRepo;

    public UserSubscriptionService(
     IUserSubscriptionRepository repo,
     ITransactionPaymentRepository txRepo,
     ISubscriptionPlanRepository planRepo,
     IFeatureCatalogService featureCatalog,
     IMapper mapper, IUnitOfWork unitOfWork, ICurrentService current, ICompanySubscriptionRepository companyRepo)
    {
        _repo = repo;
        _txRepo = txRepo;
        _planRepo = planRepo;
        _featureCatalog = featureCatalog;
        _mapper = mapper;
        _unitOfWork = unitOfWork;
        _current = current;
        _companyRepo = companyRepo;
    }

    private async Task<List<UserSubscriptionEntitlement>> BuildEntitlementsSnapshotAsync(
        SubscriptionPlan plan,
        Guid userSubscriptionId,
        CancellationToken ct)
    {
        // Full package => lấy toàn bộ feature active trong hệ thống
        if (plan.IsFullPackage)
        {
            var actives = await _featureCatalog.GetAllActiveAsync(ct);
            return actives.Select(f => new UserSubscriptionEntitlement
            {
                Id = Guid.NewGuid(),
                UserSubscriptionId = userSubscriptionId,
                FeatureId = f.Id,
                Enabled = true,
                MonthlyLimit = null,      // gói full: không giới hạn
                LimitUnit = null
            }).ToList();
        }

        // Custom plan => lấy từ SubscriptionPlan.Features (đã load kèm Feature)
        var planFeatures = plan.Features?
            .Where(pf => pf.Enabled &&
                         pf.Feature != null &&
                         pf.Feature.IsActive)
            .ToList()
            ?? new List<SubscriptionPlanFeature>();

        return planFeatures.Select(pf => new UserSubscriptionEntitlement
        {
            Id = Guid.NewGuid(),
            UserSubscriptionId = userSubscriptionId,
            FeatureId = pf.FeatureId,
            Enabled = true,
            MonthlyLimit = pf.MonthlyLimit,  // copy limit config ở PlanFeature
            LimitUnit = null                 // nếu sau này có unit riêng thì map thêm
        }).ToList();
    }
    public async Task<UserSubscriptionDetailResponse> CreateAsync(UserSubscriptionCreateRequest req, CancellationToken ct = default)
    {
        if (req == null || req.TransactionId == Guid.Empty)
            throw CustomExceptionFactory.CreateBadRequestError(ResponseMessages.INVALID_INPUT);

        // Load transaction kèm Plan(+Price, +Features)
        var tx = await _txRepo.GetByIdWithNavAsync(req.TransactionId, ct);
        if (tx == null)
            throw CustomExceptionFactory.CreateNotFoundError(ResponseMessages.NOT_FOUND.FormatMessage("Transaction"));
        if (tx.Status != PaymentStatus.Success)
            throw CustomExceptionFactory.CreateBadRequestError("Transaction is not successful.");
        if (tx.PaidAt == null && tx.TransactionDateTime == null)
            throw CustomExceptionFactory.CreateBadRequestError("Transaction paid time not found.");

        var plan = tx.SubscriptionPlan;
        if (plan == null)
            throw CustomExceptionFactory.CreateNotFoundError(ResponseMessages.NOT_FOUND.FormatMessage("Subscription plan"));
        if (plan.Price == null)
            throw CustomExceptionFactory.CreateBadRequestError("Plan has no price.");

        //var existingActive = await _repo.GetActiveByUserAsync(tx.UserId, ct);
        //if (existingActive != null)
        //    throw CustomExceptionFactory.CreateBadRequestError("You already have an active subscription.");

        // Snapshot + term
        var paidAt = tx.PaidAt ?? tx.TransactionDateTime ?? DateTimeOffset.UtcNow;
        var termStart = paidAt;
        var termEnd = AddInterval(termStart, tx.BillingPeriodSnapshot, tx.PeriodCountSnapshot);

        var sub = new UserSubscription
        {
            Id = Guid.NewGuid(),
            UserId = tx.UserId,
            PlanId = tx.PlanId,
            CreatedByTransactionId = tx.Id,

            Status = SubscriptionStatus.Active,
            TermStart = termStart,
            TermEnd = termEnd,
            NextPaymentDueAt = (tx.PaymentModeSnapshot == PaymentMode.Installments)
             ? ComputeNextDueDateAfterSuccess(tx)
             : null,

            // Snapshots (Plan)
            LicenseScopeSnapshot = plan.LicenseScope,
            IsFullPackageSnapshot = plan.IsFullPackage,
            CompanyShareLimitSnapshot = plan.CompanyShareLimit,
            SeatsPerCompanyLimitSnapshot = plan.SeatsPerCompanyLimit,

            // Snapshots (Price) — dùng snapshot từ transaction (nguồn sự thật tại thời điểm mua)
            ChargeUnitSnapshot = tx.ChargeUnitSnapshot,
            BillingPeriodSnapshot = tx.BillingPeriodSnapshot,
            PeriodCountSnapshot = tx.PeriodCountSnapshot,
            PaymentModeSnapshot = tx.PaymentModeSnapshot,
            InstallmentCountSnapshot = tx.InstallmentTotal,
            InstallmentIntervalSnapshot = tx.PaymentModeSnapshot == PaymentMode.Installments
               ? plan.Price.InstallmentInterval
               : null,

            CurrencySnapshot = (tx.Currency ?? plan.Price.Currency) ?? "VND",
            UnitPriceSnapshot = plan.Price.Price, // tổng giá gói tại thời điểm mua
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        // Persist subscription
        var created = await _repo.CreateAsync(sub, ct);

        // Build entitlements snapshot
        var entIds = await BuildEntitlementFeatureIdsAsync(plan, ct);
        var ents = entIds.Select(fid => new UserSubscriptionEntitlement
        {
            Id = Guid.NewGuid(),
            UserSubscriptionId = created.Id,
            FeatureId = fid,
            Enabled = true
        });
        await _repo.BulkAddEntitlementsAsync(ents, ct);

        // Trả kết quả
        var withNav = await _repo.GetByIdWithNavAsync(created.Id, ct);
        return _mapper.Map<UserSubscriptionDetailResponse>(withNav!);
    }

    //================= User for background Service =================//
    #region
    /// <summary>
    /// Dùng cho background service khi user mới được tạo.
    /// Cấp tất cả các plan AutoGrantMonthly (nếu chưa có).
    /// Idempotent: nếu user đã có sub active cho plan đó thì bỏ qua.
    /// </summary>
    public async Task<int> EnsureAutoMonthlyForUserAsync(Guid userId, CancellationToken ct = default)
    {
        // 1. Lấy tất cả plan auto-grant-monthly (đã include Price + Features)
        var autoPlans = await _planRepo.GetAllAutoGrantMonthlyAsync(ct);
        if (autoPlans == null || autoPlans.Count == 0)
            return 0;

        var planIds = autoPlans.Select(p => p.Id).ToList();

        // 2. Lấy các subscription active hiện có của user cho những plan này
        var existingSubs = await _repo.GetByUserAndPlanIdsAsync(userId, planIds, ct);
        var existingPlanIds = existingSubs.Select(s => s.PlanId).ToHashSet();

        var now = DateTimeOffset.UtcNow;
        var createdCount = 0;

        foreach (var plan in autoPlans)
        {
            if (existingPlanIds.Contains(plan.Id))
                continue; // user đã có sub cho plan này

            if (plan.Price == null)
                throw CustomExceptionFactory.CreateInternalServerError(
                    $"Auto-grant plan '{plan.Name}' has no price configured.");

            var price = plan.Price;

            // 3. Tạo UserSubscription từ plan (không có transaction payment)
            var sub = new UserSubscription
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                PlanId = plan.Id,
                CreatedByTransactionId = null,   // free / system created

                Status = SubscriptionStatus.Active,
                TermStart = now,
                TermEnd = null,                  // free plan không hết hạn, chỉ reset limit hằng tháng
                NextPaymentDueAt = null,

                // Snapshots (Plan)
                LicenseScopeSnapshot = plan.LicenseScope,
                IsFullPackageSnapshot = plan.IsFullPackage,
                CompanyShareLimitSnapshot = plan.CompanyShareLimit,
                SeatsPerCompanyLimitSnapshot = plan.SeatsPerCompanyLimit,

                // Snapshots (Price) lấy trực tiếp từ plan.Price
                ChargeUnitSnapshot = price.ChargeUnit,
                BillingPeriodSnapshot = price.BillingPeriod,
                PeriodCountSnapshot = price.PeriodCount,
                PaymentModeSnapshot = price.PaymentMode,
                InstallmentCountSnapshot = price.InstallmentCount,
                InstallmentIntervalSnapshot = price.InstallmentInterval,

                CurrencySnapshot = price.Currency ?? "VND",
                UnitPriceSnapshot = price.Price,

                CreatedAt = now,
                UpdatedAt = now
            };

            // Persist subscription
            var created = await _repo.CreateAsync(sub, ct);

            var ents = await BuildEntitlementsSnapshotAsync(plan, created.Id, ct);
            if (ents.Count > 0)
            {
                await _repo.BulkAddEntitlementsAsync(ents, ct);
            }

            createdCount++;
        }

        await _unitOfWork.SaveChangesAsync(ct);
        return createdCount;
    }
    /// <summary>
    /// Dùng cho background job chạy hằng tháng.
    /// Reset lại UserSubscriptionEntitlement.MonthlyLimit cho
    /// tất cả subscriptions thuộc các plan AutoGrantMonthly.
    /// </summary>
    public async Task<int> ResetAutoMonthlyEntitlementsAsync(CancellationToken ct = default)
    {
        var now = DateTimeOffset.UtcNow;

        // 1. Lấy tất cả plan auto-grant + features (đã có MonthlyLimit)
        var plans = await _planRepo.GetAllAutoGrantMonthlyAsync(ct);
        if (plans == null || plans.Count == 0)
            return 0;

        var planDict = plans.ToDictionary(p => p.Id);
        var planIds = planDict.Keys.ToList();

        // 2. Lấy tất cả user-subscription active thuộc các plan này + entitlements
        var subs = await _repo.GetAllActiveByPlanIdsWithEntitlementsAsync(planIds, ct);
        if (subs == null || subs.Count == 0)
            return 0;

        var updatedEntitlements = 0;

        foreach (var sub in subs)
        {
            if (!planDict.TryGetValue(sub.PlanId, out var plan) || plan.Features == null)
                continue;

            // Map FeatureId -> SubscriptionPlanFeature
            var featMap = plan.Features
                .Where(pf => pf.Feature != null && pf.Feature.IsActive)
                .ToDictionary(pf => pf.FeatureId, pf => pf);

            foreach (var ent in sub.Entitlements)
            {
                if (!featMap.TryGetValue(ent.FeatureId, out var pf))
                {
                    // Feature đã bị remove khỏi plan => tắt entitlement
                    if (ent.Enabled || ent.MonthlyLimit != null)
                    {
                        ent.Enabled = false;
                        ent.MonthlyLimit = null;
                        updatedEntitlements++;
                    }

                    continue;
                }

                // Cập nhật lại enable + monthly limit theo plan feature
                var changed = false;

                if (ent.Enabled != pf.Enabled)
                {
                    ent.Enabled = pf.Enabled;
                    changed = true;
                }

                if (ent.MonthlyLimit != pf.MonthlyLimit)
                {
                    ent.MonthlyLimit = pf.MonthlyLimit;  // reset limit về cấu hình gói
                    changed = true;
                }

                if (changed)
                {
                    updatedEntitlements++;
                }
            }

            // chỉ để audit, không dùng để chặn reset nữa
            sub.UpdatedAt = now;
        }

        await _unitOfWork.SaveChangesAsync(ct);

        return updatedEntitlements;
    }
    #endregion
    public async Task<UserSubscriptionDetailResponse?> GetActiveByUserAsync(Guid userId, CancellationToken ct = default)
    {
        var entity = await _repo.GetActiveByUserAsync(userId, ct);
        if (entity == null) return null;
        return _mapper.Map<UserSubscriptionDetailResponse>(entity);
    }
    public async Task<UserSubscriptionDetailResponse?> GetDetailAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await _repo.GetByIdWithNavAsync(id, ct);
        return entity == null ? null : _mapper.Map<UserSubscriptionDetailResponse>(entity);
    }
    public async Task<PagedResult<UserSubscriptionResponse>> GetPagedByUserIdAsync(UserSubscriptionPagedRequest request, CancellationToken ct = default)
    {
        var userId = _current.GetUserId();
        var paged = await _repo.GetPagedByUserIdAsync(userId, request, ct);
        return new PagedResult<UserSubscriptionResponse>
        {
            Items = _mapper.Map<List<UserSubscriptionResponse>>(paged.Items),
            PageNumber = paged.PageNumber,
            PageSize = paged.PageSize,
            TotalCount = paged.TotalCount
        };
    }
    public async Task<bool> CancelAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await _repo.GetByIdWithNavAsync(id, ct);
        if (entity == null) return false;
        entity.Status = SubscriptionStatus.Canceled;
        entity.CanceledAt = DateTimeOffset.UtcNow;
        return await _repo.UpdateAsync(entity, ct);
    }
    public async Task<bool> PauseAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await _repo.GetByIdWithNavAsync(id, ct);
        if (entity == null) return false;
        if (entity.Status != SubscriptionStatus.Active) return false;
        entity.Status = SubscriptionStatus.Paused;
        return await _repo.UpdateAsync(entity, ct);
    }
    public async Task<bool> ResumeAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await _repo.GetByIdWithNavAsync(id, ct);
        if (entity == null) return false;
        if (entity.Status != SubscriptionStatus.Paused) return false;
        entity.Status = SubscriptionStatus.Active;
        return await _repo.UpdateAsync(entity, ct);
    }
    public async Task UpdateNextDueAsync(Guid subId, DateTimeOffset? nextDueAt, CancellationToken ct = default)
    {
        await _repo.UpdateNextDueAsync(subId, nextDueAt, ct);
    }
    // ===== Helpers =====
    private static DateTimeOffset AddInterval(DateTimeOffset start, BillingPeriod period, int count)
    {
        return period switch
        {
            BillingPeriod.Week => start.AddDays(7 * count),
            BillingPeriod.Month => start.AddMonths(count),
            BillingPeriod.Year => start.AddYears(count),
            _ => start
        };
    }
    private DateTimeOffset? ComputeNextDueDateAfterSuccess(TransactionPayment paidTx)
    {
        if (paidTx.PaymentModeSnapshot != PaymentMode.Installments) return null;

        // Lấy đúng interval của installment (ưu tiên snapshot trên Transaction; fallback về Plan.Price)
        var interval = paidTx.SubscriptionPlan?.Price?.InstallmentInterval
                     ?? paidTx.BillingPeriodSnapshot; // cuối cùng mới fallback

        if (!(paidTx.InstallmentIndex.HasValue && paidTx.InstallmentTotal.HasValue)) return null;

        int i = paidTx.InstallmentIndex.Value;
        int n = paidTx.InstallmentTotal.Value;
        if (i >= n) return null; // đã là kỳ cuối thì không còn kỳ kế tiếp

        // Tính tổng đơn vị thời gian theo interval (ví dụ: Year*3 -> 36 tháng nếu interval = Month)
        int totalUnits = ConvertToUnits(paidTx.BillingPeriodSnapshot, paidTx.PeriodCountSnapshot, interval);

        // Bước mỗi kỳ = tổngUnits / sốKỳ (làm tròn lên và luôn >= 1)
        int stepUnits = Math.Max(1, (int)Math.Ceiling(totalUnits / (double)n));

        // Base là DueAt (nếu đã set lịch), nếu chưa thì lấy PaidAt, cuối cùng mới UtcNow
        var baseDue = paidTx.DueAt ?? paidTx.PaidAt ?? DateTimeOffset.UtcNow;

        // Next = base + stepUnits * interval
        return AddInterval(baseDue, interval, stepUnits);
    }

    // Chuyển đổi "tổng thời hạn" (BillingPeriod × PeriodCount) sang số đơn vị của 'toUnit'
    private static int ConvertToUnits(BillingPeriod period, int count, BillingPeriod toUnit)
    {
        // Ưu tiên các cặp quen thuộc: Year<->Month, Month<->Week
        switch (toUnit)
        {
            case BillingPeriod.Month:
                return period switch
                {
                    BillingPeriod.Year => count * 12,
                    BillingPeriod.Month => count,
                    BillingPeriod.Week => (int)Math.Ceiling(count * 7.0 / 30.0),
                    _ => count
                };

            case BillingPeriod.Year:
                return period switch
                {
                    BillingPeriod.Year => count,
                    BillingPeriod.Month => (int)Math.Ceiling(count / 12.0),
                    BillingPeriod.Week => (int)Math.Ceiling(count / 52.0),
                    _ => count
                };

            case BillingPeriod.Week:
                return period switch
                {
                    BillingPeriod.Week => count,
                    BillingPeriod.Month => (int)Math.Ceiling(count * 30.0 / 7.0),
                    BillingPeriod.Year => (int)Math.Ceiling(count * 365.0 / 7.0),
                    _ => count
                };

            default:
                return count;
        }
    }
    private async Task<List<Guid>> BuildEntitlementFeatureIdsAsync(SubscriptionPlan plan, CancellationToken ct)
    {
        if (plan.IsFullPackage)
        {
            var actives = await _featureCatalog.GetAllActiveAsync(ct);
            return actives.Select(f => f.Id).Distinct().ToList();
        }
        var ids = plan.Features?
            .Where(f => f.Enabled)
            .Select(f => f.FeatureId)
            .Distinct()
            .ToList() ?? new List<Guid>();
        return ids;
    }
    public async Task DecreaseCompanyShareLimitAsync(Guid userSubscriptionId, int amount = 1, CancellationToken ct = default)
    {
        await _repo.DecreaseCompanyShareLimitAsync(userSubscriptionId, amount, ct);
    }
    public async Task<List<UserSubscriptionActiveResponse>> GetAllActiveByUserIdAsync(CancellationToken ct = default)
    {
        var userId = _current.GetUserId();
        var entities = await _repo.GetAllActiveByUserIdAsync(userId, ct);

        return _mapper.Map<List<UserSubscriptionActiveResponse>>(entities);
    }
    public async Task<int> SyncSubscriptionStatusesByTimeAsync(CancellationToken ct = default)
    {
        var now = DateTimeOffset.UtcNow;
        var updated = 0;

        // ===== 1) Hết hạn term -> Expired (mọi PaymentMode) =====
        var expiredCandidates = await _repo.GetExpiredByTermCandidatesAsync(now, ct);

        var expiredUserSubIds = new List<Guid>();

        foreach (var us in expiredCandidates)
        {
            if (us.Status != SubscriptionStatus.Expired)
            {
                us.Status = SubscriptionStatus.Expired;
                us.TermEnd ??= now;
                us.UpdatedAt = now;

                expiredUserSubIds.Add(us.Id);
                updated++;
            }
        }
        // 2) Trễ kỳ installment -> Pending
        var pendingCandidates = await _repo.GetInstallmentPendingCandidatesAsync(now, ct);

        // Nếu sub vừa set Expired ở trên thì không set Pending nữa
        pendingCandidates = pendingCandidates
            .Where(us => !expiredUserSubIds.Contains(us.Id))
            .ToList();

        var pendingUserSubIds = new List<Guid>();

        foreach (var us in pendingCandidates)
        {
            if (us.Status != SubscriptionStatus.Pending)
            {
                us.Status = SubscriptionStatus.Pending;
                us.UpdatedAt = now;

                pendingUserSubIds.Add(us.Id);
                updated++;
            }
        }

        // ===== 3) Installments đã trả xong kỳ trễ -> Active lại =====
        var reactivateCandidates = await _repo.GetInstallmentReactivateCandidatesAsync(now, ct);

        // Loại những thằng đã expired
        reactivateCandidates = reactivateCandidates
            .Where(us => !expiredUserSubIds.Contains(us.Id))
            .ToList();

        var reactivatedUserSubIds = new List<Guid>();

        foreach (var us in reactivateCandidates)
        {
            if (us.Status != SubscriptionStatus.Active)
            {
                us.Status = SubscriptionStatus.Active;
                us.UpdatedAt = now;

                reactivatedUserSubIds.Add(us.Id);
                updated++;
            }
        }

        // ===== 4) Cascade xuống CompanySubscription =====
        var affectedUserSubIds = expiredUserSubIds
            .Concat(pendingUserSubIds)
            .Concat(reactivatedUserSubIds)
            .Distinct()
            .ToList();


        if (affectedUserSubIds.Count > 0)
        {
            var companySubs = await _companyRepo.GetByUserSubscriptionIdsAsync(affectedUserSubIds, ct);

            foreach (var cs in companySubs)
            {
                if (expiredUserSubIds.Contains(cs.UserSubscriptionId))
                {
                    if (cs.Status != SubscriptionStatus.Expired)
                    {
                        cs.Status = SubscriptionStatus.Expired;
                        cs.ExpiredAt ??= now;
                        cs.UpdatedAt = now;
                        updated++;
                    }
                }
                else if (pendingUserSubIds.Contains(cs.UserSubscriptionId))
                {
                    if (cs.Status != SubscriptionStatus.Pending)
                    {
                        cs.Status = SubscriptionStatus.Pending;
                        cs.UpdatedAt = now;
                        updated++;
                    }
                }
                else if (reactivatedUserSubIds.Contains(cs.UserSubscriptionId))
                {
                    // Subscription đã active lại, company-sub mà chưa hết hạn thì cho active lại luôn
                    if ((cs.Status == SubscriptionStatus.Pending || cs.Status == SubscriptionStatus.Paused) &&
                        (!cs.ExpiredAt.HasValue || cs.ExpiredAt >= now))
                    {
                        cs.Status = SubscriptionStatus.Active;
                        cs.UpdatedAt = now;
                        updated++;
                    }
                }
            }
        }

        await _unitOfWork.SaveChangesAsync(ct);

        return updated;
    }
}