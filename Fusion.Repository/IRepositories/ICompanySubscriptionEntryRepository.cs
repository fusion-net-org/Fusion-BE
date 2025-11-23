

using Fusion.Repository.Entities;

namespace Fusion.Repository.IRepositories;

public interface ICompanySubscriptionEntryRepository
{
    Task<CompanySubscriptionEntry> CreateAsync(
           Guid companySubscriptionId,
           Guid actorUserId,
           Guid companyId,
           CancellationToken ct = default);

    Task<List<CompanySubscriptionEntry>> GetByCompanySubscriptionIdAsync(
        Guid companySubscriptionId,
        CancellationToken ct = default);
}
