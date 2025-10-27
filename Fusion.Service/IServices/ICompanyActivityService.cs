

using Fusion.Repository.Bases.Page;
using Fusion.Repository.Bases.Page.CompanyActivityLog;
using Fusion.Repository.Entities;

namespace Fusion.Service.IServices;

public interface ICompanyActivityService
{
    Task<CompanyActivityLog> CreateLog(CompanyActivityLog log, CancellationToken cancellationToken = default);
    Task<bool> DeleteLogAsync(Guid id);
    Task<PagedResult<CompanyActivityLog>> GetPagedAsync(
       Guid companyId, CompanyActivityLogPagedSearchRequest? request, CancellationToken ct = default);

    Task<CompanyActivityLog?> GetByIdAsync(Guid id, CancellationToken ct = default);

    Task<bool> UpdateIsView(bool isView, Guid companyId, CancellationToken ct = default);
}
