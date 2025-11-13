using Fusion.Repository.Bases.Page;
using Fusion.Repository.Bases.Page.Project;
using Fusion.Repository.Data;
using Fusion.Repository.Entities;
using Fusion.Repository.ViewModels;

namespace Fusion.Repository.IRepositories
{
    public interface IProjectRepository : IGenericRepository<Project>
    {
        Task<PagedResult<Project>> GetAllProjectAsync(ProjectSearchRequest req, CancellationToken ct = default);
        Task<Project?> GetProjectDetailAsync(Guid id, CancellationToken ct = default);
        Task<PagedResult<Project>> GetProjectByMemberIdAsync(Guid userId, ProjectSearchRequest req, CancellationToken ct = default);
        Task<PagedResult<Project>> GetProjectByActorIdAsync(Guid userId, ProjectSearchRequest req, CancellationToken ct = default);
        Task<List<StatusCountResponse>> GetCountProjectByStatusAsync(CancellationToken ct = default);
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
        Task<Project> GetProjectById(Guid projectId, CancellationToken cancellationToken = default);
        Task<PagedResult<Project>> GetProjectsForAdminAsync(ProjectSummarySearchRequest request, CancellationToken cancellationToken = default);

        Task<Project?> GetProjectsByIdForAdminAsync(Guid projectId, CancellationToken cancellationToken = default);

        Task<int> GetTotalProjectsAsync(CancellationToken cancellationToken = default);

    }
}
