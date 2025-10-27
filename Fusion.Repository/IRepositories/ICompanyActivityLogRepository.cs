

using Fusion.Repository.Bases.Page;
using Fusion.Repository.Bases.Page.CompanyActivityLog;
using Fusion.Repository.Entities;

namespace Fusion.Repository.IRepositories;

public interface ICompanyActivityLogRepository
{

    Task<PagedResult<CompanyActivityLog>> GetPagedLogsByCompanyIdAsync(
        Guid companyId,
        Guid userId, 
        CompanyActivityLogPagedSearchRequest request,
        CancellationToken token = default);

    Task<CompanyActivityLog?> GetLogByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<bool> UpdateIsViewLog(bool isView, Guid companyId, Guid userId, CancellationToken cancellationToken = default);
}
