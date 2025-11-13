
using AutoMapper;
using Fusion.Repository.Bases.Exceptions;
using Fusion.Repository.Bases.Page;
using Fusion.Repository.Bases.Page.SubscriptionPlans;
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
        public SubscriptionPlanService(ISubscriptionPlanRepository subscriptionPlanRepository, IMapper mapper, IFeatureCatalogService featureCatalogService)
        {
            _subscriptionPlanRepository = subscriptionPlanRepository;
            _mapper = mapper;
            _featureCatalogService = featureCatalogService;
        }

        private static void ValidatePlan(SubscriptionPlan p)
        {
            if (p.LicenseScope == LicenseScope.CompanyWide && p.SeatsPerCompanyLimit != null)
                throw CustomExceptionFactory.CreateBadRequestError("SeatsPerCompanyLimit must be null for CompanyWide plans.");

            if (p.CompanyShareLimit.HasValue && p.CompanyShareLimit <= 0)
                throw CustomExceptionFactory.CreateBadRequestError("CompanyShareLimit must be > 0 or null.");

            if (p.SeatsPerCompanyLimit.HasValue && p.SeatsPerCompanyLimit <= 0)
                throw CustomExceptionFactory.CreateBadRequestError("SeatsPerCompanyLimit must be > 0 or null.");
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

        private async Task<List<SubscriptionPlanFeature>> BuildFeatureTogglesAsync(
       bool isFullPackage, List<Guid>? featureIds, CancellationToken ct)
        {
            if (isFullPackage)
            {
                var actives = await _featureCatalogService.GetAllActiveAsync(ct);
                // Nếu không có feature active vẫn trả list rỗng (để repo replace đúng)
                return actives.Select(f => new SubscriptionPlanFeature
                {
                    FeatureId = f.Id,
                    Enabled = true
                }).ToList();
            }

            if (featureIds == null || featureIds.Count == 0)
                return new List<SubscriptionPlanFeature>();

            var distinct = featureIds.Where(id => id != Guid.Empty).Distinct().ToList();
            return distinct.Select(fid => new SubscriptionPlanFeature
            {
                FeatureId = fid,
                Enabled = true
            }).ToList();
        }

        public async Task<SubscriptionPlanDetailResponse> CreatePlanAsync(SubscriptionPlanCreateRequest req, CancellationToken cancellationToken = default)
        {
            var entity = _mapper.Map<SubscriptionPlan>(req);
            entity.Price = _mapper.Map<SubscriptionPlanPrice>(req.Price);

            //   Quyết định features: full-package => lấy tất cả active;
            //    ngược lại => dùng FeatureIds (distinct, bỏ Guid.Empty, đồng thời có thể validate active).
            entity.Features = await BuildFeatureTogglesAsync(
                req.IsFullPackage, req.FeatureIds, cancellationToken);

            ValidatePlan(entity);
            ValidatePrice(entity.Price);

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
                    cancellationToken);
            }

            ValidatePlan(entity);
            ValidatePrice(entity.Price);

            var updated = await _subscriptionPlanRepository.UpdatePlanAsync(entity, cancellationToken);
            var withNav = await _subscriptionPlanRepository.GetByIdWithNavAsync(updated.Id, cancellationToken);

            var res = _mapper.Map<SubscriptionPlanDetailResponse>(withNav);
            return res;

        }
    }
}
