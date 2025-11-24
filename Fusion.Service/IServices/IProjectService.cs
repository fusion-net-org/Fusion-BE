
using Fusion.Repository.Bases.Page;
using Fusion.Repository.Bases.Page.Project;
using Fusion.Repository.Entities;
using Fusion.Repository.ViewModels;
using Fusion.Repository.ViewModels.Project;
using Fusion.Service.ViewModels.Project.Requests;
using Fusion.Service.ViewModels.Project.Requests.Overview;
using Fusion.Service.ViewModels.Project.Responses;
using Fusion.Service.ViewModels.Project.Responses.Overview;
using Fusion.Service.ViewModels.ProjectMembers.Responses;

namespace Fusion.Service.IServices
{
    public interface IProjectService
    {
        Task<PagedResult<ProjectListResponse>> GetAllProjectAsync(ProjectSearchRequest req, CancellationToken ct = default);
        Task<ProjectDetailResponse> GetProjectDetailAsync(Guid id, CancellationToken ct = default); 
        Task<PagedResult<AllProjectOfMememberResponse>> GetProjectByMemberIdAsync(Guid userId, ProjectSearchRequest req, CancellationToken ct = default);
        Task<PagedResult<AllProjectOfMememberResponse>> GetProjectByActorIdAsync(Guid userId, ProjectSearchRequest req, CancellationToken ct = default);
        Task<List<StatusCountResponse>> GetCountProjectByStatusAsync(CancellationToken ct = default);

        Task<ProjectDetailResponse> CreateProjectAsync(
            Guid companyId,
            ProjectCreateRequest request,
            Guid actorUserId,
            CancellationToken ct = default);
        Task<ProjectListResult> GetProjectsForCompanyAsync(
        Guid companyId, ProjectListSearchRequest req, CancellationToken ct = default);

        Task<PagedResult<ProjectSummaryResponseV2>> GetProjectsForAdminAsync(ProjectSummarySearchRequest request, CancellationToken cancellationToken = default);
        Task<PagedResult<ProjectSummaryResponseV2>> GetProjectsByUserIdAsync(ProjectSummarySearchRequest request, Guid userId, CancellationToken cancellationToken = default);
        Task<ProjectSummaryResponseV2?> GetProjectsByIdForAdminAsync(Guid projectId, CancellationToken cancellationToken = default);
        Task<ProjectSummaryResponseV2?> GetProjectsByIdDetailsAsync(Guid projectId, CancellationToken cancellationToken = default);
        Task<ProjectResponseVersion3> GetProjectById(Guid projectId, CancellationToken cancellationToken = default);

        // =================== Over view =====================
        // NEW: thống kê New vs Completed
        Task<ProjectGrowthOverviewResponse> GetProjectGrowthOverviewAsync(
            ProjectGrowthOverviewRequest req,
            CancellationToken ct = default);
        Task<ProjectExecutionOverviewResponse> GetProjectExecutionOverviewAsync(
            ProjectGrowthOverviewRequest req,
            CancellationToken ct = default);
    }

}
