using Fusion.Repository.Bases.Exceptions;
using Fusion.Repository.Bases.Page;
using Fusion.Repository.Bases.Page.TaskLogEvent;
using Fusion.Repository.Data;
using Fusion.Repository.Entities;
using Fusion.Service.ViewModels.TaskLogEventQuery;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace Fusion.Repository.Repositories
{
    public interface ITaskLogEventRepository
    {
        Task<TaskLogEvent?> GetByIdAsync(long id, CancellationToken ct = default);

        Task<PagedResult<TaskLogEvent>> GetPagedByTaskIdAsync(
            Guid taskId,
            Guid userId,
            TaskLogEventPagedSearchRequest request,
            CancellationToken ct = default);
        Task<PagedResult<TaskLogEvent>> GetPagedByProjectIdAsync(
            Guid projectId,
            Guid userId,
            TaskLogEventPagedSearchRequest request,
            CancellationToken ct = default);
        Task<bool> UpdateIsViewForTaskAsync(
            Guid taskId,
            bool isView,
            Guid userId,
            CancellationToken ct = default);
        Task<PagedResult<ProjectActivityVm>> GetPagedProjectActivitiesAsync(
      Guid projectId,
      Guid userId,
      TaskLogEventPagedSearchRequest request,
      CancellationToken ct = default);
        Task<ProjectActivityVm?> GetTaskLogVmByIdAsync(long id, Guid userId, CancellationToken ct = default);

        // ✅ NEW: paged logs của 1 task (tránh cycle)
        Task<PagedResult<ProjectActivityVm>> GetPagedTaskLogsVmByTaskIdAsync(
            Guid taskId, Guid userId, TaskLogEventPagedSearchRequest request, CancellationToken ct = default);

    }
    public sealed class TaskLogEventRepository : ITaskLogEventRepository
    {
        private readonly FusionDbContext _context;

        public TaskLogEventRepository(FusionDbContext context)
        {
            _context = context;
        }

        public async Task<TaskLogEvent?> GetByIdAsync(long id, CancellationToken ct = default)
        {
            return await _context.TaskLogEvents
                .AsNoTracking()
                .Include(x => x.Actor)
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, ct);
        }
        public async Task<PagedResult<ProjectActivityVm>> GetPagedProjectActivitiesAsync(
    Guid projectId,
    Guid userId,
    TaskLogEventPagedSearchRequest request,
    CancellationToken ct = default)
        {
            request = Normalize(request);

            var baseQuery =
                from l in _context.TaskLogEvents.AsNoTracking()
                join t in _context.ProjectTasks.AsNoTracking()
                    on l.TaskId equals (Guid?)t.Id
                join u in _context.Users.AsNoTracking()
                    on l.ActorId equals (Guid?)u.Id into uu
                from u in uu.DefaultIfEmpty()
                where t.ProjectId == projectId
                      && l.IsDeleted != true
                      && t.IsDeleted != true
                select new ProjectActivityVm
                {
                    Id = l.Id,
                    TaskId = l.TaskId ?? Guid.Empty,
                    Action = l.Action,
                    ActorId = l.ActorId,
                    ActorName = u != null ? u.UserName : null,
                    ActorEmail = u != null ? u.Email : null,
                    ChangedCols = l.ChangedCols,
                    OldRow = l.OldRow,
                    NewRow = l.NewRow,
                    Metadata = l.Metadata,
                    CreatedAt = l.CreatedAt,
                    IsView = l.IsView
                };

          

            baseQuery = ApplyFilters(baseQuery, request);
            baseQuery = ApplySort(baseQuery, request);

            var total = await baseQuery.CountAsync(ct);
            var items = await baseQuery
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync(ct);

            return new PagedResult<ProjectActivityVm>
            {
                Items = items,
                TotalCount = total,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
        }


        private static TaskLogEventPagedSearchRequest Normalize(TaskLogEventPagedSearchRequest request)
        {
            request.PageNumber = request.PageNumber <= 0 ? 1 : request.PageNumber;
            request.PageSize = request.PageSize <= 0 ? 10 : request.PageSize;

            if (string.IsNullOrWhiteSpace(request.SortColumn))
            {
                request.SortColumn = "CreatedAt";
                request.SortDescending = true;
            }

            return request;
        }

        // ✅ ApplyFilters cho DTO, hỗ trợ Action = "A,B,C"
        private static IQueryable<ProjectActivityVm> ApplyFilters(
            IQueryable<ProjectActivityVm> query,
            TaskLogEventPagedSearchRequest request)
        {
            // Action single hoặc csv
            if (!string.IsNullOrWhiteSpace(request.Action))
            {
                var raw = request.Action.Trim();
                if (raw.Contains(','))
                {
                    var set = raw.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                    query = query.Where(x => x.Action != null && set.Contains(x.Action));
                }
                else
                {
                    query = query.Where(x => x.Action == raw);
                }
            }

            if (request.ActorId.HasValue && request.ActorId.Value != Guid.Empty)
                query = query.Where(x => x.ActorId == request.ActorId.Value);

            if (!string.IsNullOrWhiteSpace(request.KeyWord))
            {
                var kw = request.KeyWord.Trim().ToLower();
                query = query.Where(x =>
                    (x.Action ?? "").ToLower().Contains(kw) ||
                    (x.ChangedCols ?? "").ToLower().Contains(kw) ||
                    (x.Metadata ?? "").ToLower().Contains(kw) ||
                    (x.ActorName ?? "").ToLower().Contains(kw) ||
                    (x.ActorEmail ?? "").ToLower().Contains(kw)
                );
            }

            if (request.DateRange != null)
            {
                if (request.DateRange.From.HasValue && request.DateRange.To.HasValue)
                {
                    var from = request.DateRange.From.Value.ToDateTime(TimeOnly.MinValue);
                    var to = request.DateRange.To.Value.ToDateTime(TimeOnly.MaxValue);
                    query = query.Where(x => x.CreatedAt >= from && x.CreatedAt <= to);
                }
                else if (request.DateRange.From.HasValue)
                {
                    var from = request.DateRange.From.Value.ToDateTime(TimeOnly.MinValue);
                    query = query.Where(x => x.CreatedAt >= from);
                }
                else if (request.DateRange.To.HasValue)
                {
                    var to = request.DateRange.To.Value.ToDateTime(TimeOnly.MaxValue);
                    query = query.Where(x => x.CreatedAt <= to);
                }
            }

            return query;
        }

        private static IQueryable<ProjectActivityVm> ApplySort(
            IQueryable<ProjectActivityVm> query,
            TaskLogEventPagedSearchRequest request)
        {
            var desc = request.SortDescending;

            // whitelist sort (đủ dùng cho timeline)
            return (request.SortColumn ?? "CreatedAt") switch
            {
                "CreatedAt" => desc ? query.OrderByDescending(x => x.CreatedAt) : query.OrderBy(x => x.CreatedAt),
                "Action" => desc ? query.OrderByDescending(x => x.Action) : query.OrderBy(x => x.Action),
                _ => desc ? query.OrderByDescending(x => x.CreatedAt) : query.OrderBy(x => x.CreatedAt),
            };
        }
        public async Task<PagedResult<TaskLogEvent>> GetPagedByProjectIdAsync(
       Guid projectId,
       Guid userId,
       TaskLogEventPagedSearchRequest request,
       CancellationToken ct = default)
        {

            // Join TaskLogEvents -> ProjectTasks để lọc theo ProjectId
            var query =
                from l in _context.TaskLogEvents.AsNoTracking()
                join t in _context.ProjectTasks.AsNoTracking() on l.TaskId equals t.Id
                where t.ProjectId == projectId
                      && !l.IsDeleted
                      && !t.IsDeleted
                select l;

            query = query.Include(l => l.Actor);

            // Friend chỉ xem public
           

            query = ApplyFilters(query, request);
            return await query.ToPagedResultAsync(Normalize(request), ct);
        }

        public async Task<PagedResult<TaskLogEvent>> GetPagedByTaskIdAsync(
            Guid taskId,
            Guid userId,
            TaskLogEventPagedSearchRequest request,
            CancellationToken ct = default)
        {
            // 1) User
            var user = await _context.Users.AsNoTracking()
                .SingleOrDefaultAsync(u => u.Id == userId, ct);

            if (user == null)
                throw CustomExceptionFactory.CreateNotFoundError("User not found");

            // 2) Task -> Project
            var taskInfo = await _context.ProjectTasks
                .AsNoTracking()
                .Where(t => t.Id == taskId)
                .Select(t => new { t.Id, t.ProjectId })
                .SingleOrDefaultAsync(ct);

            if (taskInfo == null)
                throw CustomExceptionFactory.CreateNotFoundError("Task not found");

            var project = await _context.Projects
                .AsNoTracking()
                .Where(p => p.Id == taskInfo.ProjectId)
                .Select(p => new { p.Id, p.CompanyId, p.CompanyRequestId })
                .SingleOrDefaultAsync(ct);

            if (project == null)
                throw CustomExceptionFactory.CreateNotFoundError("Project not found");

            // 3) Check project member
            var isProjectMember = await _context.ProjectMembers
                .AsNoTracking()
                .AnyAsync(pm =>
                    pm.ProjectId == project.Id &&
                    pm.UserId == userId , ct);

            // 4) Nếu không là project member -> check friend-access theo company membership giống CompanyActivityLog
            // (logic này giữ y hệt để “chuẩn chỉnh”, bạn có thể bỏ nếu chỉ muốn member mới xem)
            var myCompanyIds = await _context.CompanyMembers
                .AsNoTracking()
                .Where(cm => cm.UserId == userId && cm.CompanyId != null && cm.IsDeleted != true)
                .Select(cm => cm.CompanyId!.Value)
                .ToListAsync(ct);

            var isInOwningCompanies =
                (project.CompanyId != null && myCompanyIds.Contains(project.CompanyId.Value))
                || (project.CompanyRequestId != null && myCompanyIds.Contains(project.CompanyRequestId.Value));

            bool hasFriendAccess = false;
            if (!isProjectMember && !isInOwningCompanies && myCompanyIds.Count > 0)
            {
                const string Accepted = "Active"; // theo chuẩn bạn đang dùng
                                                  // Friend với company của project (CompanyId/CompanyRequestId)
                var projectCompanyIds = new List<Guid>();
                if (project.CompanyId != null) projectCompanyIds.Add(project.CompanyId.Value);
                if (project.CompanyRequestId != null) projectCompanyIds.Add(project.CompanyRequestId.Value);

                hasFriendAccess = await _context.CompanyFriendships
                    .AsNoTracking()
                    .AnyAsync(cf =>
                        cf.Status == Accepted &&
                        (
                            (cf.CompanyAId != null && projectCompanyIds.Contains(cf.CompanyAId.Value) &&
                             cf.CompanyBId != null && myCompanyIds.Contains(cf.CompanyBId.Value))
                            ||
                            (cf.CompanyBId != null && projectCompanyIds.Contains(cf.CompanyBId.Value) &&
                             cf.CompanyAId != null && myCompanyIds.Contains(cf.CompanyAId.Value))
                        ), ct);
            }

            if (!isProjectMember && !isInOwningCompanies && !hasFriendAccess)
                throw CustomExceptionFactory.CreateForbiddenError();

            // 5) Base query
            IQueryable<TaskLogEvent> query = _context.TaskLogEvents
      .AsNoTracking()
      .Where(l => l.TaskId == taskId && !l.IsDeleted);

            // friend chỉ xem public
            if (!isProjectMember && !isInOwningCompanies && hasFriendAccess)
                query = query.Where(l => l.IsView);

            // filters
            if (!string.IsNullOrWhiteSpace(request.Action))
            {
                var act = request.Action.Trim();
                query = query.Where(l => l.Action == act);
            }

            if (request.ActorId.HasValue && request.ActorId.Value != Guid.Empty)
                query = query.Where(l => l.ActorId == request.ActorId.Value);

            if (!string.IsNullOrWhiteSpace(request.KeyWord))
            {
                var kw = request.KeyWord.Trim().ToLower();
                query = query.Where(l =>
                    (l.Action ?? "").ToLower().Contains(kw) ||
                    (l.ChangedCols ?? "").ToLower().Contains(kw) ||
                    (l.Metadata ?? "").ToLower().Contains(kw) ||
                    (l.Actor != null &&
                        ((l.Actor.UserName ?? "").ToLower().Contains(kw) ||
                         (l.Actor.Email ?? "").ToLower().Contains(kw)))
                );
            }

            // include sau cũng được
            query = query.Include(l => l.Actor);

            // sort
            query = query.OrderByDescending(l => l.CreatedAt);


            if (request.DateRange != null)
            {
                if (request.DateRange.From.HasValue && request.DateRange.To.HasValue)
                {
                    var from = request.DateRange.From.Value.ToDateTime(TimeOnly.MinValue);
                    var to = request.DateRange.To.Value.ToDateTime(TimeOnly.MaxValue);
                    query = query.Where(l => l.CreatedAt >= from && l.CreatedAt <= to);
                }
                else if (request.DateRange.From.HasValue)
                {
                    var from = request.DateRange.From.Value.ToDateTime(TimeOnly.MinValue);
                    query = query.Where(l => l.CreatedAt >= from);
                }
                else if (request.DateRange.To.HasValue)
                {
                    var to = request.DateRange.To.Value.ToDateTime(TimeOnly.MaxValue);
                    query = query.Where(l => l.CreatedAt <= to);
                }
            }

            // 8) Sort mặc định

            // 9) Return
            return await query.ToPagedResultAsync(request, ct);
        }

        public async Task<bool> UpdateIsViewForTaskAsync(Guid taskId, bool isView, Guid userId, CancellationToken ct = default)
        {
            // Quyền: chỉ project member hoặc company member của project mới được đổi visibility
            var taskInfo = await _context.ProjectTasks
                .AsNoTracking()
                .Where(t => t.Id == taskId)
                .Select(t => new { t.Id, t.ProjectId })
                .SingleOrDefaultAsync(ct);

            if (taskInfo == null)
                throw CustomExceptionFactory.CreateNotFoundError("Task not found");

            var project = await _context.Projects
                .AsNoTracking()
                .Where(p => p.Id == taskInfo.ProjectId)
                .Select(p => new { p.Id, p.CompanyId, p.CompanyRequestId })
                .SingleOrDefaultAsync(ct);

            if (project == null)
                throw CustomExceptionFactory.CreateNotFoundError("Project not found");

            var isProjectMember = await _context.ProjectMembers
                .AsNoTracking()
                .AnyAsync(pm =>
                    pm.ProjectId == project.Id &&
                    pm.UserId == userId , ct);

            var myCompanyIds = await _context.CompanyMembers
                .AsNoTracking()
                .Where(cm => cm.UserId == userId && cm.CompanyId != null && cm.IsDeleted != true)
                .Select(cm => cm.CompanyId!.Value)
                .ToListAsync(ct);

            var isInOwningCompanies =
                (project.CompanyId != null && myCompanyIds.Contains(project.CompanyId.Value))
                || (project.CompanyRequestId != null && myCompanyIds.Contains(project.CompanyRequestId.Value));

            if (!isProjectMember && !isInOwningCompanies)
                throw CustomExceptionFactory.CreateForbiddenError();

            await _context.TaskLogEvents
                .Where(l => l.TaskId == taskId && !l.IsDeleted && l.IsView != isView)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(l => l.IsView, _ => isView)
                    .SetProperty(l => l.UpdatedAt, _ => DateTimeOffset.UtcNow),
                    ct);

            return true;
        }
        // =========================
        // Helpers
        // =========================

        private sealed record ProjectAccess(bool IsProjectMember, bool IsInOwningCompanies, bool HasFriendAccess)
        {
            public bool IsFriendOnly => !IsProjectMember && !IsInOwningCompanies && HasFriendAccess;
            public bool CanManageVisibility => IsProjectMember || IsInOwningCompanies;
        }

        private async Task<ProjectAccess> CheckProjectAccessAsync(Guid projectId, Guid userId, CancellationToken ct)
        {
            var project = await _context.Projects
                .AsNoTracking()
                .Where(p => p.Id == projectId)
                .Select(p => new { p.Id, p.CompanyId, p.CompanyRequestId })
                .SingleOrDefaultAsync(ct);

            if (project == null)
                throw CustomExceptionFactory.CreateNotFoundError("Project not found");

            var isProjectMember = await _context.ProjectMembers
                .AsNoTracking()
                .AnyAsync(pm => pm.ProjectId == project.Id && pm.UserId == userId, ct);

            var myCompanyIds = await _context.CompanyMembers
                .AsNoTracking()
                .Where(cm => cm.UserId == userId && cm.CompanyId != null && cm.IsDeleted != true)
                .Select(cm => cm.CompanyId!.Value)
                .ToListAsync(ct);

            var isInOwningCompanies =
                (project.CompanyId != null && myCompanyIds.Contains(project.CompanyId.Value))
                || (project.CompanyRequestId != null && myCompanyIds.Contains(project.CompanyRequestId.Value));

            bool hasFriendAccess = false;
            if (!isProjectMember && !isInOwningCompanies && myCompanyIds.Count > 0)
            {
                const string Accepted = "Active";

                var projectCompanyIds = new List<Guid>();
                if (project.CompanyId != null) projectCompanyIds.Add(project.CompanyId.Value);
                if (project.CompanyRequestId != null) projectCompanyIds.Add(project.CompanyRequestId.Value);

                hasFriendAccess = await _context.CompanyFriendships
                    .AsNoTracking()
                    .AnyAsync(cf =>
                        cf.Status == Accepted &&
                        (
                            (cf.CompanyAId != null && projectCompanyIds.Contains(cf.CompanyAId.Value)
                             && cf.CompanyBId != null && myCompanyIds.Contains(cf.CompanyBId.Value))
                            ||
                            (cf.CompanyBId != null && projectCompanyIds.Contains(cf.CompanyBId.Value)
                             && cf.CompanyAId != null && myCompanyIds.Contains(cf.CompanyAId.Value))
                        ), ct);
            }

            if (!isProjectMember && !isInOwningCompanies && !hasFriendAccess)
                throw CustomExceptionFactory.CreateForbiddenError();

            return new ProjectAccess(isProjectMember, isInOwningCompanies, hasFriendAccess);
        }

      

        private static IQueryable<TaskLogEvent> ApplyFilters(IQueryable<TaskLogEvent> query, TaskLogEventPagedSearchRequest request)
        {
            if (!string.IsNullOrWhiteSpace(request.Action))
            {
                var act = request.Action.Trim();
                query = query.Where(l => l.Action == act);
            }

            if (request.ActorId.HasValue && request.ActorId.Value != Guid.Empty)
                query = query.Where(l => l.ActorId == request.ActorId.Value);

            if (!string.IsNullOrWhiteSpace(request.KeyWord))
            {
                var kw = request.KeyWord.Trim().ToLower();
                query = query.Where(l =>
                    (l.Action ?? "").ToLower().Contains(kw) ||
                    (l.ChangedCols ?? "").ToLower().Contains(kw) ||
                    (l.Metadata ?? "").ToLower().Contains(kw) ||
                    (l.Actor != null &&
                        ((l.Actor.UserName ?? "").ToLower().Contains(kw) ||
                         (l.Actor.Email ?? "").ToLower().Contains(kw)))
                );
            }

            if (request.DateRange != null)
            {
                if (request.DateRange.From.HasValue && request.DateRange.To.HasValue)
                {
                    var from = request.DateRange.From.Value.ToDateTime(TimeOnly.MinValue);
                    var to = request.DateRange.To.Value.ToDateTime(TimeOnly.MaxValue);
                    query = query.Where(l => l.CreatedAt >= from && l.CreatedAt <= to);
                }
                else if (request.DateRange.From.HasValue)
                {
                    var from = request.DateRange.From.Value.ToDateTime(TimeOnly.MinValue);
                    query = query.Where(l => l.CreatedAt >= from);
                }
                else if (request.DateRange.To.HasValue)
                {
                    var to = request.DateRange.To.Value.ToDateTime(TimeOnly.MaxValue);
                    query = query.Where(l => l.CreatedAt <= to);
                }
            }

            return query;
        }
        public async Task<ProjectActivityVm?> GetTaskLogVmByIdAsync(long id, Guid userId, CancellationToken ct = default)
        {
            // Nếu muốn chặt chẽ hơn: có thể check quyền theo taskId của log giống hàm paged (bên dưới).
            // Ở đây tối thiểu: trả về VM để tránh cycle.
            var q =
                from l in _context.TaskLogEvents.AsNoTracking()
                join u in _context.Users.AsNoTracking()
                    on l.ActorId equals (Guid?)u.Id into uu
                from u in uu.DefaultIfEmpty()
                where l.Id == id && l.IsDeleted != true
                select new ProjectActivityVm
                {
                    Id = l.Id,
                    TaskId = l.TaskId ?? Guid.Empty,
                    Action = l.Action,
                    ActorId = l.ActorId,
                    ActorName = u != null ? u.UserName : null,
                    ActorEmail = u != null ? u.Email : null,
                    ChangedCols = l.ChangedCols,
                    OldRow = l.OldRow,
                    NewRow = l.NewRow,
                    Metadata = l.Metadata,
                    CreatedAt = l.CreatedAt,
                    IsView = l.IsView
                };

            return await q.FirstOrDefaultAsync(ct);
        }

        public async Task<PagedResult<ProjectActivityVm>> GetPagedTaskLogsVmByTaskIdAsync(
            Guid taskId,
            Guid userId,
            TaskLogEventPagedSearchRequest request,
            CancellationToken ct = default)
        {
            request = Normalize(request);

            // ====== ACCESS CHECK y như bạn đang làm ======
            var taskInfo = await _context.ProjectTasks
                .AsNoTracking()
                .Where(t => t.Id == taskId)
                .Select(t => new { t.Id, t.ProjectId })
                .SingleOrDefaultAsync(ct);

            if (taskInfo == null)
                throw CustomExceptionFactory.CreateNotFoundError("Task not found");

            var project = await _context.Projects
                .AsNoTracking()
                .Where(p => p.Id == taskInfo.ProjectId)
                .Select(p => new { p.Id, p.CompanyId, p.CompanyRequestId })
                .SingleOrDefaultAsync(ct);

            if (project == null)
                throw CustomExceptionFactory.CreateNotFoundError("Project not found");

            var isProjectMember = await _context.ProjectMembers
                .AsNoTracking()
                .AnyAsync(pm => pm.ProjectId == project.Id && pm.UserId == userId, ct);

            var myCompanyIds = await _context.CompanyMembers
                .AsNoTracking()
                .Where(cm => cm.UserId == userId && cm.CompanyId != null && cm.IsDeleted != true)
                .Select(cm => cm.CompanyId!.Value)
                .ToListAsync(ct);

            var isInOwningCompanies =
                (project.CompanyId != null && myCompanyIds.Contains(project.CompanyId.Value))
                || (project.CompanyRequestId != null && myCompanyIds.Contains(project.CompanyRequestId.Value));

            bool hasFriendAccess = false;
            if (!isProjectMember && !isInOwningCompanies && myCompanyIds.Count > 0)
            {
                const string Accepted = "Active";

                var projectCompanyIds = new List<Guid>();
                if (project.CompanyId != null) projectCompanyIds.Add(project.CompanyId.Value);
                if (project.CompanyRequestId != null) projectCompanyIds.Add(project.CompanyRequestId.Value);

                hasFriendAccess = await _context.CompanyFriendships
                    .AsNoTracking()
                    .AnyAsync(cf =>
                        cf.Status == Accepted &&
                        (
                            (cf.CompanyAId != null && projectCompanyIds.Contains(cf.CompanyAId.Value) &&
                             cf.CompanyBId != null && myCompanyIds.Contains(cf.CompanyBId.Value))
                            ||
                            (cf.CompanyBId != null && projectCompanyIds.Contains(cf.CompanyBId.Value) &&
                             cf.CompanyAId != null && myCompanyIds.Contains(cf.CompanyAId.Value))
                        ), ct);
            }

            if (!isProjectMember && !isInOwningCompanies && !hasFriendAccess)
                throw CustomExceptionFactory.CreateForbiddenError();

            var friendOnly = !isProjectMember && !isInOwningCompanies && hasFriendAccess;

            // ====== BASE QUERY (VM projection => không cycle) ======
            IQueryable<ProjectActivityVm> baseQuery =
                from l in _context.TaskLogEvents.AsNoTracking()
                join u in _context.Users.AsNoTracking()
                    on l.ActorId equals (Guid?)u.Id into uu
                from u in uu.DefaultIfEmpty()
                where l.TaskId == taskId
                      && l.IsDeleted != true
                select new ProjectActivityVm
                {
                    Id = l.Id,
                    TaskId = l.TaskId ?? Guid.Empty,
                    Action = l.Action,
                    ActorId = l.ActorId,
                    ActorName = u != null ? u.UserName : null,
                    ActorEmail = u != null ? u.Email : null,
                    ChangedCols = l.ChangedCols,
                    OldRow = l.OldRow,
                    NewRow = l.NewRow,
                    Metadata = l.Metadata,
                    CreatedAt = l.CreatedAt,
                    IsView = l.IsView
                };

            if (friendOnly)
                baseQuery = baseQuery.Where(x => x.IsView == true);

            // ✅ reuse filter/sort VM (đã support Action CSV)
            baseQuery = ApplyFilters(baseQuery, request);
            baseQuery = ApplySort(baseQuery, request);

            var total = await baseQuery.CountAsync(ct);
            var items = await baseQuery
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync(ct);

            return new PagedResult<ProjectActivityVm>
            {
                Items = items,
                TotalCount = total,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
        }

    }
}
