
using AutoMapper;
using Fusion.Repository.Bases.Exceptions;
using Fusion.Repository.Bases.Responses;
using Fusion.Repository.Data;
using Fusion.Repository.Entities;
using Fusion.Repository.IRepositories;
using Fusion.Service.IServices;
using Fusion.Service.ViewModels.SubscriptionPackage.Requests;
using Fusion.Service.ViewModels.SubscriptionPackage.Responses;
using Microsoft.EntityFrameworkCore;

namespace Fusion.Service.Services
{
    public class SubscriptionPackageService : ISubscriptionPackageService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IGenericRepository<SubscriptionPlan> _subscriptionRepository;
        private readonly ISubscriptionRepository _repo;

        public SubscriptionPackageService(IUnitOfWork unitOfWork,
            IMapper mapper, ISubscriptionRepository repo)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _subscriptionRepository = _unitOfWork.Repository<SubscriptionPlan>();
            _repo = repo;
        }

        public async Task<SubscriptionPlan> GetSubscriptionByIdAsync(Guid? id, CancellationToken cancellationToken = default)
        {

            var subscription = await _subscriptionRepository.FindAsync(x => x.Id == id, cancellationToken);
            if (subscription == null)
                throw CustomExceptionFactory.CreateNotFoundError(
                    string.Format(ResponseMessages.NOT_FOUND, "Subscription Package"));

            return subscription;
        }
        public async Task<SubscriptionAdminResponse> CreateSubscriptionAsync(SubscriptionRequest request, CancellationToken cancellationToken = default)
        {
            //1. check
            var ExistName = await _repo.ExistsByNameAsync(request.Name, cancellationToken);
            if (ExistName)
                throw CustomExceptionFactory.CreateBadRequestError(
                    string.Format(ResponseMessages.EXISTED, "Subscription Packages Name"));
            //2.Mapper
            var subscription = _mapper.Map<SubscriptionPlan>(request);
            subscription.CreatedAt = DateTime.UtcNow;
            subscription.UpdatedAt = DateTime.UtcNow;

            //3.save
            await _subscriptionRepository.AddAsync(subscription, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            //Map and response
            return _mapper.Map<SubscriptionAdminResponse>(subscription);
        }
        public async Task<bool> DeleteSubscriptionAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var subscription = await GetSubscriptionByIdAsync(id, cancellationToken);

            // check if subscription is being used by any company

            _subscriptionRepository.Remove(subscription);

            var result = await _unitOfWork.SaveChangesAsync(cancellationToken);

            return result > 0;

        }
        public async Task<SubscriptionAdminResponse> UpdateSubscriptionAsync(Guid id, SubscriptionRequest request, CancellationToken cancellationToken = default)
        {
            //1.check
            var subscription = await GetSubscriptionByIdAsync(id);

            //2.Mapper
            var result = _mapper.Map(request, subscription);
            result.UpdatedAt = DateTime.UtcNow;

            //3. Save
            _subscriptionRepository.Update(result);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return _mapper.Map<SubscriptionAdminResponse>(result);
        }
        public async Task<List<SubscriptionResponse?>> GetAllSubscriptionForCustomerAsync(CancellationToken cancellationToken = default)
        {
            var listSubscription = await _subscriptionRepository
               .GetAll()
               .ToListAsync(cancellationToken);

            var response = _mapper.Map<List<SubscriptionResponse>>(listSubscription);
            return response;
        }
        public async Task<List<SubscriptionAdminResponse?>> GetAllSubscriptionForAdminAsync(CancellationToken cancellationToken = default)
        {
            var listSubscription = await _subscriptionRepository
                .GetAll()
                .ToListAsync(cancellationToken);

            var response = _mapper.Map<List<SubscriptionAdminResponse>>(listSubscription);
            return response;
        }
    }
}
