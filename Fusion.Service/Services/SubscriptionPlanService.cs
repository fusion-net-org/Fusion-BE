
using AutoMapper;
using Fusion.Repository.Bases.Exceptions;
using Fusion.Repository.Bases.Page;
using Fusion.Repository.Bases.Page.SubscriptionPlans;
using Fusion.Repository.Bases.Responses;
using Fusion.Repository.Data;
using Fusion.Repository.Entities;
using Fusion.Repository.IRepositories;
using Fusion.Service.Commons.Helpers;
using Fusion.Service.IServices;
using Fusion.Service.ViewModels.SubscriptionPlan.Requests;
using Fusion.Service.ViewModels.SubscriptionPlan.Responses;
using System.Numerics;

namespace Fusion.Service.Services
{
    public class SubscriptionPlanService : ISubscriptionPlanService
    {
        private readonly ISubscriptionPlanRepository _subscriptionPlanRepository;
        private readonly IMapper _mapper;
        private readonly ICurrentService _currentService;
        private readonly IUnitOfWork _unitOfWork;
        public SubscriptionPlanService(ISubscriptionPlanRepository subscriptionPlanRepository, IMapper mapper, ICurrentService currentService, IUnitOfWork unitOfWork)
        {
            _subscriptionPlanRepository = subscriptionPlanRepository;
            _mapper = mapper;
            _currentService = currentService;
            _unitOfWork = unitOfWork;
        }

        public async Task<SubscriptionPlanResponse> CreatePlanAsync(SubscriptionPlanCreateRequest req, CancellationToken cancellationToken = default)
        {

            var entity = _mapper.Map<SubscriptionPlan>(req);
            entity.IsActive = true;
            entity.CreatedAt = DateTime.UtcNow;
            entity.UpdatedAt = DateTime.UtcNow;

            var created = await _subscriptionPlanRepository.CreatePlanAsync(entity, cancellationToken);
            return _mapper.Map<SubscriptionPlanResponse>(created);
        }

        public async Task<bool> DeletePlanAsync(Guid planId, CancellationToken cancellationToken = default)
        {
            var ok = await _subscriptionPlanRepository.DeleteAsync(planId, cancellationToken);
            if (!ok)
                throw CustomExceptionFactory.CreateNotFoundError(
                    ResponseMessages.NOT_FOUND.FormatMessage("Subscription Plan"));

            return true;

        }

        public async Task<SubscriptionPlanDetailResponse?> GetPlanByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var subscriptionPlan = await _subscriptionPlanRepository.GetByIdWithNavAsync(id, cancellationToken);

            return _mapper.Map<SubscriptionPlanDetailResponse>(subscriptionPlan);
        }

        public async Task<PagedResult<SubscriptionPlanResponse>> GetAllAsync(SubscriptionPlanPagedRequest request, CancellationToken cancellationToken = default)
        {
            var pagedEntity = await _subscriptionPlanRepository.GetAllAsync(request, cancellationToken);

            var responses = _mapper.Map<List<SubscriptionPlanResponse>>(pagedEntity.Items);

            return new PagedResult<SubscriptionPlanResponse>
            {
                Items = responses,
                TotalCount = pagedEntity.TotalCount,
                PageNumber = pagedEntity.PageNumber,
                PageSize = pagedEntity.PageSize
            };
        }

        public async Task<SubscriptionPlanResponse> UpdatePlanAsync(SubscriptionPlanUpdateRequest req, CancellationToken cancellationToken = default)
        {
            var existing = await _subscriptionPlanRepository.GetByIdWithNavAsync(req.Id, cancellationToken);
            if (existing == null)
                throw CustomExceptionFactory.CreateNotFoundError(ResponseMessages.NOT_FOUND.FormatMessage("Plan"));

            _mapper.Map(req, existing);
            existing.UpdatedAt = DateTime.UtcNow;

            var updated = await _subscriptionPlanRepository.UpdatePlan(existing, cancellationToken);
            return _mapper.Map<SubscriptionPlanResponse>(updated);
        }

        public async Task<List<SubscriptionPlanResponse>> GetAllForCusromerAsync(CancellationToken cancellationToken = default)
        {
            var subscriptionPlans = await _subscriptionPlanRepository.GetAllForCusromerAsync(cancellationToken);

            var result = _mapper.Map<List<SubscriptionPlanResponse>>(subscriptionPlans);

            return result;
        }
    }
}
