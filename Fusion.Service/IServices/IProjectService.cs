
using Fusion.Repository.Bases.Page;
using Fusion.Repository.Bases.Page.Project;
using Fusion.Service.ViewModels.Project.Requests;
using Fusion.Service.ViewModels.Project.Responses;

namespace Fusion.Service.IServices
{
    public interface IProjectService 
    {
        Task<ProjectsResponse> CreateProjectAsync(CreateProjectRequest request, CancellationToken cancellationToken = default);
        Task<PagedResult<ProjectListResponse>> GetAllProjectAsync(ProjectSearchRequest req, CancellationToken ct = default);
        Task<ProjectDetailResponse> GetProjectDetailAsync(Guid id, CancellationToken ct = default); 
        Task<PagedResult<ProjectListResponse>> GetProjectByMemberIdAsync(Guid userId, ProjectSearchRequest req, CancellationToken ct = default);
        Task<PagedResult<ProjectListResponse>> GetProjectByActorIdAsync(Guid userId, ProjectSearchRequest req, CancellationToken ct = default);
        Task<(int Todo, int Cancel, int Finish)> GetCountProjectByStatusAsync(CancellationToken ct = default);

    }
}
