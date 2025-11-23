
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

    public UserSubscriptionService(
     IUserSubscriptionRepository repo,
     ITransactionPaymentRepository txRepo,
     ISubscriptionPlanRepository planRepo,
     IFeatureCatalogService featureCatalog,
     IMapper mapper, IUnitOfWork unitOfWork, ICurrentService current)
    {
        _repo = repo;
        _txRepo = txRepo;
        _planRepo = planRepo;
        _featureCatalog = featureCatalog;
        _mapper = mapper;
        _unitOfWork = unitOfWork;
        _current = current;
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
}