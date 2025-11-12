
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

namespace Fusion.Service.Services
{
    public class UserSubscriptionService : IUserSubscriptionService
    {
        private readonly IUserSubscriptionRepository _repository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ICurrentService _currentService;
        private readonly IUserLogService _userLogService;

        public UserSubscriptionService(IUserSubscriptionRepository repository, IUnitOfWork unitOfWork, IMapper mapper, ICurrentService currentService, IUserLogService userLogService)
        {
            _repository = repository;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _currentService = currentService;
            _userLogService = userLogService;
        }

        public async Task ConsumeFeatureAsync(UseFeatureRequest request, CancellationToken cancellationToken = default)
        {
            await _repository.ConsumeFeatureAsync(request.UserSubscriptionId, request.FeatureKey, 1, cancellationToken);
        }

        public async Task<UserSubscriptionDetailResponse> CreateAsync(UserSubscriptionCreateRequest request, CancellationToken ct = default)
        {
            var entity = _mapper.Map<UserSubscription>(request);
            var created = await _repository.CreateAsync(entity, ct);

            return _mapper.Map<UserSubscriptionDetailResponse>(created);
        }

        public async Task<PagedResult<UserSubscriptionListItem>> GetAllAsync(UserSubscriptionPagedRequest request, CancellationToken ct = default)
        {

            var result = await _repository.GetAllAsync(request, ct);
            return new PagedResult<UserSubscriptionListItem>
            {
                Items = result.Items.Select(_mapper.Map<UserSubscriptionListItem>).ToList(),
                TotalCount = result.TotalCount,
                PageNumber = result.PageNumber,
                PageSize = result.PageSize
            };
        }

        public async Task<PagedResult<UserSubscriptionListItem>> GetAllByUserIdAsync(UserSubscriptionPagedRequest request, CancellationToken cancellationToken = default)
        {
            var userId = _currentService.GetUserId();

            var result = await _repository.GetAllByUserIdAsync(userId, request, cancellationToken);
            return new PagedResult<UserSubscriptionListItem>
            {
                Items = result.Items.Select(_mapper.Map<UserSubscriptionListItem>).ToList(),
                TotalCount = result.TotalCount,
                PageNumber = result.PageNumber,
                PageSize = result.PageSize
            };
        }

        public async Task<UserSubscriptionDetailResponse?> GetByIdAsync(Guid id, CancellationToken ct = default)
        {
            var entity = await _repository.GetByIdWithNavAsync(id, ct);
            return entity == null ? null : _mapper.Map<UserSubscriptionDetailResponse>(entity);
        }
        public async Task<UserSubscriptionDetailResponse> UpdateStatusAsync(Guid id, SubscriptionStatus status, CancellationToken ct = default)
        {
            var userId = _currentService.GetUserId();
            var user = await _unitOfWork.Repository<User>().FindAsync(x => x.Id == userId);
            if (user == null)
                throw CustomExceptionFactory.CreateNotFoundError(
                  ResponseMessages.LOGIN_REQUIRED);

            var updated = await _repository.UpdateStatusAsync(id, userId, status, ct);

            var userLog = new UserLog
            {
                ActorUserId = userId,
                Title = "Update Status",
                Description = $"User {user.UserName} has updated user subscription {updated.NamePlan}."
            };
            await _userLogService.CreateLog(userLog);
            return _mapper.Map<UserSubscriptionDetailResponse>(updated);
        }
    }
}