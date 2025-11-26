

using Fusion.Repository.Entities;
using Fusion.Repository.ViewModels.CompanySubscriptionEntry;

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
    Task<List<CompanySubscriptionUserUsageItem>> GetUserUsageByCompanySubscriptionAsync(
            Guid companySubscriptionId,
            CancellationToken ct = default);
}
