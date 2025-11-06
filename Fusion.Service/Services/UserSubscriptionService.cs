
//using AutoMapper;
//using Fusion.Repository.Bases.Page;
//using Fusion.Repository.Data;
//using Fusion.Repository.Entities;
//using Fusion.Repository.IRepositories;
//using Fusion.Service.Commons.Helpers;
//using Fusion.Service.IServices;
//using Fusion.Service.ViewModels.UserSubscription.Requests;
//using Fusion.Service.ViewModels.UserSubscription.Responses;
//using System.Threading;


//namespace Fusion.Service.Services
//{
//    public class UserSubscriptionService : IUserSubscriptionService
//    {
//        private readonly IUserSubscriptionRepository _repository;
//        private readonly IUnitOfWork _unitOfWork;
//        private readonly IMapper _mapper;
//        private readonly ICurrentService _currentService;
//        private readonly IUserLogService _userLogService;

//        public UserSubscriptionService(IUserSubscriptionRepository repository, IUnitOfWork unitOfWork, IMapper mapper, ICurrentService currentService, IUserLogService userLogService)
//        {
//            _repository = repository;
//            _unitOfWork = unitOfWork;
//            _mapper = mapper;
//            _currentService = currentService;
//            _userLogService = userLogService;
//        }

//        public async Task<UserSubscription> CreateUserSubscriptionAsync(Guid userId, CreateUserSubscriptionRequest request, CancellationToken cancellationToken = default)
//        {
//            var entity = new UserSubscription
//            {
//                Id = Guid.NewGuid(),
//                UserId = userId,
//                PackageId = request.PackageId,
//                PurchaseDate = request.PurchaseDate,
//                QuotaCompanyAdded = request.QuotaCompanyAdded,
//                QuotaProjectAdded = request.QuotaProjectAdded,
//                QuotaCompanyRemaining = request.QuotaCompanyAdded,
//                QuotaProjectRemaining = request.QuotaProjectAdded,
//                ExpiryDate = DateTime.UtcNow.AddMonths(4), 
//                IsActive = true
//            };

//            await _repository.AddAsync(entity, cancellationToken);
//            await _unitOfWork.SaveChangesAsync(cancellationToken);

//            var user = await _unitOfWork.Repository<User>().FindAsync(x => x.Id == userId);
//            var userLog = new UserLog
//            {
//                ActorUserId = user.Id,
//                Title = "Create User Subscription",
//                Description = $"The system has created a new Subsciption {entity.SubscriptionPackage.Name} for the user {user.UserName}."
//            };
//            await _userLogService.CreateLog(userLog);
//            return entity;
//        }

//        public async Task<PagedResult<UserSubscription>> GetAllSubscription()
//        {
//            var result = await _repository.GetAllSubscription();

//            return result ?? new PagedResult<UserSubscription>
//            {
//                Items = new List<UserSubscription>(),
//                TotalCount = 0,
//                PageNumber = 1,
//                PageSize = 0
//            };
//        }
//        public async Task DecreaseCompanyQuotaAsync(Guid userId, CancellationToken cancellationToken = default)
//        {
//            await _repository.DecreaseCompanyQuotaAsync(userId, cancellationToken);
//            var user = await _unitOfWork.Repository<User>().FindAsync(x => x.Id == userId);
//            var userLog = new UserLog
//            {
//                ActorUserId = user.Id,
//                Title = "Decrease Company Quota",
//                Description = $"User {user.UserName} has created new a company."
//            };
//            await _userLogService.CreateLog(userLog);
//        }

//        public async Task DecreaseProjectQuotaAsync(Guid userId, CancellationToken cancellationToken = default)
//        {
//            await _repository.DecreaseProjectQuotaAsync(userId, cancellationToken);
//            var user = await _unitOfWork.Repository<User>().FindAsync(x => x.Id == userId);
//            var userLog = new UserLog
//            {
//                ActorUserId = user.Id,
//                Title = "Decrease Project Quota",
//                Description = $"User {user.UserName} has created new a project."
//            };
//            await _userLogService.CreateLog(userLog);
//        }

//        public async Task<PagedResult<UserSubscriptionResponse>> GetAllUserSubscrptionByUserIdAsync(PagedRequest request, CancellationToken cancellationToken = default)
//        {
//            var userId = _currentService.GetUserId();

//            // Lấy dữ liệu đã phân trang từ repository
//            var result = await _repository.GetPagedSubscriptionsByUserIdAsync(userId, request, cancellationToken);

//            // Map sang kiểu response
//            var list = new PagedResult<UserSubscriptionResponse>
//            {
//                Items = _mapper.Map<List<UserSubscriptionResponse>>(result.Items),
//                TotalCount = result.TotalCount,
//                PageNumber = result.PageNumber,
//                PageSize = result.PageSize
//            };

//            return list;
//        }

//        public async Task<int> DeactiveExpiredOrDepleteAsync(CancellationToken ct = default)
//        {
//           var now = DateTime.UtcNow;
//           var affected = await _repository.DeactivateExpiredOrDepletedAsync(now, ct);
//           return affected;
//        }
//    }
//}
