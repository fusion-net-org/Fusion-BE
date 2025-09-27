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
        Task<PagedResult<Company>> GetPagedCompaniesAsync(CompanyPagedSearchRequest request, CancellationToken cancellationToken = default);
        Task<string> GetMailCompanyByGuid(Guid company);
        Task<string> GetCompanyNameByGuid(Guid company);
        Task<Guid?> GetCompanyIdByUserId(Guid userId);
    }
}
