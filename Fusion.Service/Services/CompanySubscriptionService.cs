
using AutoMapper;
using Fusion.Repository.IRepositories;
using Fusion.Service.IServices;
using Fusion.Service.ViewModels.CompanySubscription.Requests;
using Fusion.Service.ViewModels.CompanySubscription.Responses;

namespace Fusion.Service.Services
{
    public class CompanySubscriptionService : ICompanySubscriptionService
    {
        private readonly ICompanySubscriptionRepository _companySubscriptionRepository;
        private readonly IMapper _mapper;

        public CompanySubscriptionService(ICompanySubscriptionRepository companySubscriptionRepository, IMapper mapper)
        {
            _companySubscriptionRepository = companySubscriptionRepository;
            _mapper = mapper;

        }

        public Task<CompanySubscriptionDetailResponse> CreateAsync(CompanySubscriptionCreateRequest request, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public Task<CompanySubscriptionDetailResponse?> GetDetailAsync(Guid id, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }
    }
}