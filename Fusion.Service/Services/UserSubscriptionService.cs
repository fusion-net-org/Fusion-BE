
using AutoMapper;
using Fusion.Repository.Bases.Exceptions;
using Fusion.Repository.Bases.Page;
using Fusion.Repository.Bases.Responses;
using Fusion.Repository.Data;
using Fusion.Repository.Entities;
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
        private readonly IUnitOfWork unitOfWork;
        private readonly IMapper _mapper;
        private readonly ICurrentService _currentService;

        public UserSubscriptionService(IUserSubscriptionRepository repository, IUnitOfWork unitOfWork, IMapper mapper, ICurrentService currentService)
        {
            _repository = repository;
            this.unitOfWork = unitOfWork;
            _mapper = mapper;
            _currentService = currentService;
        }

        public async Task<UserSubscription> CreateUserSubscriptionAsync(Guid userId, CreateUserSubscriptionRequest request, CancellationToken cancellationToken = default)
        {
            var entity = new UserSubscription
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                PackageId = request.PackageId,
                PurchaseDate = request.PurchaseDate,
                QuotaCompanyAdded = request.QuotaCompanyAdded,
                QuotaProjectAdded = request.QuotaProjectAdded,
                QuotaCompanyRemaining = request.QuotaCompanyAdded,
                QuotaProjectRemaining = request.QuotaProjectAdded,
                ExpiryDate = DateTime.UtcNow.AddMonths(1), 
                IsActive = true
            };

            await _repository.AddAsync(entity, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            return entity;
        }

        public async Task DecreaseCompanyQuotaAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            await _repository.DecreaseCompanyQuotaAsync(userId, cancellationToken);
        }

        public async Task DecreaseProjectQuotaAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            await _repository.DecreaseProjectQuotaAsync(userId, cancellationToken);
        }

        public async Task<PagedResult<UserSubscriptionResponse>> GetAllUserSubscrptionByUserIdAsync(PagedRequest request, CancellationToken cancellationToken = default)
        {
            var userId = _currentService.GetUserId();

            // Lấy dữ liệu đã phân trang từ repository
            var result = await _repository.GetPagedSubscriptionsByUserIdAsync(userId, request, cancellationToken);

            // Map sang kiểu response
            var list = new PagedResult<UserSubscriptionResponse>
            {
                Items = _mapper.Map<List<UserSubscriptionResponse>>(result.Items),
                TotalCount = result.TotalCount,
                PageNumber = result.PageNumber,
                PageSize = result.PageSize
            };

            return list;
        }
    }
}
