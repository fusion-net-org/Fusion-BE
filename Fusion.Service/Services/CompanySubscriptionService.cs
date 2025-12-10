
using AutoMapper;
using Fusion.Repository.Bases.Exceptions;
using Fusion.Repository.Bases.Page;
using Fusion.Repository.Bases.Page.CompanySubscriptions;
using Fusion.Repository.Bases.Responses;
using Fusion.Repository.Data;
using Fusion.Repository.Entities;
using Fusion.Repository.Enums;
using Fusion.Repository.IRepositories;
using Fusion.Repository.ViewModels.CompanySubscriptionEntry;
using Fusion.Service.Commons.Helpers;
using Fusion.Service.IServices;
using Fusion.Service.ViewModels.CompanySubscription.Requests;
using Fusion.Service.ViewModels.CompanySubscription.Responses;

namespace Fusion.Service.Services
{
    public class CompanySubscriptionService : ICompanySubscriptionService
    {
        private readonly ICompanySubscriptionRepository _companySubscriptionRepository;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IUserLogService _userLogService;
        private readonly ICompanySubscriptionEntryRepository _companyEntryRepo;
        private readonly IUserSubscriptionService _userSubService;
        private readonly IUserSubscriptionRepository _userSubRepo;
        private readonly ISubscriptionPlanRepository _planRepo;
        private readonly ICurrentService _currentService;


        public CompanySubscriptionService(ICompanySubscriptionRepository companySubscriptionRepository, IMapper mapper,
            IUnitOfWork unitOfWork, IUserLogService userLogService, ICompanySubscriptionEntryRepository companyEntryRepo,
            IUserSubscriptionService userSubService, IUserSubscriptionRepository userSubRepo,
            ISubscriptionPlanRepository planRepo,ICurrentService currentService)
        {
            _companySubscriptionRepository = companySubscriptionRepository;
            _mapper = mapper;
            _unitOfWork = unitOfWork;
            _userLogService = userLogService;
            _companyEntryRepo = companyEntryRepo;
            _userSubService = userSubService;
            _userSubRepo = userSubRepo;
            _planRepo = planRepo;
            _currentService = currentService;
        }

        public async Task<CompanySubscriptionDetailResponse> CreateAsync(CompanySubscriptionCreateRequest request, CancellationToken ct = default)
        {
            var userSubscription = await _unitOfWork.Repository<UserSubscription>().FindAsync(x => x.Id == request.UserSubscriptionId, ct);
            if (userSubscription == null)
                throw CustomExceptionFactory.CreateNotFoundError("User subscription.");

            if(request.OwnerUserId == Guid.Empty)
                throw CustomExceptionFactory.CreateNotFoundError("Owner.");

            if (userSubscription.UserId != request.OwnerUserId)
                throw CustomExceptionFactory.CreateForbiddenError();

            var entity = _mapper.Map<CompanySubscription>(request);
            entity.CompanyId = request.CompanyId;
            entity.UserSubscriptionId = request.UserSubscriptionId;
            entity.OwnerUserId = request.OwnerUserId;

            var createdEntity = await _companySubscriptionRepository.CreateAsync(entity, ct);

            // 4. Log hành động
            var company = await _unitOfWork.Repository<Company>()
                .FindAsync(x => x.Id == entity.CompanyId, ct);

            var userLog = new UserLog
            {
                ActorUserId = request.OwnerUserId,
                Title = "Create company subscription",
                Description =
                    $"User created a company subscription for company '{company?.Name}' (Id: {entity.CompanyId}) " +
                    $"from user subscription '{userSubscription.Plan?.Name}' (Id: {userSubscription.Id})."
            };
            await _userLogService.CreateLog(userLog, ct);


            // 5. Load lại đầy đủ nav để trả detail
            var full = await _companySubscriptionRepository.GetByIdWithNavAsync(createdEntity.Id, ct)
                       ?? createdEntity;

            return _mapper.Map<CompanySubscriptionDetailResponse>(full);
        }
        public async Task<List<CompanySubscriptionActiveResponse>> GetAllActiveByCompanyIdAsync(Guid companyId, CancellationToken ct = default)
        {
            var entities = await _companySubscriptionRepository.GetAllActiveByCompanyIdAsync(companyId, ct);

            return _mapper.Map<List<CompanySubscriptionActiveResponse>>(entities);
        }
        public async Task<PagedResult<CompanySubscriptionListResponse>> GetAllByCompanyAsync(Guid companyId, CompanySubscriptionPagedRequest request, CancellationToken ct = default)
        {
            var userId = _currentService.GetUserId();
            await EnsureAutoMonthlyForCompanyAsync(companyId, userId, ct);

            var entities = await _companySubscriptionRepository
            .GetAllByCompanyIdAsync(companyId, request, ct);

            return new PagedResult<CompanySubscriptionListResponse>
            {
                Items = _mapper.Map<List<CompanySubscriptionListResponse>>(entities.Items),
                TotalCount = entities.TotalCount,
                PageNumber = entities.PageNumber,
                PageSize = entities.PageSize
            };
        }
        public async Task<CompanySubscriptionDetailResponse?> GetDetailAsync(Guid id, CancellationToken ct = default)
        {
            var entity = await _companySubscriptionRepository.GetByIdWithNavAsync(id, ct);

            if (entity == null)
                throw CustomExceptionFactory.CreateNotFoundError("Company subscription.");

            return _mapper.Map<CompanySubscriptionDetailResponse>(entity);
        }
        public async Task<bool> UseFeatureInCompanyAutoAsync(UserFeatureRequest request, CancellationToken ct = default)
        {
            await _companySubscriptionRepository.UseFeatureInCompanyAutoAsync(request.ActorUserId,request.CompanyId, request.FeatureName, ct);
            return true;
        }
        public async Task<bool> UseFeatureInUserAutoAsync(Guid userId, string featureName, CancellationToken ct = default)
        {
            await _companySubscriptionRepository.UseFeatureInUserAutoAsync(userId, featureName, ct);
            return true;
        }
        public async Task<List<CompanySubscriptionUserUsageItem>> GetUserUsageAsync( Guid companySubscriptionId, CancellationToken ct = default)
        {
            if (companySubscriptionId == Guid.Empty)
                throw CustomExceptionFactory.CreateBadRequestError("CompanySubscriptionId is required.");

            var items = await _companyEntryRepo
                .GetUserUsageByCompanySubscriptionAsync(companySubscriptionId, ct);

            return _mapper.Map<List<CompanySubscriptionUserUsageItem>>(items);
        }
        private static List<CompanySubscriptionEntitlement> BuildCompanyEntitlementsSnapshot(UserSubscription userSub,Guid companySubscriptionId)
        {
            if (userSub == null) throw new ArgumentNullException(nameof(userSub));

            var sourceEnts = userSub.Entitlements?
                .Where(e =>
                    e.Enabled &&
                    e.Feature != null &&
                    e.Feature.IsActive &&
                    !string.IsNullOrWhiteSpace(e.Feature.Category) &&
                    e.Feature.Category.Trim()
                      .Equals("Company", StringComparison.OrdinalIgnoreCase))
                .ToList()
                ?? new List<UserSubscriptionEntitlement>();

            return sourceEnts.Select(e => new CompanySubscriptionEntitlement
            {
                Id = Guid.NewGuid(),
                CompanySubscriptionId = companySubscriptionId,
                FeatureId = e.FeatureId,
                Enabled = true,

                // snapshot quota tháng từ user entitlement
                MonthlyLimit = e.MonthlyLimit,
                LimitUnit = e.LimitUnit
            }).ToList();
        }
        public async Task<int> EnsureAutoMonthlyForCompanyAsync(Guid companyId,Guid ownerUserId,CancellationToken ct = default)
        {
            if (companyId == Guid.Empty || ownerUserId == Guid.Empty)
                throw CustomExceptionFactory.CreateBadRequestError(ResponseMessages.INVALID_INPUT);

            var now = DateTimeOffset.UtcNow;
            var createdCompanySubCount = 0;
            var createdEntitlementCount = 0;

            // 1) Đảm bảo owner đã có gói auto-month cho user
            await _userSubService.EnsureAutoMonthlyForUserAsync(ownerUserId, ct);

            // 2) Lấy user-sub auto-month kèm entitlements + feature
            var userSubs = await _userSubRepo.GetAutoMonthlyForUserWithEntitlementsAsync(ownerUserId, ct);
            if (userSubs == null || userSubs.Count == 0)
                return 0;

            // 3) Chỉ giữ sub Active và có ít nhất 1 feature Company-level
            var companyEligibleSubs = userSubs
                .Where(us =>
                    us.Status == SubscriptionStatus.Active &&
                    us.Entitlements.Any(e =>
                        e.Enabled &&
                        e.Feature != null &&
                        e.Feature.IsActive &&
                        !string.IsNullOrWhiteSpace(e.Feature.Category) &&
                        e.Feature.Category.Trim()
                          .Equals("Company", StringComparison.OrdinalIgnoreCase)))
                .ToList();

            if (companyEligibleSubs.Count == 0)
                return 0;

            foreach (var us in companyEligibleSubs)
            {
                var companySub = await _companySubscriptionRepository
                    .FindByCompanyAndUserSubAsync(companyId, us.Id, ct);

                // 4) Tạo company-sub nếu chưa có
                if (companySub == null)
                {
                    companySub = new CompanySubscription
                    {
                        Id = Guid.NewGuid(),
                        CompanyId = companyId,
                        UserSubscriptionId = us.Id,
                        OwnerUserId = ownerUserId,
                        Status = SubscriptionStatus.Active,
                        SharedOn = now,
                        UpdatedAt = now,
                        SeatsLimitSnapshot = us.SeatsPerCompanyLimitSnapshot,
                        SeatsLimitUnit = null,
                        // ExpiredAt = null  -> auto-month
                    };

                    // Vẫn gọi CreateAsync, nhưng CreateAsync giờ biết auto-month và KHÔNG trừ share limit
                    await _companySubscriptionRepository.CreateAsync(companySub, ct);
                    createdCompanySubCount++;
                }

                // 5) Fallback: nếu vì lý do gì đó chưa có entitlements thì snapshot từ user-sub
                var existingEnts = await _companySubscriptionRepository
                    .GetEntitlementsByCompanySubIdAsync(companySub.Id, ct);

                if (existingEnts == null || existingEnts.Count == 0)
                {
                    var ents = BuildCompanyEntitlementsSnapshot(us, companySub.Id);

                    if (ents.Count > 0)
                    {
                        await _companySubscriptionRepository.BulkAddEntitlementsAsync(ents, ct);
                        createdEntitlementCount += ents.Count;
                    }
                }
            }

            if (createdCompanySubCount > 0 || createdEntitlementCount > 0)
            {
                await _unitOfWork.SaveChangesAsync(ct);
            }

            return createdCompanySubCount;
        }
        public async Task<int> ResetCompanyAutoMonthlyEntitlementsAsync(CancellationToken ct = default)
        {
            var now = DateTimeOffset.UtcNow;

            var monthStart = new DateTimeOffset(
                now.Year, now.Month, 1,
                0, 0, 0,
                TimeSpan.Zero);

            // 1) Plans auto-grant-monthly
            var plans = await _planRepo.GetAllAutoGrantMonthlyAsync(ct);
            if (plans == null || plans.Count == 0)
                return 0;

            var planDict = plans.ToDictionary(p => p.Id);
            var planIds = planDict.Keys.ToList();

            // 2) CompanySubscriptions active thuộc các plan này (kèm UserSubscription + Entitlements)
            var companySubs = await _companySubscriptionRepository
                .GetAllActiveAutoMonthlyByPlanIdsWithEntitlementsAsync(planIds, now, ct);

            if (companySubs == null || companySubs.Count == 0)
                return 0;

            var updatedEntitlements = 0;

            foreach (var cs in companySubs)
            {
                // Đã reset trong tháng này rồi -> bỏ qua
                if (cs.UpdatedAt >= monthStart)
                    continue;

                var userSub = cs.UserSubscription;
                if (userSub == null)
                    continue;

                if (!planDict.TryGetValue(userSub.PlanId, out var plan) ||
                    plan.Features == null)
                    continue;

                // Lấy list feature Company-level trong plan
                var planFeatures = plan.Features
                    .Where(pf =>
                        pf.Feature != null &&
                        pf.Feature.IsActive &&
                        !string.IsNullOrWhiteSpace(pf.Feature.Category) &&
                        pf.Feature.Category.Trim()
                            .Equals("Company", StringComparison.OrdinalIgnoreCase))
                    .ToList();

                // Map FeatureId -> SubscriptionPlanFeature
                var featMap = planFeatures.ToDictionary(pf => pf.FeatureId, pf => pf);

                // Map FeatureId -> Entitlement hiện có
                var entMap = cs.Entitlements.ToDictionary(e => e.FeatureId, e => e);

                // 2.1. Update / disable entitlements đang có
                foreach (var ent in cs.Entitlements)
                {
                    if (!featMap.TryGetValue(ent.FeatureId, out var pf))
                    {
                        // Feature đã bị remove khỏi plan => disable entitlement
                        if (ent.Enabled || ent.MonthlyLimit != null)
                        {
                            ent.Enabled = false;
                            ent.MonthlyLimit = null;
                            updatedEntitlements++;
                        }

                        continue;
                    }

                    // Reset lại enable + quota/tháng theo cấu hình của plan
                    ent.Enabled = pf.Enabled;
                    ent.MonthlyLimit = pf.MonthlyLimit;
                    updatedEntitlements++;
                }

                // 2.2. THÊM entitlement cho feature mới có trong plan nhưng chưa có ở company
                foreach (var pf in planFeatures)
                {
                    if (entMap.ContainsKey(pf.FeatureId))
                        continue;

                    var newEnt = new CompanySubscriptionEntitlement
                    {
                        Id = Guid.NewGuid(),
                        CompanySubscriptionId = cs.Id,
                        FeatureId = pf.FeatureId,
                        Enabled = pf.Enabled,
                        MonthlyLimit = pf.MonthlyLimit,
                        LimitUnit = null,
                    };

                    cs.Entitlements.Add(newEnt);
                    updatedEntitlements++;
                }

                // Đánh dấu đã refresh cho tháng này
                cs.UpdatedAt = now;
            }

            if (updatedEntitlements > 0)
            {
                await _unitOfWork.SaveChangesAsync(ct);
            }

            return updatedEntitlements;
        }
    }
}