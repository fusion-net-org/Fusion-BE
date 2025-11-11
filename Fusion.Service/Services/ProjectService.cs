using AutoMapper;
using FluentValidation;
using Fusion.Repository.Bases.Exceptions;
using Fusion.Repository.Bases.Page;
using Fusion.Repository.Bases.Page.Project;
using Fusion.Repository.Bases.Responses;
using Fusion.Repository.Data;
using Fusion.Repository.Entities;
using Fusion.Repository.Enums;
using Fusion.Repository.IRepositories;
using Fusion.Repository.Repositories;
using Fusion.Repository.ViewModels;
using Fusion.Service.Commons.Helpers;
using Fusion.Service.IServices;
using Fusion.Service.ViewModels.Project.Requests;
using Fusion.Service.ViewModels.Project.Responses;
using Fusion.Service.ViewModels.ProjectMembers.Responses;
using Fusion.Service.ViewModels.Sprint.Responses;
using Fusion.Service.ViewModels.Task.Response;
using Microsoft.EntityFrameworkCore;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Fusion.Service.Services
{

    public class ProjectService : IProjectService
    {
        private readonly IMapper _mapper;
        private readonly IValidator<ProjectCreateRequest> _validator;
        private readonly IProjectRepository _projectRepo;
        private readonly ISprintRepository _sprintRepo;
        private readonly IProjectMemberRepository _projMemberRepo;
        private readonly IWorkflowDesignerRepository _workflowReadRepo;
        private readonly FusionDbContext _ctx;

        public ProjectService(
            IMapper mapper,
            IValidator<ProjectCreateRequest> validator,
            IProjectRepository projectRepo,
            ISprintRepository sprintRepo,
            IProjectMemberRepository projMemberRepo,
            IWorkflowDesignerRepository workflowReadRepo,
            FusionDbContext ctx)
        {
            _mapper = mapper;
            _validator = validator;
            _projectRepo = projectRepo;
            _sprintRepo = sprintRepo;
            _projMemberRepo = projMemberRepo;
            _workflowReadRepo = workflowReadRepo;
            _ctx = ctx;
        }

        public async Task<ProjectDetailResponse> CreateProjectAsync(
            Guid companyId, ProjectCreateRequest request, Guid actorUserId, CancellationToken ct = default)
        {
            await _validator.ValidateAndThrowAsync(request, ct);

            // 1) Company
            var company = await _ctx.Companies
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == companyId && (c.IsDeleted ?? false) == false, ct);
            if (company == null)
                throw CustomExceptionFactory.CreateNotFoundError("Company not found.");

            // 2) Hired company (optional)
            if (request.IsHired)
            {
                if (request.CompanyRequestId == null || request.CompanyRequestId == companyId)
                    throw CustomExceptionFactory.CreateBadRequestError("Hired company invalid.");

                var hired = await _ctx.Companies.AsNoTracking()
                    .FirstOrDefaultAsync(c => c.Id == request.CompanyRequestId && (c.IsDeleted ?? false) == false, ct);
                if (hired == null)
                    throw CustomExceptionFactory.CreateBadRequestError("Hired company not found.");
            }

            // 3) Unique project code per company
            if (await _projectRepo.IsCodeExistedAsync(companyId, request.Code.Trim(), ct))
                throw CustomExceptionFactory.CreateBadRequestError(
                    ResponseMessages.DUPLICATE.FormatMessage("Project code"));

            // 4) Workflow must exist in this company (ONLY store Id)
            var wfExists = await _workflowReadRepo.ExistsInCompanyAsync(request.WorkflowId, companyId, ct);
            if (!wfExists)
                throw CustomExceptionFactory.CreateBadRequestError("Workflow does not exist in this company.");

            // 5) Build project
            var project = new Project
            {
                Id = Guid.NewGuid(),
                CompanyId = companyId,
                IsHired = request.IsHired,
                CompanyRequestId = request.CompanyRequestId,
                ProjectRequestId = request.ProjectRequestId,
                Code = request.Code.Trim(),
                Name = request.Name.Trim(),
                Description = request.Description,
                Status = request.Status,
                WorkflowId = request.WorkflowId, // <-- only Id
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                CreatedBy = actorUserId,
                CreateAt = DateTime.UtcNow,
                UpdateAt = DateTime.UtcNow
            };

            // 6) Generate sprints by weeks
            var sprints = GenerateSprints(project.Id, request);

            // 7) Validate + stage members
            var validCompanies = new HashSet<Guid> { companyId };
            if (request.IsHired && request.CompanyRequestId.HasValue)
                validCompanies.Add(request.CompanyRequestId.Value);

            var stagedMembers = new List<(Guid userId, bool isPartner)>();


            // 8) Transactional save
            using var tx = await _ctx.Database.BeginTransactionAsync(ct);
            try
            {
                await _ctx.Projects.AddAsync(project, ct);
                await _sprintRepo.AddRangeAsync(sprints, ct);

                foreach (var (uid, isPartner) in stagedMembers)
                    await _projMemberRepo.AddIfNotExistsAsync(project.Id, uid, isPartner, isViewAll: false, ct);

                await _ctx.SaveChangesAsync(ct);
                await tx.CommitAsync(ct);
            }
            catch
            {
                await tx.RollbackAsync(ct);
                throw;
            }

            // 9) Return
            var created = await _projectRepo.GetByIdWithSprintsAsync(project.Id, ct);
            var res = new ProjectDetailResponse
            {
                Id = created.Id,
                Code = created.Code,
                Name = created.Name,
                Description = created.Description,
                Status = created.Status,
                CompanyRequestId = created.ProjectRequestId,

                IsHired = created.IsHired,
                CompanyId = created.CompanyId,
                CompanyHiredId = created.CompanyRequestId,

                StartDate = created.StartDate?.ToDateTime(TimeOnly.MinValue),
                EndDate = created.EndDate?.ToDateTime(TimeOnly.MinValue),

                CreatedBy = created.CreatedBy,
                CreateAt = created.CreateAt,
                UpdateAt = created.UpdateAt,

                CompanyName = created.Company?.Name,
                CompanyHiredName = created.CompanyRequest?.Name,
                CreatedByName = created.CreatedByNavigation?.UserName,

                Sprints = created.Sprints
        .Where(s => !s.IsDeleted)
        .OrderBy(s => s.StartDate)
        .Select(s => new SprintDto
        {
            Id = s.Id,
            Name = s.Name,
            StartDate = s.StartDate,
            EndDate = s.EndDate,
        })
        .ToList()
            };
            if (res != null)
            {
                res.CompanyName = company.Name;
            }
            return res!;
        }

        private static List<Sprint> GenerateSprints(Guid projectId, ProjectCreateRequest req)
        {
            var list = new List<Sprint>();
            if (!req.StartDate.HasValue || !req.EndDate.HasValue) return list;

            var daysPerSprint = Math.Max(1, req.SprintLengthWeeks) * 7;
            var cur = req.StartDate.Value;
            int idx = 1;

            while (cur <= req.EndDate.Value)
            {
                var to = cur.AddDays(daysPerSprint - 1);
                if (to > req.EndDate.Value) to = req.EndDate.Value;

                list.Add(new Sprint
                {
                    Id = Guid.NewGuid(),
                    ProjectId = projectId,
                    Name = $"Sprint {idx++}",
                    StartDate = cur.ToDateTime(TimeOnly.MinValue),
                    EndDate = to.ToDateTime(TimeOnly.MaxValue),
                    Status = 0,
                    IsDeleted = false,
                    CreatedAt = DateTime.UtcNow,
                    UpdateAt = DateTime.UtcNow
                });

                cur = to.AddDays(1);
            }
            return list;
        }
        public async Task<ProjectListResult> GetProjectsForCompanyAsync(
      Guid companyId, ProjectListSearchRequest req, CancellationToken ct = default)
        {
            var statusStrings = req.Statuses?.Select(s => s.ToString());

            var result = await _projectRepo.GetProjectsForCompanyAsync(
                companyId: companyId,
                q: req.Q,
                statuses: statusStrings,   // convert enum -> string
                sort: req.Sort,
                pageNumber: req.PageNumber,
                pageSize: req.PageSize,
                ct: ct);

            var (entities, total) = result;

            var items = entities.Select(p => new ProjectListItemResponse
            {
                Id = p.Id,
                Code = p.Code ?? "",
                Name = p.Name ?? "",
                Description = p.Description,
                OwnerCompany = p.Company != null ? p.Company.Name : "",
                HiredCompany = p.CompanyRequest != null ? p.CompanyRequest.Name : null,
                Workflow = p.Company != null && p.Workflow != null
                                  ? $"{p.Company.Name} — {p.Workflow.Name}"
                                  : p.Workflow?.Name,
                StartDate = p.StartDate.HasValue
    ? p.StartDate.Value.ToDateTime(TimeOnly.MinValue)
    : (DateTime?)null,

                EndDate = p.EndDate.HasValue
    ? p.EndDate.Value.ToDateTime(TimeOnly.MinValue)
    : (DateTime?)null,

                Status = p.Status ?? "Planned",
                Ptype = p.CompanyRequestId == companyId ? "Outsourced" : "Internal",
                IsRequest = (p.CompanyRequestId == companyId)
            }).ToList();

            return new ProjectListResult
            {
                Items = items,
                TotalCount = total,
                PageNumber = Math.Max(1, req.PageNumber),
                PageSize = Math.Max(1, req.PageSize)
            };
        }

        public async Task<PagedResult<ProjectListResponse>> GetAllProjectAsync(ProjectSearchRequest req, CancellationToken ct = default)
        {
            //var result = await _repo.GetAllProjectAsync(req, ct);
            //return result.Map(p => _mapper.Map<ProjectListResponse>(p));
            throw new NotImplementedException();
        }
        public async Task<List<StatusCountResponse>> GetCountProjectByStatusAsync(CancellationToken ct = default)
        {
            var rows = await _projectRepo.GetCountProjectByStatusAsync(ct);
            return rows ?? new List<StatusCountResponse>();
        }
        public async Task<PagedResult<AllProjectOfMememberResponse>> GetProjectByMemberIdAsync(Guid userId, ProjectSearchRequest req, CancellationToken ct = default)
        {
            var projectsPaged = await _projectRepo.GetProjectByMemberIdAsync(userId, req, ct);

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
        public async Task<PagedResult<AllProjectOfMememberResponse>> GetProjectByActorIdAsync(Guid userId, ProjectSearchRequest req, CancellationToken ct = default)
        {
            var projectsPaged = await _projectRepo.GetProjectByActorIdAsync(userId, req, ct);

            if (projectsPaged == null || projectsPaged.Items == null || projectsPaged.Items.Count == 0)
                throw CustomExceptionFactory.CreateNotFoundError("No projects created by this user.");

            var items = projectsPaged.Items.Select(p => new AllProjectOfMememberResponse
            {
                Id = p.Id,
                Name = p.Name ?? string.Empty,
                Code = p.Code ?? string.Empty,
                Status = p.Status ?? string.Empty
            }).ToList();

            return new PagedResult<AllProjectOfMememberResponse>
            {
                Items = items,
                TotalCount = projectsPaged.TotalCount,
                PageNumber = projectsPaged.PageNumber,
                PageSize = projectsPaged.PageSize
            };
        }
        public Task<ProjectDetailResponse> GetProjectDetailAsync(Guid id, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public async Task<PagedResult<ProjectSummaryResponseV2>> GetProjectsForAdminAsync(ProjectSummarySearchRequest request, CancellationToken cancellationToken = default)
        {
            var result = await _projectRepo.GetProjectsForAdminAsync(request, cancellationToken);

            var response = new PagedResult<ProjectSummaryResponseV2>
            {
                PageNumber = result.PageNumber,
                TotalCount = result.TotalCount,
                PageSize = result.PageSize,
                Items = new List<ProjectSummaryResponseV2>()
            };

            foreach (var p in result.Items ?? new List<Project>())
            {
                // ✅ Sprints
                var sprintSummary = (p.Sprints ?? new List<Sprint>())
                    .Select(s => new SprintSummaryResponse
                    {
                        Id = s.Id,
                        Name = s.Name ?? "N/A",
                        TaskCount = s.ProjectTasks?.Count ?? 0,
                        TotalPoint = s.ProjectTasks?.Sum(t => t.Point ?? 0) ?? 0,
                        Tasks = (s.ProjectTasks ?? new List<ProjectTask>())
                            .Select(t => new TaskSummaryResponse
                            {
                                Id = t.Id,
                                Title = t.Title ?? "N/A",
                                Point = t.Point ?? 0,
                                Status = t.Status ?? "Unknown"
                            })
                            .ToList()
                    })
                    .ToList();

                // ✅ Progress
                var totalTasks = sprintSummary.Sum(s => s.TaskCount);
                var doneTasks = sprintSummary
                    .SelectMany(s => s.Tasks)
                    .Count(t => (t.Status ?? "").Equals("Done", StringComparison.OrdinalIgnoreCase));
                double progress = totalTasks == 0 ? 0 : (double)doneTasks / totalTasks * 100;

                // ✅ Safe add ProjectSummaryResponseV2
                response.Items.Add(new ProjectSummaryResponseV2
                {
                    Id = p.Id,
                    Name = p.Name ?? "N/A",

                    CompanyId = p.Company?.Id ?? Guid.Empty,
                    CompanyName = p.Company?.Name ?? "N/A",

                    CompanyHiredId = p.CompanyRequest?.Id,
                    CompanyHiredName = p.CompanyRequest?.Name ?? "N/A",

                    WorkflowId = p.Workflow?.Id ?? Guid.Empty,
                    WorkflowName = p.Workflow?.Name ?? "N/A",

                    ProjectType = p.CompanyRequestId != null ? "OutSource" : "Product",

                    OwnerId = p.CreatedByNavigation?.Id ?? Guid.Empty,
                    OwnerName = p.CreatedByNavigation?.UserName ?? "Unknown",

                    Members = (p.ProjectMembers ?? new List<ProjectMember>())
                        .Select(m => new ProjectMemberSummaryResponse
                        {
                            MemberId = m.User?.Id ?? Guid.Empty,
                            MemberName = m.User?.UserName ?? "Unknown",
                            Avatar = m.User?.Avatar
                        })
                        .ToList(),

                    SprintCount = sprintSummary.Count,
                    TotalTask = totalTasks,
                    TotalPoint = sprintSummary.Sum(s => s.TotalPoint),
                    Progress = Math.Round(progress, 2),
                    Sprints = sprintSummary
                });
            }

            return response;
        }

       
        public async Task<ProjectSummaryResponseV2?> GetProjectsByIdForAdminAsync(Guid projectId, CancellationToken cancellationToken = default)
        {
            var result = await _projectRepo.GetProjectsByIdForAdminAsync(projectId, cancellationToken);

            if (result == null)
                throw CustomExceptionFactory.CreateNotFoundError("Projects Not found");

            var sprintSummary = (result.Sprints ?? new List<Sprint>()).Select(s => new SprintSummaryResponse
            {
                Id = s.Id,
                Name = s.Name ?? "N/A",
                TaskCount = s.ProjectTasks?.Count ?? 0,
                TotalPoint = s.ProjectTasks?.Sum(t => t.Point ?? 0) ?? 0,
                Tasks = s.ProjectTasks.Select(t => new TaskSummaryResponse
                {
                    Id = t.Id,
                    Title = t.Title ?? "N/A",
                    Point = t.Point ?? 0,
                    Status = t.Status ?? "Unknown"
                }).ToList()
            }).ToList();

            var totalTasks = sprintSummary.Sum(s => s.TaskCount);
            var doneTasks = sprintSummary
                .SelectMany(s => s.Tasks)
                .Count(t => (t.Status ?? "")
                .Equals("Done", StringComparison.OrdinalIgnoreCase));

            double progress = totalTasks == 0 ? 0 : (double)doneTasks / totalTasks * 100;

            return new ProjectSummaryResponseV2
            {
                Id = result.Id,
                Name = result.Name,
                CompanyId = result.Company?.Id ?? Guid.Empty,
                CompanyName = result.Company?.Name ?? "N/A",
                CompanyHiredId = result.CompanyRequest?.Id,
                CompanyHiredName = result.CompanyRequest?.Name ?? "N/A",
                WorkflowId = result.Workflow.Id,
                WorkflowName = result.Workflow?.Name ?? "N/A",
                ProjectType = result.CompanyRequestId != null ? "OutSource" : "Product",
                OwnerId = result.CreatedByNavigation?.Id ?? Guid.Empty,
                OwnerName = result.CreatedByNavigation?.UserName ?? "Unknown",
                Members = (result.ProjectMembers ?? new List<ProjectMember>())
                .Select(m => new ProjectMemberSummaryResponse
                {
                    MemberId = m.User?.Id ?? Guid.Empty,
                    MemberName = m.User?.UserName ?? "N/A",
                    Avatar = m.User.Avatar,
                }).ToList() ?? new List<ProjectMemberSummaryResponse>(),
                SprintCount = sprintSummary.Count,
                TotalTask = totalTasks,
                TotalPoint = sprintSummary.Sum(s => s.TotalPoint),
                Progress = Math.Round(progress, 2),
                Sprints = sprintSummary
            };
        }

        public async Task<ProjectResponseVersion3> GetProjectById(Guid projectId, CancellationToken cancellationToken = default)
        {
            var project = await _projectRepo.GetProjectById(projectId, cancellationToken);
            if (project == null)
                throw CustomExceptionFactory.CreateNotFoundError("Project not found");

            return _mapper.Map<ProjectResponseVersion3>(project);
        }


    }
}
