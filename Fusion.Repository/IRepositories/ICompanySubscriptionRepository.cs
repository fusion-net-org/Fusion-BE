
using Fusion.Repository.Entities;

namespace Fusion.Repository.IRepositories;

public interface ICompanySubscriptionRepository
{
    Task<CompanySubscription?> GetByIdWithNavAsync(Guid id, CancellationToken ct = default);
    Task<CompanySubscription> CreateAsync(CompanySubscription companySubscription, CancellationToken cancellationToken = default);
}
