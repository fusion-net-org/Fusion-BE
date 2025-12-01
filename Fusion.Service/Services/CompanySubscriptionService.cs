
using AutoMapper;
using Fusion.Repository.Bases.Exceptions;
using Fusion.Repository.Bases.Page;
using Fusion.Repository.Bases.Page.CompanySubscriptions;
using Fusion.Repository.Data;
using Fusion.Repository.Entities;
using Fusion.Repository.IRepositories;
using Fusion.Repository.ViewModels.CompanySubscriptionEntry;
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


        public CompanySubscriptionService(ICompanySubscriptionRepository companySubscriptionRepository, IMapper mapper,
            IUnitOfWork unitOfWork, IUserLogService userLogService, ICompanySubscriptionEntryRepository companyEntryRepo)
        {
            _companySubscriptionRepository = companySubscriptionRepository;
            _mapper = mapper;
            _unitOfWork = unitOfWork;
            _userLogService = userLogService;
            _companyEntryRepo = companyEntryRepo;
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
        public async Task<bool> UseFeatureInCompanyAsync(UserFeatureRequest request, CancellationToken ct = default)
        {
            await _companySubscriptionRepository.UseFeatureInCompanyAsync(request.CompanySubscriptionId, request.ActorUserId,request.CompanyId, request.FeatureName, ct);
            return true;
        }
        public async Task<bool> UseFeatureInUserAsync(Guid userSubscriptionId, Guid userId, string featureName, CancellationToken ct = default)
        {
            await _companySubscriptionRepository.UseFeatureInUserAsync(userSubscriptionId, userId, featureName, ct);
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
    }
}