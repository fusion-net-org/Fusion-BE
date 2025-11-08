

using AutoMapper;
using Fusion.Repository.Bases.Exceptions;
using Fusion.Repository.Bases.Responses;
using Fusion.Repository.Data;
using Fusion.Repository.Entities;
using Fusion.Repository.IRepositories;
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
        private readonly ICurrentService _currentService;
        private readonly IUnitOfWork _unitOfWork;

        public CompanySubscriptionService(ICompanySubscriptionRepository companySubscriptionRepository, IMapper mapper, ICurrentService currentService, IUnitOfWork unitOfWork)
        {
            _companySubscriptionRepository = companySubscriptionRepository;
            _mapper = mapper;
            _currentService = currentService;
            _unitOfWork = unitOfWork;
        }

        public async Task<CompanySubscriptionDetailResponse> CreateAsync(CompanySubscriptionCreateRequest dto, CancellationToken cancellationToken = default)
        {
            var userCurrentId = _currentService.GetUserId();

            var userSubscription = await _unitOfWork.Repository<UserSubscription>().FindAsync(x => x.Id == dto.UserSubscriptionId);
            if(userSubscription == null)
                throw CustomExceptionFactory.CreateNotFoundError(ResponseMessages.NOT_FOUND.FormatMessage("User subscription."));

            var transaction = await _unitOfWork.Repository<TransactionPayment>().FindAsync(x => x.Id == userSubscription.TransactionId);
            if (transaction == null)
                throw CustomExceptionFactory.CreateNotFoundError(ResponseMessages.NOT_FOUND.FormatMessage("Transaction."));

            if(userCurrentId != transaction.UserId)
                throw CustomExceptionFactory.CreateForbiddenError();

            // map DTO -> Entity
            var entity = _mapper.Map<CompanySubscription>(dto);

            // set thông tin chung
            entity.CompanyId = dto.CompanyId;
            entity.UserSubscriptionId = dto.UserSubscriptionId;

            // gọi repository để xử lý quota, validate, transaction
            var createdEntity = await _companySubscriptionRepository.CreateAsync(entity, cancellationToken);

            // map lại sang DTO trả ra
            return _mapper.Map<CompanySubscriptionDetailResponse>(createdEntity);
        }
    }
}
