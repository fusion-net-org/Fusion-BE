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

    public async Task<PagedResult<AllProjectOfMememberResponse>> GetAllProjectsByMemberIdAsync(Guid userId, ProjectMemberSearchRequest request, CancellationToken cancellationToken = default)
    {
        var projectsPaged = await _projectMemberRepository.GetAllProjectsByMemberIdAsync(userId, request, cancellationToken);

        if (projectsPaged == null || !projectsPaged.Items.Any())
            throw CustomExceptionFactory.CreateNotFoundError("No projects found for this member in the specified company.");

        var items = projectsPaged.Items.Select(p =>
        {
            // Lấy membership hiện tại của user (repo đã Include ProjectMembers)
            var me = p.ProjectMembers?.FirstOrDefault(pm => pm.UserId == userId);


            // Nếu entity ProjectMember có IsViewAll -> lấy; nếu không bạn thay theo field thực tế
            var isViewAll = me?.IsViewAll ?? false;

            return new AllProjectOfMememberResponse
            {
                Id = p.Id,
                Name = p.Name ?? string.Empty,
                Code = p.Code ?? string.Empty,
                Status = p.Status ?? string.Empty,
            };
        }).ToList();

        return new PagedResult<AllProjectOfMememberResponse>
        {
            Items = items,
            TotalCount = projectsPaged.TotalCount,
            PageNumber = projectsPaged.PageNumber,
            PageSize = projectsPaged.PageSize
        };
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
