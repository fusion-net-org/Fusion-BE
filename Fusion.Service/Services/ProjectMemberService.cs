using AutoMapper;
using Fusion.Repository.Bases.Exceptions;
using Fusion.Repository.Bases.Page;
using Fusion.Repository.Bases.Page.ProjectMember;
using Fusion.Repository.IRepositories;
using Fusion.Repository.Repositories;
using Fusion.Service.IServices;
using Fusion.Service.ViewModels.ProjectMembers.Responses;
using Microsoft.EntityFrameworkCore;

public class ProjectMemberService : IProjectMemberService
{
    private readonly IProjectMemberRepository _projectMemberRepository;
    private readonly IMapper _mapper;

    public ProjectMemberService(IProjectMemberRepository projectMemberRepository,IMapper mapper)
    {
        _projectMemberRepository = projectMemberRepository;
        _mapper = mapper;
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

    public async Task<PagedResult<ProjectMemberResponseV2>> GetProjectMemberByProjectId(
     Guid projectId,
     ProjectMemberSearchRequestV2 request,
     CancellationToken ct = default)
    {
        // Call repository
        var pagedMembers = await _projectMemberRepository
            .GetProjectMemberByProjectId(projectId, request, ct);

        // Mapping list
        var mappedItems = _mapper.Map<List<ProjectMemberResponseV2>>(pagedMembers.Items);

        return new PagedResult<ProjectMemberResponseV2>
        {
            Items = mappedItems,
            TotalCount = pagedMembers.TotalCount,
            PageNumber = pagedMembers.PageNumber,
            PageSize = pagedMembers.PageSize
        };
    }

    public async Task<ProjectMemberChartResponse> GetProjectMemberChartsAsync(Guid projectId, CancellationToken ct = default)
    {
        var members = await _projectMemberRepository
            .GetAll()
            .Include(pm => pm.User)
            .Where(pm => pm.ProjectId == projectId)
            .ToListAsync(ct);

        var now = DateTime.UtcNow;

        var joinedOverTime = members
            .Where(m => m.JoinedAt.Year == now.Year)
            .GroupBy(m => m.JoinedAt.Month)
            .Select(g => new JoinedOverTimeItem
            {
                Month = new DateTime(now.Year, g.Key, 1).ToString("MMM"),
                Members = g.Count()
            }).OrderBy(x => DateTime.ParseExact(x.Month, "MMM", null)).ToList();

        var genderDistribution = members
            .GroupBy(m => m.User?.Gender ?? "Other")
            .Select(g => new GenderDistributionItem { Gender = g.Key, Count = g.Count() })
            .ToList();

        var statusDistribution = members
            .GroupBy(m => m.User.Status == true ? "Active" : "Inactive")
            .Select(g => new StatusDistributionItem { Status = g.Key, Count = g.Count() })
            .ToList();

        var partnerDistribution = members
            .GroupBy(m => m.IsPartner ? "Partner" : "Non-Partner")
            .Select(g => new PartnerDistributionItem { Type = g.Key, Count = g.Count() })
            .ToList();

        return new ProjectMemberChartResponse
        {
            JoinedOverTime = joinedOverTime,
            GenderDistribution = genderDistribution,
            StatusDistribution = statusDistribution,
            PartnerDistribution = partnerDistribution
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
