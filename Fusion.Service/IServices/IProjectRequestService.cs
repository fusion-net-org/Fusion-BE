using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fusion.Repository.Bases.Page;
using Fusion.Repository.Bases.Page.ProjectRequest;
using Fusion.Repository.Entities;
using Fusion.Repository.Enums;
using Fusion.Service.ViewModels.Projects.Requests;
using Fusion.Service.ViewModels.Projects.Responses;

namespace Fusion.Service.IServices
{
    public interface IProjectRequestService
    {
        Task<ProjectRequestResponse> AddProjectRequestAsync(CreateProjectRequestRequest request, string vendorEmail, CancellationToken cancellationToken);

        Task<ProjectRequestResponse> UpdateProjectRequestAsync(Guid id, UpdateProjectRequestRequest request, string vendorEmail, CancellationToken cancellationToken = default);

        Task<PagedResult<ProjectRequestResponse>> SearchProjectRequestAsync(ProjectRequestSearchRequest filter, Guid userCompanyId, CancellationToken cancellationToken = default);

        Task<PagedResult<ProjectRequestResponse>> SearchProjectRequestAsync(ProjectRequestSearchRequest filter, Guid userCompanyId, Guid partnerId, CancellationToken cancellationToken = default);

        Task<PagedResult<ProjectRequestResponseV2>> SearchProjectRequestAdminAsync(ProjectRequestSearchAdminRequest filter, Guid adminId, CancellationToken cancellationToken = default);

        Task<ProjectRequestResponse?> GetProjectRequestByIdAsync(Guid id, CancellationToken cancellationToken = default);

        Task<bool> DeleteProjectRequestAsync(Guid id,string reason, CancellationToken cancellationToken = default);
        Task<bool> RestoreProjectRequestAsync(Guid id, CancellationToken cancellationToken = default);
        Task<ProjectRequestResponse> AcceptProjectRequestAsync(Guid requestId, string executorEmail, CancellationToken cancellationToken = default);

        Task<ProjectRequestRejectResponse> RejectProjectRequestAsync(Guid requestId, string executorEmail, string reason, CancellationToken cancellationToken = default);
        Task<ProjectRequestResponse?> GetProjectRequestByContractIdAsync(Guid contractId, CancellationToken cancellationToken = default);

    }
}
