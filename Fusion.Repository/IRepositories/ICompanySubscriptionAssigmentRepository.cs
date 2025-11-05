

using Fusion.Repository.Entities;

namespace Fusion.Repository.IRepositories;

public interface ICompanySubscriptionAssigmentRepository
{
    Task<CompanySubscriptionAssignment> CreateCompanySubscriptionAsync(Guid ownerId, CompanySubscriptionAssignment req);
    Task<CompanySubscriptionAssignment> UpdateCompanySubscriptionAsync(Guid ownerId, CompanySubscriptionAssignment req);
    Task<bool> DeleteCompanySubscriptionAsync(Guid ownerId, CompanySubscriptionAssignment req);
}
