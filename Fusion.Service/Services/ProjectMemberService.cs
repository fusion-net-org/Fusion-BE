using AutoMapper;
using Fusion.Repository.Bases.Exceptions;
using Fusion.Repository.Bases.Page;
using Fusion.Repository.Bases.Page.ProjectMember;
using Fusion.Repository.Data;
using Fusion.Repository.IRepositories;
using Fusion.Repository.Repositories;
using Fusion.Service.IServices;
using Fusion.Service.ViewModels.ProjectMembers.Request;
using Fusion.Service.ViewModels.ProjectMembers.Responses;
using Microsoft.EntityFrameworkCore;

public class ProjectMemberService : IProjectMemberService
{
    private readonly IProjectMemberRepository _projectMemberRepository;
    private readonly IMapper _mapper;

    private readonly FusionDbContext _context;
    public ProjectMemberService(IProjectMemberRepository projectMemberRepository, IMapper mapper, FusionDbContext context)
    {
        _projectMemberRepository = projectMemberRepository;
        _mapper = mapper;
        _context = context;
    }
    public async Task<List<ProjectMemberRoleResponse>> GetProjectMembersWithRoleAsync(
       Guid projectId,
       CancellationToken ct = default)
    {
        var members = await _projectMemberRepository
            .GetProjectMembersWithUserAndRoleAsync(projectId, ct);

        if (members == null || members.Count == 0)
            return new List<ProjectMemberRoleResponse>();

        var result = members.Select(pm =>
        {
            // Xác định company của project (internal vs hired)
            var projectCompanyId = pm.Project != null
                ? (pm.Project.IsHired
                    ? pm.Project.CompanyRequestId
                    : pm.Project.CompanyId)
                : null;

            // Role trong company đó
            var roleName = pm.User?.UserRoles
                .Where(ur =>
                    ur.Role != null &&
                    projectCompanyId.HasValue &&
                    ur.Role.CompanyId == projectCompanyId.Value)
                .Select(ur => ur.Role!.RoleName)
                .FirstOrDefault();

            return new ProjectMemberRoleResponse
            {
                ProjectId = pm.ProjectId ?? projectId,
                UserId = pm.UserId ?? Guid.Empty,
                UserName = pm.User?.UserName,
                Email = pm.User?.Email,
                AvatarUrl = pm.User?.Avatar, 
                CompanyRoleName = roleName,
                IsPartner = pm.IsPartner,
                IsViewAll = pm.IsViewAll,
                JoinedAt = pm.JoinedAt
            };
        }).ToList();

        return result;
    }
    public async Task<ProjectMemberResponseV2> AddMemberAsync(
    ProjectMemberCreateRequest request,
    CancellationToken ct = default)
    {
        // 1) Validate cơ bản
        if (request.ProjectId == Guid.Empty || request.CompanyId == Guid.Empty || request.UserId == Guid.Empty)
            throw CustomExceptionFactory.CreateBadRequestError("Invalid ProjectId / CompanyId / UserId.");

        var belongs = await _projectMemberRepository.UserBelongsToCompanyAsync(
            request.UserId,
            request.CompanyId,
            ct);

        if (!belongs)
            throw CustomExceptionFactory.CreateBadRequestError("User does not belong to this company or is inactive.");

        await _projectMemberRepository.AddIfNotExistsAsync(
            request.ProjectId,
            request.UserId,
            request.IsPartner,
            request.IsViewAll,
            ct);

        await _context.SaveChangesAsync(ct);

        var entity = await _projectMemberRepository
            .GetAll()
            .Include(pm => pm.User)
            .Include(pm => pm.Project)
            .FirstAsync(pm => pm.ProjectId == request.ProjectId && pm.UserId == request.UserId, ct);

        return _mapper.Map<ProjectMemberResponseV2>(entity);
    }

    public async Task RemoveMemberAsync(Guid projectId, Guid userId, CancellationToken ct = default)
    {
        if (projectId == Guid.Empty || userId == Guid.Empty)
            throw CustomExceptionFactory.CreateBadRequestError("Invalid ProjectId / UserId.");

        await _projectMemberRepository.RemoveAsync(projectId, userId, ct);
        await _context.SaveChangesAsync(ct);
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
