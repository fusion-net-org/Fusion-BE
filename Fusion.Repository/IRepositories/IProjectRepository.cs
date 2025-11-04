using Fusion.Repository.Data;
using Fusion.Repository.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fusion.Repository.IRepositories
{
    public interface IProjectRepository : IGenericRepository<Project>
    {
        Task<bool> IsCodeExistedAsync(Guid companyId, string code, CancellationToken ct = default);
        Task<Project?> GetByIdWithSprintsAsync(Guid projectId, CancellationToken ct = default);
        Task<int> GetAllProjectCountAsync(CancellationToken cancellationToken = default);
        Task<(List<Project> Items, int TotalCount)> GetProjectsForCompanyAsync(
       Guid companyId,
       string? q,
       IEnumerable<string>? statuses,  
       string? sort,
       int pageNumber,
       int pageSize,
       CancellationToken ct = default);
    }
}
