using AutoMapper;
using FluentValidation;
using Fusion.Repository.Bases.Exceptions;
using Fusion.Repository.Bases.Responses;
using Fusion.Repository.Data;
using Fusion.Repository.Entities;
using Fusion.Repository.IRepositories;
using Fusion.Repository.Repositories;
using Fusion.Service.IServices;
using Fusion.Service.ViewModels.Project.Requests;
using Fusion.Service.ViewModels.Project.Responses;
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
                if (request.CompanyHiredId == null || request.CompanyHiredId == companyId)
                    throw CustomExceptionFactory.CreateBadRequestError("Hired company invalid.");

                var hired = await _ctx.Companies.AsNoTracking()
                    .FirstOrDefaultAsync(c => c.Id == request.CompanyHiredId && (c.IsDeleted ?? false) == false, ct);
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
                CompanyHiredId = request.CompanyHiredId,
                ProjectRequestId = null,
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
            if (request.IsHired && request.CompanyHiredId.HasValue)
                validCompanies.Add(request.CompanyHiredId.Value);

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

                IsHired = created.IsHired,
                CompanyId = created.CompanyId,
                CompanyHiredId = created.CompanyHiredId,

                StartDate = created.StartDate?.ToDateTime(TimeOnly.MinValue),
                EndDate = created.EndDate?.ToDateTime(TimeOnly.MinValue),

                CreatedBy = created.CreatedBy,
                CreateAt = created.CreateAt,
                UpdateAt = created.UpdateAt,

                CompanyName = created.Company?.Name,
                CompanyHiredName = created.CompanyHired?.Name,
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
                HiredCompany = p.CompanyHired != null ? p.CompanyHired.Name : null,
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
                Ptype = p.IsHired ? "Outsourced" : "Internal"
            }).ToList();

            return new ProjectListResult
            {
                Items = items,
                TotalCount = total,
                PageNumber = Math.Max(1, req.PageNumber),
                PageSize = Math.Max(1, req.PageSize)
            };
        }


    }
}
