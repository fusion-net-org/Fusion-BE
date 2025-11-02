using Fusion.Repository.Bases.Page;
using Fusion.Repository.Bases.Page.Company;
using Fusion.Service.ViewModels.Companies.Requests;
using Fusion.Service.ViewModels.Companies.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fusion.Repository.Entities;

namespace Fusion.Service.IServices
{
    public interface ICompanyService
    {
        Task<string> GetMailCompanyByGuid(Guid company);
        Task<string> GetCompanyNameByGuid(Guid company);
        Task<Guid?> GetCompanyIdByUserId(Guid userId);
        Task<PagedResult<CompanyResponse>> GetPagedCompaniesAsync(string userMail, CompanyPagedSearchRequest request, CancellationToken cancellationToken = default);
        Task<PagedResult<CompanyResponseVersion2>> GetAllCompaniesAsync(string userMail, CompanyPagedSearchRequestVersion2 request, Guid? selectedCompanyId, CancellationToken cancellationToken = default);
        Task<CompanyResponse> CreateCompanyAsync(CompanyRequest request, string Email, CancellationToken cancellationToken = default);
        Task<CompanyResponse> GetCompanyByIdAsync(Guid companyId, CancellationToken cancellationToken = default);
        Task<CompanyResponse> UpdateCompanyAsync(Guid companyId, CompanyRequest request, string Email, CancellationToken cancellationToken = default);
        Task<bool> DeleteCompanyAsync(Guid companyId, string Email, CancellationToken cancellationToken = default);
        Task<CompanySummaryResponse> GetCompanySummaryAsync(Guid companyId);
        Task<CompanyPerformanceResponse> GetCompanyPerformanceAsync(Guid companyId);
        Task<bool> DeleteCompanyByAdminAsync(Guid companyId, CancellationToken cancellationToken = default);
        Task<CompanyResponse> UpdateCompanyByAdminAsync(Guid companyId, CompanyRequest request, CancellationToken cancellationToken = default);
        Task<CompanyStatusCountsVm> GetCompanyStatusCountsAsync(CancellationToken cancellationToken = default);
        Task<CompanyMonthlyStatsVm> GetCompaniesCreatedByMonthAsync(int year, CancellationToken ct = default);
    }
}
