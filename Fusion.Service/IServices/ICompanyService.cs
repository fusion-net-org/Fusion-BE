using Fusion.Repository.Bases.Page;
using Fusion.Repository.Bases.Page.Company;
using Fusion.Service.ViewModels.Companies.Requests;
using Fusion.Service.ViewModels.Companies.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fusion.Service.IServices
{
    public interface ICompanyService
    {
        Task<PagedResult<CompanyResponse>> GetPagedCompaniesAsync(CompanyPagedSearchRequest request, CancellationToken cancellationToken = default);

        Task<CompanyResponse> CreateCompanyAsync(CreateCompanyRequest request, string Email, CancellationToken cancellationToken = default);
    }
}
