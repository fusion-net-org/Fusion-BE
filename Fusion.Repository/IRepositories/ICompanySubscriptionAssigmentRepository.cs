

using Fusion.Repository.Entities;

namespace Fusion.Repository.IRepositories;

public interface ICompanySubscriptionAssigmentRepository
{
    Task<CompanySubscriptionAssignment> CreateCompanySubscriptionAsync(CompanySubscriptionAssignment req);
    Task<CompanySubscriptionAssignment> UpdateCompanySubscriptionAsync(CompanySubscriptionAssignment req);
    Task<bool> DeleteCompanySubscriptionAsync(CompanySubscriptionAssignment req);

}
