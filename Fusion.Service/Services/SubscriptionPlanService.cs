using AutoMapper;
using Fusion.Repository.Bases.Exceptions;
using Fusion.Repository.Bases.Page;
using Fusion.Repository.Bases.Page.SubscriptionPlans;
using Fusion.Repository.Data;
using Fusion.Repository.Entities;
using Fusion.Repository.Enums;
using Fusion.Repository.IRepositories;
using Fusion.Service.IServices;
using Fusion.Service.ViewModels.SubscriptionPlan.Requests;
using Fusion.Service.ViewModels.SubscriptionPlan.Responses;

namespace Fusion.Service.Services
{
    public class SubscriptionPlanService : ISubscriptionPlanService
    {
        private readonly ISubscriptionPlanRepository _subscriptionPlanRepository;
        private readonly IMapper _mapper;
        private readonly IFeatureCatalogService _featureCatalogService;
        private readonly IUserSubscriptionRepository _userSubscriptionRepository;
        private readonly ICompanySubscriptionRepository _companySubscriptionRepository;
        private readonly IUnitOfWork _unitOfWork;
        public SubscriptionPlanService(ISubscriptionPlanRepository subscriptionPlanRepository, IMapper mapper,
            IFeatureCatalogService featureCatalogService, IUserSubscriptionRepository userSubscriptionRepository,
            ICompanySubscriptionRepository companySubscriptionRepository, IUnitOfWork unitOfWork)
        {
            _subscriptionPlanRepository = subscriptionPlanRepository;
            _mapper = mapper;
            _featureCatalogService = featureCatalogService;
            _userSubscriptionRepository = userSubscriptionRepository;
            _companySubscriptionRepository = companySubscriptionRepository;
            _unitOfWork = unitOfWork;
        }

        private static void ValidatePlan(SubscriptionPlan p)
        {
            if (p.LicenseScope == LicenseScope.EntireCompany && p.SeatsPerCompanyLimit != null)
                throw CustomExceptionFactory.CreateBadRequestError("SeatsPerCompanyLimit must be null for EntireCompany plans.");

            if (p.CompanyShareLimit.HasValue && p.CompanyShareLimit < 0)
                throw CustomExceptionFactory.CreateBadRequestError("CompanyShareLimit must be >= 0 or null.");

            if (p.SeatsPerCompanyLimit.HasValue && p.SeatsPerCompanyLimit < 0)
                throw CustomExceptionFactory.CreateBadRequestError("SeatsPerCompanyLimit must be >= 0 or null.");
        }
        private static void ValidatePrice(SubscriptionPlanPrice pr)
        {
            if (pr.PeriodCount <= 0)
                throw CustomExceptionFactory.CreateBadRequestError("Price.PeriodCount must be > 0.");

            if (pr.Price < 0)
                throw CustomExceptionFactory.CreateBadRequestError("Price.Price must be >= 0.");

            if (pr.PaymentMode == PaymentMode.Installments)
            {
                if (pr.InstallmentCount is null || pr.InstallmentInterval is null)
                    throw CustomExceptionFactory.CreateBadRequestError("Installments require both InstallmentCount and InstallmentInterval.");
                if (pr.InstallmentCount <= 1)
                    throw CustomExceptionFactory.CreateBadRequestError("InstallmentCount must be > 1 when installments.");
            }
            else
            {
                if (pr.InstallmentCount is not null || pr.InstallmentInterval is not null)
                    throw CustomExceptionFactory.CreateBadRequestError("Prepaid price must not have installment fields.");
            }
        }
        private static void ValidateDiscounts(SubscriptionPlanPriceInput pr)
        {
            // Prepaid thì không được có discount
            if (pr.PaymentMode == PaymentMode.Prepaid)
            {
                if (pr.Discounts != null && pr.Discounts.Count > 0)
                    throw CustomExceptionFactory.CreateBadRequestError("Prepaid price must not have discounts.");
                return;
            }

            // Installments
            var discounts = pr.Discounts;
            if (discounts == null || discounts.Count == 0)
                return; // không cấu hình discount cũng ok

            var seenIndexes = new HashSet<int>();

            foreach (var d in discounts)
            {
                if (d.InstallmentIndex <= 0)
                    throw CustomExceptionFactory.CreateBadRequestError("Discount.InstallmentIndex must be > 0.");

                if (pr.InstallmentCount.HasValue && d.InstallmentIndex > pr.InstallmentCount.Value)
                    throw CustomExceptionFactory.CreateBadRequestError(
                        $"Discount.InstallmentIndex {d.InstallmentIndex} cannot be greater than InstallmentCount {pr.InstallmentCount}."
                    );

                if (!seenIndexes.Add(d.InstallmentIndex))
                    throw CustomExceptionFactory.CreateBadRequestError(
                        $"Duplicate discount for installment index {d.InstallmentIndex}."
                    );

                if (d.DiscountValue < 0 || d.DiscountValue > 100)
                    throw CustomExceptionFactory.CreateBadRequestError(
                        "DiscountValue must be between 0 and 100 (percent)."
                    );

                if (!string.IsNullOrWhiteSpace(d.Note) && d.Note.Length > 250)
                    throw CustomExceptionFactory.CreateBadRequestError("Discount.Note max length is 250 characters.");
            }
        }

        private async Task<List<SubscriptionPlanFeature>> BuildFeatureTogglesAsync(bool isFullPackage, List<Guid>? featureIds,
         List<SubscriptionPlanFeatureLimitInput>? featureLimits,CancellationToken ct)
        {
            // map featureId -> limit từ request.FE
            var limitMap = (featureLimits ?? new List<SubscriptionPlanFeatureLimitInput>())
                .Where(x => x.FeatureId != Guid.Empty)
                .GroupBy(x => x.FeatureId)
                .ToDictionary(g => g.Key, g => g.Last().MonthlyLimit);

            if (isFullPackage)
            {
                var actives = await _featureCatalogService.GetAllActiveAsync(ct);
                // Nếu không có feature active vẫn trả list rỗng (để repo replace đúng)
                return actives.Select(f => new SubscriptionPlanFeature
                {
                    FeatureId = f.Id,
                    Enabled = true,
                    MonthlyLimit = limitMap.TryGetValue(f.Id, out var ml) ? ml : (int?)null
                }).ToList();
            }

            if (featureIds == null || featureIds.Count == 0)
                return new List<SubscriptionPlanFeature>();

            var distinct = featureIds.Where(id => id != Guid.Empty).Distinct().ToList();
            return distinct.Select(fid => new SubscriptionPlanFeature
            {
                FeatureId = fid,
                Enabled = true,
                MonthlyLimit = limitMap.TryGetValue(fid, out var ml) ? ml : (int?)null
            }).ToList();
        }
        private async Task CascadePlanStatusToSubscriptionsAsync(SubscriptionPlan plan,CancellationToken ct)
        {
            if (plan == null) return;

            var now = DateTimeOffset.UtcNow;

            // 1) Lấy tất cả user-sub của plan này
            var userSubs = await _userSubscriptionRepository
                .GetByPlanIdAsync(plan.Id, ct);

            if (userSubs.Count == 0)
                return;

            var userSubIds = userSubs.Select(us => us.Id).ToList();

            // 2) Lấy tất cả company-sub tương ứng
            var companySubs = await _companySubscriptionRepository.GetByUserSubscriptionIdsAsync(userSubIds, ct);

            if (!plan.IsActive)
            {
                // ===== Plan -> INACTIVE =====
                foreach (var us in userSubs)
                {
                    switch (us.Status)
                    {
                        case SubscriptionStatus.Active:
                        case SubscriptionStatus.Pending:
                            us.Status = SubscriptionStatus.Paused;
                            us.UpdatedAt = now;
                            // Nếu muốn cắt hạn luôn:
                            // if (!us.TermEnd.HasValue || us.TermEnd > now) us.TermEnd = now;
                            break;

                            // Paused / Canceled / Expired -> giữ nguyên
                    }
                }

                foreach (var cs in companySubs)
                {
                    switch (cs.Status)
                    {
                        case SubscriptionStatus.Active:
                        case SubscriptionStatus.Pending:
                            cs.Status = SubscriptionStatus.Paused;
                            cs.UpdatedAt = now;
                            // Nếu muốn cắt share luôn:
                            // if (!cs.ExpiredAt.HasValue || cs.ExpiredAt > now) cs.ExpiredAt = now;
                            break;

                            // Paused / Canceled / Expired -> giữ nguyên
                    }
                }
            }
            else
            {
                // ===== Plan -> ACTIVE =====
                foreach (var us in userSubs)
                {
                    switch (us.Status)
                    {
                        case SubscriptionStatus.Paused:
                            // Chỉ revive nếu chưa hết hạn
                            if (!us.TermEnd.HasValue || us.TermEnd >= now)
                            {
                                us.Status = SubscriptionStatus.Active;
                                us.UpdatedAt = now;
                            }
                            break;

                            // Pending / Active / Canceled / Expired -> giữ nguyên
                    }
                }

                foreach (var cs in companySubs)
                {
                    switch (cs.Status)
                    {
                        case SubscriptionStatus.Paused:
                            if (!cs.ExpiredAt.HasValue || cs.ExpiredAt >= now)
                            {
                                cs.Status = SubscriptionStatus.Active;
                                cs.UpdatedAt = now;
                            }
                            break;

                            // Pending / Active / Canceled / Expired -> giữ nguyên
                    }
                }
            }

            await _unitOfWork.SaveChangesAsync(ct);
        }
        public async Task<SubscriptionPlanDetailResponse> CreatePlanAsync(SubscriptionPlanCreateRequest req, CancellationToken cancellationToken = default)
        {
            var entity = _mapper.Map<SubscriptionPlan>(req);
            entity.Price = _mapper.Map<SubscriptionPlanPrice>(req.Price);

            //   Quyết định features: full-package => lấy tất cả active;
            //    ngược lại => dùng FeatureIds (distinct, bỏ Guid.Empty, đồng thời có thể validate active).
            entity.Features = await BuildFeatureTogglesAsync(
              req.IsFullPackage,
              req.FeatureIds,
              req.FeatureMonthlyLimits,   
              cancellationToken);

            ValidatePlan(entity);
            ValidatePrice(entity.Price);
            ValidateDiscounts(req.Price);


            var created = await _subscriptionPlanRepository.CreatePlanAsync(entity, cancellationToken);
            var withNav = await _subscriptionPlanRepository.GetByIdWithNavAsync(created.Id, cancellationToken);

            var res = _mapper.Map<SubscriptionPlanDetailResponse>(withNav);
            return res;

        }
        public async Task<bool> DeletePlanAsync(Guid planId, CancellationToken cancellationToken = default)
        {
            return await _subscriptionPlanRepository.DeleteAsync(planId, cancellationToken);
        }
        public async Task<PagedResult<SubscriptionPlanListItemResponse>> GetAllAsync(SubscriptionPlanPagedRequest request, CancellationToken cancellationToken = default)
        {
            var entities = await _subscriptionPlanRepository.GetAllAsync(request, cancellationToken);

            return new PagedResult<SubscriptionPlanListItemResponse>
            {
                Items = _mapper.Map<List<SubscriptionPlanListItemResponse>>(entities.Items),
                TotalCount = entities.TotalCount,
                PageNumber = entities.PageNumber,
                PageSize = entities.PageSize
            };
        }
        public async Task<List<SubscriptionPlanCustomerResponse>> GetAllForCusromerAsync(CancellationToken cancellationToken = default)
        {
            var entities = await _subscriptionPlanRepository.GetAllForCusromerAsync(cancellationToken);
            return _mapper.Map<List<SubscriptionPlanCustomerResponse>>(entities);
        }
        public async Task<SubscriptionPlanDetailResponse?> GetPlanByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var plan = await _subscriptionPlanRepository.GetByIdWithNavAsync(id, cancellationToken);
            if (plan == null) return null;

            var res = _mapper.Map<SubscriptionPlanDetailResponse>(plan);

            return res;
        }
        public async Task<SubscriptionPlanDetailResponse> UpdatePlanAsync(SubscriptionPlanUpdateRequest req, CancellationToken cancellationToken = default)
        {
            var entity = _mapper.Map<SubscriptionPlan>(req);
            entity.Price = _mapper.Map<SubscriptionPlanPrice>(req.Price);

            if (req.IsFullPackage || req.FeatureIds != null)
            {
                entity.Features = await BuildFeatureTogglesAsync(
                     req.IsFullPackage,
                     req.FeatureIds,
                     req.FeatureMonthlyLimits,
                     cancellationToken);
            }

            ValidatePlan(entity);
            ValidatePrice(entity.Price);

            var updated = await _subscriptionPlanRepository.UpdatePlanAsync(entity, cancellationToken);
            await CascadePlanStatusToSubscriptionsAsync(updated, cancellationToken);

            var withNav = await _subscriptionPlanRepository.GetByIdWithNavAsync(updated.Id, cancellationToken);

            var res = _mapper.Map<SubscriptionPlanDetailResponse>(withNav);
            return res;

        }
    }
}
