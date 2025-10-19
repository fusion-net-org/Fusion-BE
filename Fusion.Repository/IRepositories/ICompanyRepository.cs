using Fusion.Repository.Bases.Page;
using Fusion.Repository.Bases.Page.Company;
using Fusion.Repository.Data;
using Fusion.Repository.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fusion.Repository.Entities;

namespace Fusion.Repository.IRepositories
{
    public interface ICompanyRepository : IGenericRepository<Company>
    {
        Task<PagedResult<Company>> GetPagedCompaniesAsync(string userMail, CompanyPagedSearchRequest request, CancellationToken cancellationToken = default);
        Task<PagedResult<Company>> GetAllCompaniesAsync(string userMail, CompanyPagedSearchRequestVersion2 request, Guid? selectedCompanyId, CancellationToken cancellationToken = default);

        Task<Company?> GetCompanyByTaxCode(string taxcode);
        Task<Company?> GetCompanyByEmail(string email);
        Task<Company?> GetCompanyByIdAsync(Guid Id);
        Task<Company?> AddCompanyAsync(User user, string image_company, string avatar_company, Company new_company, CancellationToken cancellationToken = default);
        Task<Company?> UpdateCompanyAsync(string image_company, string avatar_company, Guid companyId, Company update_company, CancellationToken cancellationToken = default);
        Task<bool?> DeleteCompanyAsync(Company company, CancellationToken cancellationToken = default);
        Task<string> GetMailCompanyByGuid(Guid company);
        Task<string> GetCompanyNameByGuid(Guid company);
        Task<Guid?> GetCompanyIdByUserId(Guid userId);
    }
}
