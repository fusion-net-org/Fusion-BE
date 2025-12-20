
using Fusion.Repository.Bases.Exceptions;
using Fusion.Repository.Bases.Page;
using Fusion.Repository.Bases.Page.Project;
using Fusion.Repository.Data;
using Fusion.Repository.Entities;
using Fusion.Repository.Enums;
using Fusion.Repository.IRepositories;
using Fusion.Repository.ViewModels;
using Fusion.Repository.ViewModels.Project;
using Microsoft.EntityFrameworkCore;

namespace Fusion.Repository.Repositories
{

    public class ProjectRepository : GenericRepository<Project>, IProjectRepository
    {
        private readonly FusionDbContext _ctx;
        public ProjectRepository(FusionDbContext ctx) : base(ctx) => _ctx = ctx;

        public Task<bool> IsCodeExistedAsync(Guid companyId, string code, CancellationToken ct = default)
            => _ctx.Projects.AnyAsync(p => p.CompanyId == companyId && p.Code == code, ct);

        public Task<Project?> GetByIdWithSprintsAsync(Guid projectId, CancellationToken ct = default)
            => _ctx.Projects
                    .Include(p => p.Sprints)
                    .FirstOrDefaultAsync(p => p.Id == projectId, ct);
        public Task<int> GetAllProjectCountAsync(CancellationToken cancellationToken = default)
        {
            return _context.Projects
                .AsNoTracking()
                .CountAsync(cancellationToken);
        }

        public async Task<(List<Project> Items, int TotalCount)> GetProjectsForCompanyAsync(
       Guid companyId,
       Guid userId,
       string? q,
       IEnumerable<string>? statuses,
       string? sort,
       int pageNumber,
       int pageSize,
       CancellationToken ct = default)
        {
            var query = _ctx.Projects
                .AsNoTracking()
                .Include(p => p.Company)
                .Include(p => p.CompanyRequest)
                .Include(p => p.Workflow)
                .Where(p =>
                    // Case A: company là owner/executor của project => phải là project member
                    (p.CompanyId == companyId && p.ProjectMembers.Any(pm => pm.UserId == userId))
                    // Case B: company là company request => xem tất cả, không cần project member
                    || (p.CompanyRequestId == companyId)
                );

            // Search
            if (!string.IsNullOrWhiteSpace(q))
            {
                var key = q.Trim().ToLower();
                query = query.Where(p =>
                    (p.Code ?? "").ToLower().Contains(key) ||
                    (p.Name ?? "").ToLower().Contains(key));
            }

            // Status filter
            if (statuses != null && statuses.Any())
            {
                var bag = statuses.Where(s => !string.IsNullOrWhiteSpace(s))
                                  .Select(s => s.Trim())
                                  .ToHashSet(StringComparer.OrdinalIgnoreCase);

                query = query.Where(p => p.Status != null && bag.Contains(p.Status));
            }

            // Sort
            query = (sort ?? "recent").ToLower() switch
            {
                "name" => query.OrderBy(p => p.Name).ThenByDescending(p => p.CreateAt),
                "start" => query.OrderBy(p => p.StartDate).ThenBy(p => p.Name),
                _ => query.OrderByDescending(p => p.UpdateAt).ThenByDescending(p => p.CreateAt)
            };

            var total = await query.CountAsync(ct);

            var page = Math.Max(1, pageNumber);
            var size = Math.Max(1, pageSize);

            var items = await query
                .Skip((page - 1) * size)
                .Take(size)
                .ToListAsync(ct);

            return (items, total);
        }

        public async Task<PagedResult<Project>> GetAllProjectAsync(ProjectSearchRequest req, CancellationToken ct = default)
        {
            req ??= new ProjectSearchRequest();
            if (string.IsNullOrWhiteSpace(req.SortColumn))
            {
                req.SortColumn = nameof(Project.CreateAt);
                req.SortDescending = true;
            }

            var q = _context.Projects.AsNoTracking();

            // Filter
            if (!string.IsNullOrWhiteSpace(req.Keyword))
            {
                var kw = req.Keyword.Trim().ToLower();
                q = q.Where(p =>
                    ((p.Name ?? "").ToLower().Contains(kw)) ||
                    ((p.Code ?? "").ToLower().Contains(kw)) ||
                    ((p.Description ?? "").ToLower().Contains(kw)));
            }

            if (req.CompanyId.HasValue)
                q = q.Where(p => p.CompanyId == req.CompanyId.Value);

            if (!string.IsNullOrWhiteSpace(req.Status))
            {
                q = q.Where(p => (p.Status ?? "").Trim().ToLower() == req.Status);
            }

            // Date range on CreateAt (DateOnly -> UTC range)
            if (req.DateRange is { } dr)
            {
                if (dr.From.HasValue)
                {
                    var from = DateTime.SpecifyKind(
                        dr.From.Value.ToDateTime(TimeOnly.MinValue), DateTimeKind.Utc);
                    q = q.Where(p => p.CreateAt >= from);
                }

                if (dr.To.HasValue)
                {
                    var toExclusive = DateTime.SpecifyKind(
                        dr.To.Value.ToDateTime(TimeOnly.MinValue), DateTimeKind.Utc).AddDays(1);
                    q = q.Where(p => p.CreateAt < toExclusive);
                }
            }

            return await q.ToPagedResultAsync(req, ct);

        }
        public async Task<List<StatusCountResponse>> GetCountProjectByStatusAsync(CancellationToken ct = default)
        {
            var rows = await _context.Projects
                        .AsNoTracking()
                        .GroupBy(p => (p.Status ?? "").Trim().ToLower())
                        .Select(g => new StatusCountResponse
                        {
                            Status = string.IsNullOrWhiteSpace(g.Key) ? "(none)" : g.Key,
                            Count = g.Count()
                        })
                         .OrderByDescending(x => x.Count)
                         .ToListAsync(ct);

            return rows;
        }
        public async Task<PagedResult<Project>> GetProjectByMemberIdAsync(Guid userId, ProjectSearchRequest req, CancellationToken ct = default)
        {
            req ??= new ProjectSearchRequest();
            if (string.IsNullOrWhiteSpace(req.SortColumn))
            {
                req.SortColumn = nameof(Project.CreateAt);
                req.SortDescending = true;
            }

            var q = _context.Projects
                .AsNoTracking()
                .Where(x => x.ProjectMembers.Any(m => m.UserId == userId));

            if (!string.IsNullOrWhiteSpace(req.Keyword))
            {
                var kw = req.Keyword.Trim().ToLower();
                q = q.Where(p =>
                ((p.Name ?? "").ToLower().Contains(kw)) ||
                ((p.Code ?? "").ToLower().Contains(kw)) ||
                ((p.Description ?? "").ToLower().Contains(kw))
                );
            }

            if (req.CompanyId.HasValue)
                q = q.Where(p => p.CompanyId == req.CompanyId.Value);

            if (!string.IsNullOrWhiteSpace(req.Status))
            {
                q = q.Where(p => (p.Status ?? "").Trim().ToLower() == req.Status);
            }

            if (req.DateRange is { } dr)
            {
                if (dr.From.HasValue)
                {
                    var from = DateTime.SpecifyKind(
                        dr.From.Value.ToDateTime(TimeOnly.MinValue), DateTimeKind.Utc);
                    q = q.Where(p => p.CreateAt >= from);
                }

                if (dr.To.HasValue)
                {
                    var toExclusive = DateTime.SpecifyKind(
                        dr.To.Value.ToDateTime(TimeOnly.MinValue), DateTimeKind.Utc).AddDays(1);
                    q = q.Where(p => p.CreateAt < toExclusive);
                }
            }
            return await q.ToPagedResultAsync(req, ct);
        }
        public async Task<PagedResult<Project>> GetProjectByActorIdAsync(Guid userId, ProjectSearchRequest req, CancellationToken ct = default)
        {
            req ??= new ProjectSearchRequest();
            if (string.IsNullOrWhiteSpace(req.SortColumn))
            {
                req.SortColumn = nameof(Project.CreateAt);
                req.SortDescending = true;
            }

            var q = _context.Projects
                .AsNoTracking()
                .Where(x => x.CreatedBy == userId);

            if (!string.IsNullOrWhiteSpace(req.Keyword))
            {
                var kw = req.Keyword.Trim().ToLower();
                q = q.Where(p =>
                ((p.Name ?? "").ToLower().Contains(kw)) ||
                ((p.Code ?? "").ToLower().Contains(kw)) ||
                ((p.Description ?? "").ToLower().Contains(kw))
                );
            }

            if (req.CompanyId.HasValue)
                q = q.Where(p => p.CompanyId == req.CompanyId.Value);

            if (!string.IsNullOrWhiteSpace(req.Status))
            {
                q = q.Where(p => (p.Status ?? "").Trim().ToLower() == req.Status);
            }

            if (req.DateRange is { } dr)
            {
                if (dr.From.HasValue)
                {
                    var from = DateTime.SpecifyKind(
                        dr.From.Value.ToDateTime(TimeOnly.MinValue), DateTimeKind.Utc);
                    q = q.Where(p => p.CreateAt >= from);
                }

                if (dr.To.HasValue)
                {
                    var toExclusive = DateTime.SpecifyKind(
                        dr.To.Value.ToDateTime(TimeOnly.MinValue), DateTimeKind.Utc).AddDays(1);
                    q = q.Where(p => p.CreateAt < toExclusive);
                }
            }
            return await q.ToPagedResultAsync(req, ct);
        }
        public Task<Project?> GetProjectDetailAsync(Guid id, CancellationToken ct = default)
        {
            return _context.Projects
                .AsNoTracking()
                .Where(p => p.Id == id)
                .Include(p => p.Sprints.Where(s => !s.IsDeleted))
                .ThenInclude(s => s.ProjectTasks.Where(s => !s.IsDeleted))
                .Include(p => p.ProjectTasks.Where(s => !s.IsDeleted))
                .Include(c => c.Company.Name)
                .FirstOrDefaultAsync(ct);
        }

        public async Task<PagedResult<Project>> GetProjectsForAdminAsync(ProjectSummarySearchRequest request, CancellationToken cancellationToken = default)
        {
            var query = _context.Projects
                .Include(p => p.CreatedByNavigation)
                .Include(p => p.Company)
                .Include(p => p.CompanyRequest)
                .Include(p => p.Workflow)
                .Include(p => p.ProjectMembers).ThenInclude(x => x.User)
                .Include(p => p.Sprints).ThenInclude(s => s.ProjectTasks)
                .AsQueryable();

            if (request.CompanyId.HasValue)
            {
                query = query.Where(p =>
                        (p.Company != null && p.Company.Id == request.CompanyId) ||
                        (p.CompanyRequest != null && p.CompanyRequest.Id == request.CompanyId));
            }

            // FILTER: companyName
            if (!string.IsNullOrEmpty(request.CompanyName))
            {
                query = query.Where(p =>
                        (p.Company != null && p.Company.Name.Contains(request.CompanyName)) ||
                        (p.CompanyRequest != null && p.CompanyRequest.Name.Contains(request.CompanyName)));
            }

            if (!string.IsNullOrEmpty(request.ProjectName))
            {
                query = query.Where(p => (p.Name ?? "").Contains(request.ProjectName));
            }

            return await query.ToPagedResultAsync(request, cancellationToken);
        }

        public async Task<PagedResult<Project>> GetProjectsByUserIdAsync(ProjectSummarySearchRequest request, Guid userId, CancellationToken cancellationToken = default)
        {
            var query = _context.Projects
                .Include(p => p.CreatedByNavigation)
                .Include(p => p.Company)
                .Include(p => p.CompanyRequest)
                .Include(p => p.Workflow)
                .Include(p => p.ProjectMembers).ThenInclude(x => x.User)
                .Include(p => p.Sprints).ThenInclude(s => s.ProjectTasks)
                .Include(p => p.ProjectTasks)
                    .ThenInclude(p => p.TaskWorkflows)
                        .ThenInclude(p => p.AssignUser)
                .Where(p =>
                    p.ProjectTasks.Any(t =>
                        t.TaskWorkflows.Any(tw => tw.AssignUserId == userId)
                    ))
                .AsQueryable();

            // FILTER: companyName
            if (!string.IsNullOrEmpty(request.CompanyName))
            {
                query = query.Where(p =>
                        (p.Company != null && p.Company.Name.Contains(request.CompanyName)) ||
                        (p.CompanyRequest != null && p.CompanyRequest.Name.Contains(request.CompanyName)));
            }

            return await query.ToPagedResultAsync(request, cancellationToken);
        }

        public async Task<Project?> GetProjectsByIdForAdminAsync(Guid projectId, CancellationToken cancellationToken = default)
        {
            var query = await _context.Projects
                .Include(p => p.CreatedByNavigation)
                .Include(p => p.Company)
                .Include(p => p.CompanyRequest)
                .Include(p => p.Workflow)
                .Include(p => p.ProjectMembers).ThenInclude(x => x.User)
                .Include(p => p.Sprints).ThenInclude(s => s.ProjectTasks)
                .SingleOrDefaultAsync(x => x.Id == projectId);

            return query;
        }

        public async Task<Project> GetProjectById(Guid projectId, CancellationToken cancellationToken = default)
        {
            var query = await _context.Projects
                        .Include(p => p.Company)
                        .Include(p => p.CompanyRequest)
                        .Include(p => p.CreatedByNavigation)
                        .SingleOrDefaultAsync(x => x.Id == projectId);
            return query;
        }

        public async Task<int> GetTotalProjectsAsync(CancellationToken token)
        {
            return await _context.Projects.CountAsync(token);
        }

        // =================== Over view =====================
        public async Task<IReadOnlyList<ProjectMonthlyStat>> GetProjectMonthlyCreationAndCompletionAsync(
           Guid? companyId,
           DateTime? from,
           DateTime? to,
           CancellationToken ct = default)
        {
            // ===== New projects: group theo CreateAt =====
            var newQuery = _ctx.Projects
                .AsNoTracking()
                .AsQueryable();

            if (companyId.HasValue)
            {
                newQuery = newQuery.Where(p => p.CompanyId == companyId.Value);
            }

            if (from.HasValue)
            {
                newQuery = newQuery.Where(p => p.CreateAt >= from.Value);
            }

            if (to.HasValue)
            {
                newQuery = newQuery.Where(p => p.CreateAt <= to.Value);
            }

            var newPerMonth = await newQuery
                .GroupBy(p => new { p.CreateAt.Year, p.CreateAt.Month })
                .Select(g => new
                {
                    g.Key.Year,
                    g.Key.Month,
                    Count = g.Count()
                })
                .ToListAsync(ct);

            // ===== Completed projects: chỉ cần có EndDate =====
            var completedQuery = _ctx.Projects
                .AsNoTracking()
                .Where(p => p.EndDate != null);

            if (companyId.HasValue)
            {
                completedQuery = completedQuery.Where(p => p.CompanyId == companyId.Value);
            }

            if (from.HasValue)
            {
                var fromDateOnly = DateOnly.FromDateTime(from.Value.Date);
                completedQuery = completedQuery.Where(p => p.EndDate >= fromDateOnly);
            }

            if (to.HasValue)
            {
                var toDateOnly = DateOnly.FromDateTime(to.Value.Date);
                completedQuery = completedQuery.Where(p => p.EndDate <= toDateOnly);
            }

            var completedPerMonth = await completedQuery
                .GroupBy(p => new { Year = p.EndDate!.Value.Year, Month = p.EndDate!.Value.Month })
                .Select(g => new
                {
                    g.Key.Year,
                    g.Key.Month,
                    Count = g.Count()
                })
                .ToListAsync(ct);

            // ===== Merge 2 series theo Year/Month =====
            var allKeys = newPerMonth
                .Select(x => new { x.Year, x.Month })
                .Concat(completedPerMonth.Select(x => new { x.Year, x.Month }))
                .Distinct()
                .OrderBy(x => x.Year)
                .ThenBy(x => x.Month)
                .ToList();

            var result = new List<ProjectMonthlyStat>();

            foreach (var key in allKeys)
            {
                var newCount = newPerMonth
                    .FirstOrDefault(x => x.Year == key.Year && x.Month == key.Month)?.Count ?? 0;

                var completedCount = completedPerMonth
                    .FirstOrDefault(x => x.Year == key.Year && x.Month == key.Month)?.Count ?? 0;

                result.Add(new ProjectMonthlyStat
                {
                    Year = key.Year,
                    Month = key.Month,
                    NewProjects = newCount,
                    CompletedProjects = completedCount
                });
            }

            return result;
        }

        public async Task<ProjectExecutionOverviewResponse> GetProjectExecutionOverviewAsync(
             Guid? companyId,
             DateTime? fromUtc,
             DateTime? toUtc,
             CancellationToken ct = default)
        {
            var nowUtc = DateTime.UtcNow;

            var to = toUtc ?? nowUtc;
            var from = fromUtc ?? to.AddMonths(-11); // last 12 months

            if (from > to)
            {
                var tmp = from;
                from = to;
                to = tmp;
            }

            // base queries
            var taskQuery = _ctx.ProjectTasks
                .AsNoTracking()
                .Include(t => t.CurrentStatus)
                .Include(t => t.Project)
                .Where(t => !t.IsDeleted);

            var sprintQuery = _ctx.Sprints
                .AsNoTracking()
                .Include(s => s.Project)
                .Include(s => s.ProjectTasks)
                .Where(s => !s.IsDeleted);

            if (companyId.HasValue)
            {
                var cid = companyId.Value;
                taskQuery = taskQuery.Where(t => t.Project != null && t.Project.CompanyId == cid);
                sprintQuery = sprintQuery.Where(s => s.Project != null && s.Project.CompanyId == cid);
            }

            /* =========================================================
             * 1. Task-level stats
             *  - Completed task = CurrentStatus.IsEnd == true
             *  - Overdue task   = past due_date && NOT IsEnd
             * =======================================================*/
            var totalTasks = await taskQuery.CountAsync(ct);

            var completedTasks = await taskQuery
                .CountAsync(t => t.CurrentStatus != null && t.CurrentStatus.IsEnd, ct);

            var overdueTasks = await taskQuery
                .CountAsync(t =>
                    t.DueDate != null &&
                    t.DueDate < nowUtc &&
                    (t.CurrentStatus == null || !t.CurrentStatus.IsEnd),
                    ct);

            /* =========================================================
             * 2. Task flow by month (Created vs Completed)
             * =======================================================*/
            var createdByMonth = await taskQuery
                .Where(t => t.CreateAt != null &&
                            t.CreateAt >= from &&
                            t.CreateAt <= to)
                .GroupBy(t => new
                {
                    t.CreateAt!.Value.Year,
                    t.CreateAt!.Value.Month
                })
                .Select(g => new
                {
                    g.Key.Year,
                    g.Key.Month,
                    Count = g.Count()
                })
                .ToListAsync(ct);

            var completedByMonth = await taskQuery
                .Where(t =>
                    t.CurrentStatus != null &&
                    t.CurrentStatus.IsEnd &&          // <<< dùng IsEnd thay cho IsDone
                    t.UpdateAt != null &&
                    t.UpdateAt >= from &&
                    t.UpdateAt <= to)
                .GroupBy(t => new
                {
                    t.UpdateAt!.Value.Year,
                    t.UpdateAt!.Value.Month
                })
                .Select(g => new
                {
                    g.Key.Year,
                    g.Key.Month,
                    Count = g.Count()
                })
                .ToListAsync(ct);

            var createdDict = createdByMonth
                .ToDictionary(x => (x.Year, x.Month), x => x.Count);

            var completedDict = completedByMonth
                .ToDictionary(x => (x.Year, x.Month), x => x.Count);

            var taskFlow = new List<TaskFlowPointResponse>();

            var currentMonth = new DateTime(from.Year, from.Month, 1);
            var lastMonth = new DateTime(to.Year, to.Month, 1);

            while (currentMonth <= lastMonth)
            {
                var key = (currentMonth.Year, currentMonth.Month);
                createdDict.TryGetValue(key, out var createdCount);
                completedDict.TryGetValue(key, out var completedCount);

                taskFlow.Add(new TaskFlowPointResponse
                {
                    Year = currentMonth.Year,
                    Month = currentMonth.Month,
                    CreatedTasks = createdCount,
                    CompletedTasks = completedCount
                });

                currentMonth = currentMonth.AddMonths(1);
            }

            /* =========================================================
             * 3. Sprint-level stats
             * =======================================================*/
            var totalSprints = await sprintQuery.CountAsync(ct);

            // NOTE: tuỳ enum SprintStatus của bạn,
            // giữ nguyên nếu đã có Active / Completed.
            var activeSprints = await sprintQuery
                .CountAsync(s => s.Status == SprintStatus.Active, ct);

            var completedSprints = await sprintQuery
                .CountAsync(s => s.Status == SprintStatus.Completed, ct);

            /* =========================================================
             * 4. Sprint velocity
             *    - CompletedPoints = sum(Point) của tasks với CurrentStatus.IsEnd == true
             * =======================================================*/
            var sprintVelocityRaw = await sprintQuery
                .OrderBy(s => s.StartDate)
                .Select(s => new
                {
                    s.Id,
                    s.Name,
                    s.StartDate,
                    s.EndDate,
                    Committed = s.CommittedPoints ?? 0,
                    Completed = s.ProjectTasks
                        .Where(t => !t.IsDeleted &&
                                    t.CurrentStatus != null &&
                                    t.CurrentStatus.IsEnd)  // <<< dùng IsEnd
                        .Sum(t => (int?)t.Point ?? 0)
                })
                .ToListAsync(ct);

            var sprintVelocity = sprintVelocityRaw
                .Select(x => new SprintVelocityPointResponse
                {
                    SprintId = x.Id,
                    SprintName = x.Name,
                    StartDate = x.StartDate,
                    EndDate = x.EndDate,
                    CommittedPoints = x.Committed,
                    CompletedPoints = x.Completed
                })
                .ToList();

            return new ProjectExecutionOverviewResponse
            {
                TotalTasks = totalTasks,
                CompletedTasks = completedTasks,
                OverdueTasks = overdueTasks,

                TotalSprints = totalSprints,
                ActiveSprints = activeSprints,
                CompletedSprints = completedSprints,

                TaskFlow = taskFlow,
                SprintVelocity = sprintVelocity
            };
        }



        public async Task<List<Project>> GetProjectsByCompanyAsync(
         Guid companyId,
         Guid? companyRequestId,
         Guid? executorCompanyId,
         CancellationToken cancellationToken = default)
        {
            var companyExists = await _ctx.Companies
                .AnyAsync(c => c.Id == companyId && (c.IsDeleted ?? false) == false, cancellationToken);

            if (!companyExists)
                throw new Exception("Company not found");

            var query = _dbSet
                .Include(x => x.Company)             // Executor
                .Include(x => x.CompanyRequest)      // Requester
                .Include(x => x.Workflow)
                .AsQueryable();

            if (companyRequestId.HasValue)
                query = query.Where(x => x.CompanyRequestId == companyRequestId.Value);

            if (executorCompanyId.HasValue)
                query = query.Where(x => x.CompanyId == executorCompanyId.Value);

            return await query.ToListAsync(cancellationToken);
        }
        public async Task<List<Project>> GetProjectsByCompanyRequestAsync(
        Guid companyId,
        CancellationToken cancellationToken = default)
        {
            var companyExists = await _ctx.Companies
                .AnyAsync(c => c.Id == companyId && (c.IsDeleted ?? false) == false, cancellationToken);

            if (!companyExists)
                throw new Exception("Company not found");

            var query = _dbSet
                .Include(x => x.Company)            // Executor company
                .Include(x => x.CompanyRequest)     // Request company
                .Include(x => x.Workflow)
                .Where(x => x.CompanyRequestId == companyId && x.IsHired == true);

            return await query.ToListAsync(cancellationToken);
        }
        public async Task<bool> CloseFromProjectAsync(Guid projectId, Guid actorUserId, CancellationToken ct = default)
        {
            using var tx = await _context.Database.BeginTransactionAsync(ct);

            var project = await _context.Projects
                .Include(p => p.ProjectRequest)
                .FirstOrDefaultAsync(p => p.Id == projectId, ct);

            if (project == null)
                throw CustomExceptionFactory.CreateNotFoundError("Project not found");
            
            if (!project.CreatedBy.HasValue || project.CreatedBy.Value != actorUserId)
                throw CustomExceptionFactory.CreateForbiddenError();

            project.IsClosed = true;
            project.ClosedBy = actorUserId;
            project.UpdateAt = DateTime.UtcNow;

            var pr = project.ProjectRequest;
            if (pr != null)
            {
                if (pr.IsDeleted == true)
                    throw CustomExceptionFactory.CreateBadRequestError("Linked project request is deleted");

                pr.IsClosed = true;
                pr.ClosedBy = actorUserId;
                pr.UpdatedBy = actorUserId;
                pr.UpdateAt = DateTime.UtcNow.AddHours(7);
            }

            await _context.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);
            return true;
        }

        public async Task<bool> ReopenFromProjectAsync(Guid projectId, Guid actorUserId, CancellationToken ct = default)
        {
            using var tx = await _context.Database.BeginTransactionAsync(ct);

            var project = await _context.Projects
                .Include(p => p.ProjectRequest)
                .FirstOrDefaultAsync(p => p.Id == projectId, ct);

            if (project == null)
                throw CustomExceptionFactory.CreateNotFoundError("Project not found");

            // quyền: chỉ người tạo project
            if (!project.CreatedBy.HasValue || project.CreatedBy.Value != actorUserId)
                throw CustomExceptionFactory.CreateForbiddenError();

            // mở lại project
            project.IsClosed = false;
            project.ClosedBy = null;
            project.UpdateAt = DateTime.UtcNow;

            // cascade mở lại project request (nếu có)
            var pr = project.ProjectRequest;
            if (pr != null)
            {
                if (pr.IsDeleted == true)
                    throw CustomExceptionFactory.CreateBadRequestError("Linked project request is deleted");

                pr.IsClosed = false;
                pr.ClosedBy = null;
                pr.UpdatedBy = actorUserId;
                pr.UpdateAt = DateTime.UtcNow.AddHours(7);
            }

            await _context.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);
            return true;
        }
        public async Task<ProjectTaskProgressVm> GetTaskProgressAsync(Guid projectId, CancellationToken ct = default)
        {
            var row = await (
                from t in _ctx.ProjectTasks.AsNoTracking()
                join s in _ctx.WorkflowStatuses.AsNoTracking()
                    on t.CurrentStatusId equals s.Id into sj
                from s in sj.DefaultIfEmpty()
                where t.ProjectId == projectId && !t.IsDeleted
                group new { t, s } by 1 into g
                select new
                {
                    Total = g.Count(),
                    Done = g.Count(x => x.s != null && x.s.IsEnd)
                }
            ).FirstOrDefaultAsync(ct);

            if (row == null)
                return new ProjectTaskProgressVm { TotalTasks = 0, DoneTasks = 0 };

            return new ProjectTaskProgressVm
            {
                TotalTasks = row.Total,
                DoneTasks = row.Done
            };
        }
    }
}
