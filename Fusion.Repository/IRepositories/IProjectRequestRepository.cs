using Fusion.Repository.Bases.Page;
using Fusion.Repository.Bases.Page.ProjectRequest;
using Fusion.Repository.Data;
using Fusion.Repository.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fusion.Repository.IRepositories
{
    public interface IProjectRequestRepository : IGenericRepository<ProjectRequest>
    {
        Task<ProjectRequest> AddProjectRequestAsync(ProjectRequest request, string vendorEmail, string code, CancellationToken cancellationToken = default);

        Task<ProjectRequest> UpdateProjectRequestAsync(Guid id, ProjectRequest request, string vendorEmail, CancellationToken cancellationToken = default);

        Task<PagedResult<ProjectRequest>> SearchProjectRequestAsync(ProjectRequestSearchRequest filter, Guid userCompanyId, CancellationToken cancellationToken = default);

        Task<ProjectRequest?> GetProjectRequestByIdAsync(Guid id, CancellationToken cancellationToken = default);

        Task<bool> DeleteProjectRequestAsync(Guid id, CancellationToken cancellationToken = default);

    }
}
