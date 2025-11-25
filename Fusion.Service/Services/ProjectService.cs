using AutoMapper;
using FluentValidation;
using Fusion.Repository.Bases.Exceptions;
using Fusion.Repository.Bases.Page;
using Fusion.Repository.Bases.Page.Project;
using Fusion.Repository.Bases.Responses;
using Fusion.Repository.Data;
using Fusion.Repository.Entities;
using Fusion.Repository.IRepositories;
using Fusion.Repository.Repositories;
using Fusion.Repository.ViewModels;
using Fusion.Repository.ViewModels.Project;
using Fusion.Service.Commons.Helpers;
using Fusion.Service.IServices;
using Fusion.Service.ViewModels.CompanySubscription.Requests;
using Fusion.Service.ViewModels.Project.Requests;
using Fusion.Service.ViewModels.Project.Requests.Overview;
using Fusion.Service.ViewModels.Project.Responses;
using Fusion.Service.ViewModels.Project.Responses.Overview;
using Fusion.Service.ViewModels.ProjectMembers.Responses;
using Fusion.Service.ViewModels.Sprint.Responses;
using Fusion.Service.ViewModels.Task.Response;
using Microsoft.EntityFrameworkCore;
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
        private readonly ICompanySubscriptionService _companySubscriptionService;
        public ProjectService(
            IMapper mapper,
            IValidator<ProjectCreateRequest> validator,
            IProjectRepository projectRepo,
            ISprintRepository sprintRepo,
            IProjectMemberRepository projMemberRepo,
            IWorkflowDesignerRepository workflowReadRepo,
            FusionDbContext ctx, ICompanySubscriptionService companySubscriptionService)
        {
            _mapper = mapper;
            _validator = validator;
            _projectRepo = projectRepo;
            _sprintRepo = sprintRepo;
            _projMemberRepo = projMemberRepo;
            _workflowReadRepo = workflowReadRepo;
            _ctx = ctx;
            _companySubscriptionService = companySubscriptionService;
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
                SprintLengthWeeks = request.SprintLengthWeeks,
                CreateAt = DateTime.UtcNow,
                UpdateAt = DateTime.UtcNow
            };

            // 6) Generate sprints by weeks
            var sprints = GenerateSprints(project.Id, request);

            // 7) Validate + stage members
            var stagedMembers = new List<(Guid userId, bool isPartner)>();

            if (request.MemberIds != null && request.MemberIds.Count > 0)
            {
                // owner company luôn có
                var ownerCompanyId = companyId;
                Guid? hiredCompanyId = request.IsHired ? request.CompanyRequestId : null;

                foreach (var uid in request.MemberIds.Distinct())
                {
                    // 7.1: check user thuộc owner company?
                    var belongsOwner = await _projMemberRepo
                        .UserBelongsToCompanyAsync(uid, ownerCompanyId, ct);

                    var belongsHired = false;
                    if (hiredCompanyId.HasValue)
                    {
                        belongsHired = await _projMemberRepo
                            .UserBelongsToCompanyAsync(uid, hiredCompanyId.Value, ct);
                    }

                    if (!belongsOwner && !belongsHired)
                    {
                        throw CustomExceptionFactory.CreateBadRequestError(
                            $"User {uid} is not a member of the owner/hired company.");
                    }

                    var isPartner = belongsHired && !belongsOwner;
                    stagedMembers.Add((uid, isPartner));
                }
            }

            // (option) tự add luôn actor thành member nếu chưa có
            if (!stagedMembers.Any(m => m.userId == actorUserId))
            {
                stagedMembers.Add((actorUserId, isPartner: false));
            }



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

                var totalTasks = sprintSummary.Sum(s => s.TaskCount);

                var doneTasks = sprintSummary
                    .SelectMany(s => s.Tasks)
                    .Count(t => (t.Status ?? "").Equals("Done", StringComparison.OrdinalIgnoreCase));
                double progress = totalTasks == 0 ? 0 : (double)doneTasks / totalTasks * 100;

                response.Items.Add(new ProjectSummaryResponseV2
                {
                    Id = p.Id,
                    Name = p.Name ?? "N/A",
                    Description = p.Description ?? "",
                    Status = p.Status ?? "Unknown",

                    CompanyExecutorId = p.Company?.Id ?? Guid.Empty,
                    CompanyExecutorName = p.Company?.Name ?? "N/A",

                    CompanyRequestId = p.CompanyRequest?.Id,
                    CompanyRequestName = p.CompanyRequest?.Name ?? "N/A",

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
                    MembersCount = p.ProjectMembers?.Count ?? 0,
                    SprintCount = sprintSummary.Count,
                    TotalTask = totalTasks,
                    TotalPoint = sprintSummary.Sum(s => s.TotalPoint),
                    Progress = Math.Round(progress, 2),
                    Sprints = sprintSummary
                });
            }

            return response;
        }

        public async Task<PagedResult<ProjectSummaryResponseV2>> GetProjectsByUserIdAsync(ProjectSummarySearchRequest request, Guid userId, CancellationToken cancellationToken = default)
        {
            var result = await _projectRepo.GetProjectsByUserIdAsync(request, userId, cancellationToken);

            var response = new PagedResult<ProjectSummaryResponseV2>
            {
                PageNumber = result.PageNumber,
                TotalCount = result.TotalCount,
                PageSize = result.PageSize,
                Items = new List<ProjectSummaryResponseV2>()
            };

            foreach (var p in result.Items ?? new List<Project>())
            {
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

                var totalTasks = sprintSummary.Sum(s => s.TaskCount);

                var doneTasks = sprintSummary
                    .SelectMany(s => s.Tasks)
                    .Count(t => (t.Status ?? "").Equals("Done", StringComparison.OrdinalIgnoreCase));
                double progress = totalTasks == 0 ? 0 : (double)doneTasks / totalTasks * 100;

                response.Items.Add(new ProjectSummaryResponseV2
                {
                    Id = p.Id,
                    Name = p.Name ?? "N/A",
                    Description = p.Description ?? "",
                    Status = p.Status ?? "Unknown",

                    CompanyExecutorId = p.Company?.Id ?? Guid.Empty,
                    CompanyExecutorName = p.Company?.Name ?? "N/A",

                    CompanyRequestId = p.CompanyRequest?.Id,
                    CompanyRequestName = p.CompanyRequest?.Name ?? "N/A",

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
                    MembersCount = p.ProjectMembers?.Count ?? 0,
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
            var totalMembers = result.ProjectMembers?.Count ?? 0;
            var doneTasks = sprintSummary
                .SelectMany(s => s.Tasks)
                .Count(t => (t.Status ?? "")
                .Equals("Done", StringComparison.OrdinalIgnoreCase));

            double progress = totalTasks == 0 ? 0 : (double)doneTasks / totalTasks * 100;

            return new ProjectSummaryResponseV2
            {
                Id = result.Id,
                Name = result.Name,
                Description = result.Description ?? "",
                Status = result.Status ?? "Unknown",
                StartDate = result.StartDate,
                EndDate = result.EndDate,
                CompanyExecutorId = result.Company?.Id ?? Guid.Empty,
                CompanyExecutorName = result.Company?.Name ?? "N/A",
                CompanyRequestId = result.CompanyRequest?.Id,
                CompanyRequestName = result.CompanyRequest?.Name ?? "N/A",
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
                    Avatar = m.User?.Avatar,
                }).ToList() ?? new List<ProjectMemberSummaryResponse>(),
                SprintCount = sprintSummary.Count,
                MembersCount = totalMembers,
                TotalTask = totalTasks,
                TotalPoint = sprintSummary.Sum(s => s.TotalPoint),
                Progress = Math.Round(progress, 2),
                Sprints = sprintSummary
            };
        }

        public async Task<ProjectSummaryResponseV2?> GetProjectsByIdDetailsAsync(Guid projectId, CancellationToken cancellationToken = default)
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
            var totalMembers = result.ProjectMembers?.Count ?? 0;
            var doneTasks = sprintSummary
                .SelectMany(s => s.Tasks)
                .Count(t => (t.Status ?? "")
                .Equals("Done", StringComparison.OrdinalIgnoreCase));

            double progress = totalTasks == 0 ? 0 : (double)doneTasks / totalTasks * 100;

            return new ProjectSummaryResponseV2
            {
                Id = result.Id,
                Name = result.Name,
                Code = result.Code,
                Description = result.Description ?? "",
                Status = result.Status ?? "Unknown",
                StartDate = result.StartDate,
                EndDate = result.EndDate,
                CompanyExecutorId = result.Company?.Id ?? Guid.Empty,
                CompanyExecutorName = result.Company?.Name ?? "N/A",
                CompanyRequestId = result.CompanyRequest?.Id,
                CompanyRequestName = result.CompanyRequest?.Name ?? "N/A",
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
                    Avatar = m.User?.Avatar,
                }).ToList() ?? new List<ProjectMemberSummaryResponse>(),
                SprintCount = sprintSummary.Count,
                MembersCount = totalMembers,
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

        // =================== Over view =====================
        public async Task<ProjectGrowthOverviewResponse> GetProjectGrowthOverviewAsync(ProjectGrowthOverviewRequest req, CancellationToken ct = default)
        {
            var nowUtc = DateTime.UtcNow;

            // Default: 12 tháng gần nhất
            var defaultFrom = new DateTime(nowUtc.Year, nowUtc.Month, 1).AddMonths(-11);
            var from = (req.From ?? defaultFrom).ToUniversalTime();
            var to = (req.To ?? nowUtc).ToUniversalTime();

            // Lấy thống kê theo tháng từ repository
            var monthlyStats = await _projectRepo.GetProjectMonthlyCreationAndCompletionAsync(
                req.CompanyId,
                from,
                to,
                ct);

            // Build dải tháng liên tục (kể cả tháng ko có project -> 0)
            var points = new List<ProjectGrowthPointResponse>();

            var year = from.Year;
            var month = from.Month;
            var cumulative = 0;

            while (new DateTime(year, month, 1) <= new DateTime(to.Year, to.Month, 1))
            {
                var stat = monthlyStats.FirstOrDefault(x => x.Year == year && x.Month == month);

                var newProjects = stat?.NewProjects ?? 0;
                var completedProjects = stat?.CompletedProjects ?? 0;

                cumulative += newProjects;

                points.Add(new ProjectGrowthPointResponse
                {
                    Year = year,
                    Month = month,
                    NewProjects = newProjects,
                    CompletedProjects = completedProjects,
                    CumulativeProjects = cumulative
                });

                month++;
                if (month > 12)
                {
                    month = 1;
                    year++;
                }
            }

            // ===== Summary counters =====
            var baseQuery = _ctx.Projects.AsNoTracking().AsQueryable();

            if (req.CompanyId.HasValue)
            {
                baseQuery = baseQuery.Where(p => p.CompanyId == req.CompanyId.Value);
            }

            var totalProjects = await baseQuery.CountAsync(ct);

            // Completed = có EndDate <= hôm nay
            var todayDateOnly = DateOnly.FromDateTime(nowUtc.Date);

            var completedProjectsTotal = await baseQuery
                .Where(p => p.EndDate != null && p.EndDate <= todayDateOnly)
                .CountAsync(ct);

            var activeProjects = totalProjects - completedProjectsTotal;

            var last30Days = nowUtc.AddDays(-30);

            var newProjectsLast30Days = await baseQuery
                .Where(p => p.CreateAt >= last30Days)
                .CountAsync(ct);

            return new ProjectGrowthOverviewResponse
            {
                TotalProjects = totalProjects,
                ActiveProjects = activeProjects,
                CompletedProjects = completedProjectsTotal,
                NewProjectsLast30Days = newProjectsLast30Days,
                Growth = points
            };
        }

        public async Task<ProjectExecutionOverviewResponse> GetProjectExecutionOverviewAsync( ProjectGrowthOverviewRequest req, CancellationToken ct = default)
        {
            // Không xử lý from/to ở đây nữa,
            // để repo handle default range (12 tháng gần nhất) cho đồng bộ.
            var vm = await _projectRepo.GetProjectExecutionOverviewAsync(
                req.CompanyId,
                req.From,
                req.To,
                ct);

            var response = new ProjectExecutionOverviewResponse
            {
                TotalTasks = vm.TotalTasks,
                CompletedTasks = vm.CompletedTasks,
                OverdueTasks = vm.OverdueTasks,

                TotalSprints = vm.TotalSprints,
                ActiveSprints = vm.ActiveSprints,
                CompletedSprints = vm.CompletedSprints,

                TaskFlow = vm.TaskFlow
                    .Select(x => new TaskFlowPointResponse
                    {
                        Year = x.Year,
                        Month = x.Month,
                        CreatedTasks = x.CreatedTasks,
                        CompletedTasks = x.CompletedTasks
                    })
                    .ToList(),

                SprintVelocity = vm.SprintVelocity
                    .Select(x => new SprintVelocityPointResponse
                    {
                        SprintId = x.SprintId,
                        SprintName = x.SprintName,
                        StartDate = x.StartDate,
                        EndDate = x.EndDate,
                        CommittedPoints = x.CommittedPoints,
                        CompletedPoints = x.CompletedPoints
                    })
                    .ToList()
            };

            return response;

        }

        public async Task<List<ProjectResponseVersion3>> GetProjectsByCompanyAsync(
           Guid companyId,
           Guid? companyRequestId,
           Guid? executorCompanyId,
           CancellationToken cancellationToken = default)
        {
            var projects = await _projectRepo.GetProjectsByCompanyAsync(
                companyId,
                companyRequestId,
                executorCompanyId,
                cancellationToken);

            return _mapper.Map<List<ProjectResponseVersion3>>(projects);
        }



    }
}
