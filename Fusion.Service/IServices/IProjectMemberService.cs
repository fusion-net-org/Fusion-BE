
using Fusion.Repository.Bases.Page;
using Fusion.Repository.Bases.Page.ProjectMember;
using Fusion.Repository.Entities;
using Fusion.Service.ViewModels.ProjectMembers.Responses;

namespace Fusion.Service.IServices
{
    public interface IProjectMemberService
    {
        Task<PagedResult<MemberProjectListResponse>> GetProjectsByMemberAsync(Guid companyId, Guid userId, ProjectMemberSearchRequest request, CancellationToken cancellationToken = default);
        Task<PagedResult<AllProjectOfMememberResponse>> GetAllProjectsByMemberIdAsync(Guid userId, ProjectMemberSearchRequest request, CancellationToken cancellationToken = default);
        Task<PagedResult<ProjectMemberResponseV2>> GetProjectMemberByProjectId(Guid projectId, ProjectMemberSearchRequestV2 request, CancellationToken ct = default);
        Task<ProjectMemberChartResponse> GetProjectMemberChartsAsync(Guid projectId, CancellationToken ct = default);

    }
}
