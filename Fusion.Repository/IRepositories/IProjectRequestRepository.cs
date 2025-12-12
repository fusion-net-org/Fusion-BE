using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fusion.Repository.Bases.Page;
using Fusion.Repository.Bases.Page.ProjectRequest;
using Fusion.Repository.Data;
using Fusion.Repository.Entities;
using Fusion.Repository.Enums;

namespace Fusion.Repository.IRepositories
{
    public interface IProjectRequestRepository : IGenericRepository<ProjectRequest>
    {
        Task<ProjectRequest> AddProjectRequestAsync(ProjectRequest request, string vendorEmail, string code, CancellationToken cancellationToken = default);

        Task<ProjectRequest> UpdateProjectRequestAsync(Guid id, ProjectRequest request, string vendorEmail, CancellationToken cancellationToken = default);

        Task<PagedResult<ProjectRequest>> SearchProjectRequestAsync(ProjectRequestSearchRequest filter, Guid userCompanyId, CancellationToken cancellationToken = default);

        Task<PagedResult<ProjectRequest>> SearchProjectRequestAsync(ProjectRequestSearchRequest filter, Guid userCompanyId, Guid partnerId, CancellationToken cancellationToken = default);

        Task<PagedResult<ProjectRequest>> SearchProjectRequestAdminAsync(ProjectRequestSearchAdminRequest filter, Guid adminId, CancellationToken cancellationToken = default);


        Task<ProjectRequest?> GetProjectRequestByIdAsync(Guid id, CancellationToken cancellationToken = default);

        Task<bool> DeleteProjectRequestAsync(Guid id,string reason, Guid currentUserId, CancellationToken cancellationToken = default);
        Task<bool> RestoreProjectRequestAsync(Guid id, Guid currentUserId, CancellationToken cancellationToken = default);

        Task<ProjectRequest> AcceptProjectRequestAsync(Guid requestId, string executorEmail, CancellationToken cancellationToken = default);

        Task<ProjectRequest?> RejectProjectRequestAsync(Guid requestId, string executorEmail, string reason, CancellationToken cancellationToken = default);
        Task<ProjectRequest?> GetProjectRequestByContractIdAsync(Guid contractId, CancellationToken cancellationToken = default);
        Task<ProjectRequest> UpdateProjectRequestStatusAsync(Guid requestId, ProjectRequestStatusEnum status, CancellationToken cancellationToken = default);

        Task<bool> CloseFromProjectRequestAsync(Guid requestId, Guid actorUserId, CancellationToken ct = default);
        Task<bool> ReopenFromProjectRequestAsync(Guid requestId, Guid actorUserId, CancellationToken ct = default);
    }
}
