
using Fusion.Service.ViewModels.Project.Requests;
using Fusion.Service.ViewModels.Project.Responses;

namespace Fusion.Service.IServices
{
    public interface IProjectService
    {
        Task<ProjectDetailResponse> CreateProjectAsync(
            Guid companyId,
            ProjectCreateRequest request,
            Guid actorUserId,
            CancellationToken ct = default);
        Task<ProjectListResult> GetProjectsForCompanyAsync(
        Guid companyId, ProjectListSearchRequest req, CancellationToken ct = default);
    }
   
}
