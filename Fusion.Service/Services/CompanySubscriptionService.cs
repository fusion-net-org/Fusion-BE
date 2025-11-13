

//using AutoMapper;
//using Fusion.Repository.Bases.Exceptions;
//using Fusion.Repository.Bases.Page;
//using Fusion.Repository.Bases.Page.CompanySubscriptions;
//using Fusion.Repository.Bases.Responses;
//using Fusion.Repository.Data;
//using Fusion.Repository.Entities;
//using Fusion.Repository.Enums;
//using Fusion.Repository.IRepositories;
//using Fusion.Service.Commons.Helpers;
//using Fusion.Service.IServices;
//using Fusion.Service.ViewModels.CompanySubscription.Requests;
//using Fusion.Service.ViewModels.CompanySubscription.Responses;
//using Fusion.Service.ViewModels.UserSubscription.Requests;
//using System.Threading;

//namespace Fusion.Service.Services
//{
//    public class CompanySubscriptionService : ICompanySubscriptionService
//    {
//        private readonly ICompanySubscriptionRepository _companySubscriptionRepository;
//        private readonly IMapper _mapper;
//        private readonly ICurrentService _currentService;
//        private readonly IUnitOfWork _unitOfWork;
//        private readonly IUserSubscriptionService _userSubscriptionService;
//        private readonly IUserLogService _userLogService;

//        public CompanySubscriptionService(ICompanySubscriptionRepository companySubscriptionRepository, IMapper mapper, ICurrentService currentService,
//            IUnitOfWork unitOfWork, IUserSubscriptionService userSubscriptionService, IUserLogService userLogService)
//        {
//            _companySubscriptionRepository = companySubscriptionRepository;
//            _mapper = mapper;
//            _currentService = currentService;
//            _unitOfWork = unitOfWork;
//            _userSubscriptionService = userSubscriptionService;
//            _userLogService = userLogService;
//        }

//        public async Task<CompanySubscriptionDetailResponse> CreateAsync(CompanySubscriptionCreateRequest dto, CancellationToken cancellationToken = default)
//        {
//            var userCurrentId = _currentService.GetUserId();

//            var userSubscription = await _unitOfWork.Repository<UserSubscription>().FindAsync(x => x.Id == dto.UserSubscriptionId);
//            if (userSubscription == null)
//                throw CustomExceptionFactory.CreateNotFoundError(ResponseMessages.NOT_FOUND.FormatMessage("User subscription."));

//            var transaction = await _unitOfWork.Repository<TransactionPayment>().FindAsync(x => x.Id == userSubscription.TransactionId);
//            if (transaction == null)
//                throw CustomExceptionFactory.CreateNotFoundError(ResponseMessages.NOT_FOUND.FormatMessage("Transaction."));

//            if (userCurrentId != transaction.UserId)
//                throw CustomExceptionFactory.CreateForbiddenError();
//            try
//            {
//                var check = new UseFeatureRequest
//                {
//                    UserSubscriptionId = userSubscription.Id,
//                    FeatureKey = FeatureKeys.Share
//                };
//                await _userSubscriptionService.ConsumeFeatureAsync(check, cancellationToken);

//                // map DTO -> Entity
//                var entity = _mapper.Map<CompanySubscription>(dto);

//                // set thông tin chung
//                entity.CompanyId = dto.CompanyId;
//                entity.UserSubscriptionId = dto.UserSubscriptionId;

//                // gọi repository để xử lý quota, validate, transaction
//                var createdEntity = await _companySubscriptionRepository.CreateAsync(entity, cancellationToken);

//                var company = await _unitOfWork.Repository<Company>().FindAsync(x => x.Id == entity.CompanyId);
//                var userLog = new UserLog
//                {
//                    ActorUserId = userCurrentId,
//                    Title = "Create company subscription",
//                    Description = $"User create new a company subscription for company '{company.Name}' with id {entity.CompanyId} from subscription '{userSubscription.NamePlan}' with id {userSubscription.Id}."
//                };
//                await _userLogService.CreateLog(userLog, cancellationToken);
//                // map lại sang DTO trả ra
//                return _mapper.Map<CompanySubscriptionDetailResponse>(createdEntity);
//            }
//            catch
//            {
//                throw;
//            }
//        }

//        public async Task<CompanySubscriptionDetailResponse> UpdateAsync(
//             CompanySubscriptionUpdateRequest dto,
//             CancellationToken cancellationToken = default)
//        {
//            var userCurrentId = _currentService.GetUserId();

//            var existing = await _companySubscriptionRepository.GetByIdWithNavAsync(dto.Id);
//            if (existing == null)
//                throw CustomExceptionFactory.CreateNotFoundError(
//                    ResponseMessages.NOT_FOUND.FormatMessage("Company Subscription"));

//            // Map allowed fields only
//            var updateEntity = new CompanySubscription
//            {
//                Id = dto.Id,
//                Status = dto.Status,
//                CompanySubscriptionEntitlements = _mapper.Map<List<CompanySubscriptionEntitlement>>(dto.Entitlements)
//            };

//            var updated = await _companySubscriptionRepository.UpdateAsync(userCurrentId, updateEntity, cancellationToken);
//            return _mapper.Map<CompanySubscriptionDetailResponse>(updated);
//        }

//        public async Task<CompanySubscriptionDetailResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
//        {
//            var entity = await _companySubscriptionRepository.GetByIdWithNavAsync(id, cancellationToken);
//            if (entity == null)
//                throw CustomExceptionFactory.CreateNotFoundError(ResponseMessages.NOT_FOUND.FormatMessage("Company Subscription"));
//            return _mapper.Map<CompanySubscriptionDetailResponse>(entity);
//        }

//        public async Task<PagedResult<CompanySubscriptionListResponse>> GetAllAsync(
//        CompanySubscriptionPagedRequest request,
//        CancellationToken cancellationToken = default)
//        {
//            var entities = await _companySubscriptionRepository.GetAllAsync(request, cancellationToken);
//            return new PagedResult<CompanySubscriptionListResponse>
//            {
//                Items = _mapper.Map<List<CompanySubscriptionListResponse>>(entities.Items),
//                TotalCount = entities.TotalCount,
//                PageNumber = entities.PageNumber,
//                PageSize = entities.PageSize
//            };
//        }

//        public async Task<PagedResult<CompanySubscriptionListResponse>> GetAllByCompanyAsync(
//        Guid companyId,
//        CompanySubscriptionPagedRequest request,
//        CancellationToken cancellationToken = default)
//        {
//            var entities = await _companySubscriptionRepository.GetAllByCompanyIdAsync(companyId, request, cancellationToken);
//            return new PagedResult<CompanySubscriptionListResponse>
//            {
//                Items = _mapper.Map<List<CompanySubscriptionListResponse>>(entities.Items),
//                TotalCount = entities.TotalCount,
//                PageNumber = entities.PageNumber,
//                PageSize = entities.PageSize
//            };
//        }

//        public async Task<List<CompanySubscriptionActiveResponse>> GetAllActiveByCompanyIdAsync(Guid companyId, CancellationToken ct = default)
//        {
//            var activeSubs = await _companySubscriptionRepository.GetAllActiveByCompanyIdAsync(companyId, ct);

//            return _mapper.Map<List<CompanySubscriptionActiveResponse>>(activeSubs);
//        }
//        public async Task ConsumeCompanyFeatureAsync(Guid companySubscriptionId, FeatureKeys featureKey, int quantity = 1, CancellationToken ct = default)
//        {
//            await _companySubscriptionRepository.UseFeatureAsync(companySubscriptionId, featureKey, quantity, ct);
//        }
//    }
//}