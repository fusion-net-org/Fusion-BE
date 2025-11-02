using Fusion.Repository.Bases.Exceptions;
using Fusion.Repository.Bases.Page;
using Fusion.Repository.Bases.Page.ProjectMember;
using Fusion.Repository.IRepositories;
using Fusion.Service.IServices;
using Fusion.Service.ViewModels.ProjectMembers.Responses;

public class ProjectMemberService : IProjectMemberService
{
    private readonly IProjectMemberRepository _projectMemberRepository;

    public ProjectMemberService(IProjectMemberRepository projectMemberRepository)
    {
        _projectMemberRepository = projectMemberRepository;
    }

    public async Task<PagedResult<MemberProjectListResponse>> GetProjectsByMemberAsync(
        Guid companyId,
        Guid userId,
        ProjectMemberSearchRequest request,
        CancellationToken cancellationToken = default)
    {
        var projectsPaged = await _projectMemberRepository.GetProjectsByMemberAsync(companyId, userId, request, cancellationToken);

        if (projectsPaged == null || !projectsPaged.Items.Any())
            throw CustomExceptionFactory.CreateNotFoundError("No projects found for this member in the specified company.");

        var projectResponses = projectsPaged.Items.Select(p => new ProjectBelongToMemberResponse
        {
            Id = p.Id,
            Name = p.Name,
            Code = p.Code,
            Status = p.Status,
            StartDate = p.StartDate,
            EndDate = p.EndDate,
            IsHired = p.IsHired
        }).ToList();

        var result = new MemberProjectListResponse
        {
            CompanyId = companyId,
            UserId = userId,
            TotalProject = projectsPaged.TotalCount,
            Projects = projectResponses
        };

        return new PagedResult<MemberProjectListResponse>
        {
            Items = new List<MemberProjectListResponse> { result },
            TotalCount = projectsPaged.TotalCount,
            PageNumber = projectsPaged.PageNumber,
            PageSize = projectsPaged.PageSize
        };
    }
}
