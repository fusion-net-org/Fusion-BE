using Fusion.Repository.Bases.Exceptions;
using Fusion.Repository.Data;
using Fusion.Repository.Entities;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace Fusion.Repository.Repositories
{
    public record WorkflowVm(string Id, string Name);
    public record StatusVm(
        string Id, string Name, bool IsStart, bool IsEnd,
        int X, int Y, List<string> Roles, string? Color
    );
    public record TransitionVm(
        long? Id,
        string FromStatusId, string ToStatusId,
        string Type, string? Label, string? Rule,
        List<string>? RoleNames
    );
    public record DesignerDto(WorkflowVm Workflow, List<StatusVm> Statuses, List<TransitionVm> Transitions);
    public record WorkflowListItemVm(string Id, string Name);
    public record StatusPreviewVm(string Id, string Name, bool IsStart, bool IsEnd, int X, int Y, string? Color, List<string> Roles);
    public record TransitionPreviewVm(string FromStatusId, string ToStatusId, string Type, string? Label);
    public record WorkflowPreviewVm(string Id, string Name, List<StatusPreviewVm> Statuses, List<TransitionPreviewVm> Transitions);

    public interface IWorkflowDesignerRepository
    {
        Task<List<WorkflowListItemVm>> GetAllAsync(Guid companyId, CancellationToken ct = default);
        Task<Guid> CreateAsync(Guid companyId, string name, CancellationToken ct = default);
        Task DeleteAsync(Guid companyId, Guid workflowId, CancellationToken ct = default);
        Task<DesignerDto> GetDesignerAsync(Guid workflowId, CancellationToken ct = default);
        Task SaveDesignerAsync(Guid companyId, Guid workflowId, DesignerDto payload, CancellationToken ct = default);
        Task<List<WorkflowPreviewVm>> GetPreviewsAsync(Guid companyId, CancellationToken ct = default);
        Task<List<WorkflowPreviewVm>> GetPreviewsAdminAsync(Guid adminId, CancellationToken ct = default);
        Task<bool> ExistsInCompanyAsync(Guid workflowId, Guid companyId, CancellationToken ct = default);

    }

    public sealed class WorkflowDesignerRepository : IWorkflowDesignerRepository
    {
        private readonly FusionDbContext _db;
        public WorkflowDesignerRepository(FusionDbContext db) => _db = db;

        public Task<List<WorkflowListItemVm>> GetAllAsync(Guid companyId, CancellationToken ct = default)
            => _db.Workflows.AsNoTracking()
                .Where(w => w.CompanyId == companyId)
                .OrderByDescending(w => w.IsDefault).ThenBy(w => w.Name)
                .Select(w => new WorkflowListItemVm(w.Id.ToString(), w.Name ?? ""))
                .ToListAsync(ct);
        public Task<bool> ExistsInCompanyAsync(Guid workflowId, Guid companyId, CancellationToken ct = default)
           => _db.Workflows.AnyAsync(w => w.Id == workflowId && w.CompanyId == companyId, ct);
        public async Task<Guid> CreateAsync(Guid companyId, string name, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Workflow name is required.");

            var wf = new Workflow
            {
                Id = Guid.NewGuid(),
                CompanyId = companyId,
                Name = name.Trim(),
                IsDefault = false
            };
            _db.Workflows.Add(wf);
            await _db.SaveChangesAsync(ct);
            return wf.Id;
        }

        public async Task DeleteAsync(Guid companyId, Guid workflowId, CancellationToken ct = default)
        {
            var wf = await _db.Workflows
                .Include(w => w.WorkflowStatuses)
                .SingleOrDefaultAsync(w => w.Id == workflowId, ct);

            if (wf == null || wf.CompanyId != companyId)
                throw new KeyNotFoundException("Workflow not found.");

            // block nếu đang được Project dùng
            var anyProjects = await _db.Projects.AsNoTracking()
                .AnyAsync(p => p.WorkflowId == workflowId, ct);
            if (anyProjects)
                throw new InvalidOperationException("Cannot delete workflow because it is referenced by Projects.");

            using var tx = await _db.Database.BeginTransactionAsync(ct);

            var statusIds = await _db.WorkflowStatuses.AsNoTracking()
                .Where(s => s.WorkflowId == workflowId)
                .Select(s => s.Id)
                .ToListAsync(ct);

            if (statusIds.Count > 0)
            {
                var usedByTickets = await _db.Tickets.AsNoTracking()
                    .AnyAsync(t => t.StatusId != null && statusIds.Contains(t.StatusId.Value), ct);
                if (usedByTickets)
                    throw new InvalidOperationException("Cannot delete workflow because some statuses are referenced by Tickets.");

                var usedByTaskWf = await _db.TaskWorkflows.AsNoTracking()
                    .AnyAsync(tw => tw.WorkflowStatusId != null && statusIds.Contains(tw.WorkflowStatusId.Value), ct);
                if (usedByTaskWf)
                    throw new InvalidOperationException("Cannot delete workflow because some statuses are referenced by TaskWorkflow.");

                var trans = await _db.WorkflowTransitions
                    .Where(tr => tr.WorkflowId == workflowId)
                    .ToListAsync(ct);
                if (trans.Count > 0) _db.WorkflowTransitions.RemoveRange(trans);

                var statuses = await _db.WorkflowStatuses
                    .Where(s => s.WorkflowId == workflowId)
                    .ToListAsync(ct);
                if (statuses.Count > 0) _db.WorkflowStatuses.RemoveRange(statuses);
            }

            _db.Workflows.Remove(wf);
            await _db.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);
        }

        public async Task<DesignerDto> GetDesignerAsync(Guid workflowId, CancellationToken ct = default)
        {
            var wf = await _db.Workflows
                .Include(w => w.WorkflowStatuses)
                .Include(w => w.WorkflowTransitions)
                .AsNoTracking()
                .SingleAsync(w => w.Id == workflowId, ct);

            var statuses = wf.WorkflowStatuses
                .OrderBy(s => s.Position)
                .Select(s => new StatusVm(
                    s.Id.ToString(),
                    s.Name ?? "",
                    s.IsStart,
                    s.IsEnd,
                    X: s.X == 0 ? 200 : s.X,     // fallback nếu data cũ
                    Y: s.Y == 0 ? 320 : s.Y,
                    Roles: WorkflowMap.ParseList(s.RolesJson),
                    Color: s.Color
                ))
                .ToList();

            var transitions = wf.WorkflowTransitions
                .OrderBy(t => t.Id)
                .Select(t => new TransitionVm(
                    t.Id,
                    t.FromStatusId!.Value.ToString(),
                    t.ToStatusId!.Value.ToString(),
                    WorkflowMap.NormalizeType(t.Type),
                    t.Label,
                    t.Rule,
                    WorkflowMap.ParseList(t.RoleNamesJson)
                ))
                .ToList();

            return new DesignerDto(
                new WorkflowVm(wf.Id.ToString(), wf.Name ?? ""),
                statuses,
                transitions
            );
        }

        public async Task SaveDesignerAsync(Guid companyId, Guid workflowId, DesignerDto payload, CancellationToken ct = default)
        {
            if (payload == null) throw new ArgumentNullException(nameof(payload));

            var wf = await _db.Workflows
                .Include(w => w.WorkflowStatuses)
                .Include(w => w.WorkflowTransitions)
                .SingleOrDefaultAsync(w => w.Id == workflowId, ct);

            if (wf == null) throw new KeyNotFoundException("Workflow not found.");
            if (wf.CompanyId != companyId) throw new InvalidOperationException("Workflow does not belong to the specified company.");

            // check trùng tên status (không phân biệt hoa thường)
            var dupCheck = payload.Statuses
                .Where(s => !string.IsNullOrWhiteSpace(s.Name))
                .GroupBy(s => s.Name.Trim(), StringComparer.OrdinalIgnoreCase)
                .FirstOrDefault(g => g.Count() > 1);
            if (dupCheck != null)
                throw new InvalidOperationException($"Duplicate status name in payload: \"{dupCheck.Key}\"");

            using var tx = await _db.Database.BeginTransactionAsync(ct);

            // ===== Upsert Statuses =====
            var statusIncoming = payload.Statuses ?? new();

            var normalized = statusIncoming.Select((s, idx) =>
            {
                var ok = Guid.TryParse(s.Id, out var gid);
                var newId = ok ? gid : Guid.NewGuid();
                return new
                {
                    OldId = s.Id,            // id FE gửi
                    Id = newId,              // id lưu DB
                    Name = (s.Name ?? "").Trim(),
                    s.IsStart,
                    s.IsEnd,
                    Position = idx,
                    s.X,
                    s.Y,
                    Color = WorkflowMap.NormalizeHex(s.Color),
                    RolesJson = WorkflowMap.ToJson(s.Roles)
                };
            }).ToList();

            // name unique
            var names = normalized.Where(x => !string.IsNullOrWhiteSpace(x.Name)).Select(x => x.Name).ToList();
            if (names.Count != names.Distinct(StringComparer.OrdinalIgnoreCase).Count())
                throw new InvalidOperationException("Duplicate status names (case-insensitive).");

            var mapByOldId = normalized.ToDictionary(x => x.OldId, x => x.Id, StringComparer.Ordinal);
            var existById = wf.WorkflowStatuses.ToDictionary(x => x.Id, x => x);

            // xoá những status không còn trong payload (kèm xoá transition liên quan)
            var keepIds = normalized.Select(x => x.Id).ToHashSet();
            var toDelete = wf.WorkflowStatuses.Where(s => !keepIds.Contains(s.Id)).ToList();
            if (toDelete.Count > 0)
            {
                var delIds = toDelete.Select(s => s.Id).ToList();

                var usedByTickets = await _db.Tickets.AsNoTracking()
                    .AnyAsync(t => t.StatusId != null && delIds.Contains(t.StatusId.Value), ct);
                if (usedByTickets)
                    throw new InvalidOperationException("Some statuses are referenced by Tickets — deletion is not allowed.");

                var usedByTaskWf = await _db.TaskWorkflows.AsNoTracking()
                    .AnyAsync(tw => tw.WorkflowStatusId != null && delIds.Contains(tw.WorkflowStatusId.Value), ct);
                if (usedByTaskWf)
                    throw new InvalidOperationException("Some statuses are referenced by TaskWorkflow — deletion is not allowed.");

                var transToRemove = await _db.WorkflowTransitions
                    .Where(tr => tr.WorkflowId == workflowId &&
                                 ((tr.FromStatusId != null && delIds.Contains(tr.FromStatusId.Value)) ||
                                  (tr.ToStatusId != null && delIds.Contains(tr.ToStatusId.Value))))
                    .ToListAsync(ct);
                if (transToRemove.Count > 0) _db.WorkflowTransitions.RemoveRange(transToRemove);

                _db.WorkflowStatuses.RemoveRange(toDelete);
            }

            // upsert
            foreach (var s in normalized)
            {
                if (!existById.TryGetValue(s.Id, out var entity))
                {
                    entity = new WorkflowStatus { Id = s.Id, WorkflowId = wf.Id };
                    _db.WorkflowStatuses.Add(entity);
                    wf.WorkflowStatuses.Add(entity);
                }

                entity.Name = s.Name;
                entity.IsStart = s.IsStart;
                entity.IsEnd = s.IsEnd;
                entity.Position = s.Position;
                entity.X = s.X;
                entity.Y = s.Y;
                entity.Color = s.Color;
                entity.RolesJson = s.RolesJson;
            }
            await _db.SaveChangesAsync(ct);

            // ===== Upsert Transitions (string type) =====
            var existingTrans = await _db.WorkflowTransitions
                .Where(tr => tr.WorkflowId == wf.Id)
                .ToListAsync(ct);

            // map From/To bằng mapByOldId trước, nếu không có thì TryParseGuid
            var trIncoming = (payload.Transitions ?? new())
                .Select(t =>
                {
                    Guid? f = mapByOldId.TryGetValue(t.FromStatusId, out var fId) ? fId : TryParseGuid(t.FromStatusId);
                    Guid? o = mapByOldId.TryGetValue(t.ToStatusId, out var tId) ? tId : TryParseGuid(t.ToStatusId);
                    return new
                    {
                        From = f,
                        To = o,
                        Type = WorkflowMap.NormalizeType(t.Type),
                        t.Label,
                        t.Rule,
                        RoleNamesJson = WorkflowMap.ToJson(t.RoleNames ?? new())
                    };
                })
                .Where(x => x.From.HasValue && x.To.HasValue)
                .Select(x => new { From = x.From!.Value, To = x.To!.Value, x.Type, x.Label, x.Rule, x.RoleNamesJson })
                .ToList();

            var keepPairs = trIncoming.Select(i => (i.From, i.To)).ToHashSet();
            var toRemoveTrans = existingTrans
                .Where(tr => !keepPairs.Contains((tr.FromStatusId!.Value, tr.ToStatusId!.Value)))
                .ToList();
            if (toRemoveTrans.Count > 0) _db.WorkflowTransitions.RemoveRange(toRemoveTrans);

            foreach (var inc in trIncoming)
            {
                var tr = existingTrans.FirstOrDefault(e => e.FromStatusId == inc.From && e.ToStatusId == inc.To);
                if (tr == null)
                {
                    tr = new WorkflowTransition
                    {
                        WorkflowId = wf.Id,
                        FromStatusId = inc.From,
                        ToStatusId = inc.To
                    };
                    _db.WorkflowTransitions.Add(tr);
                    existingTrans.Add(tr);
                }

                tr.Type = inc.Type;          // string
                tr.Label = inc.Label;
                tr.Rule = inc.Rule;
                tr.RoleNamesJson = inc.RoleNamesJson;
            }

            await _db.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);
        }
        public async Task<List<WorkflowPreviewVm>> GetPreviewsAsync(Guid companyId, CancellationToken ct = default)
        {
            var workflows = await _db.Workflows.AsNoTracking()
                .Where(w => w.CompanyId == companyId)
                .OrderByDescending(w => w.IsDefault).ThenBy(w => w.Name)
                .Select(w => new { w.Id, w.Name })
                .ToListAsync(ct);

            var wfIds = workflows.Select(w => w.Id).ToList();

            var statuses = await _db.WorkflowStatuses.AsNoTracking()
      .Where(s => s.WorkflowId.HasValue && wfIds.Contains(s.WorkflowId.Value))
      .Select(s => new
      {
          s.WorkflowId,
          s.Id,
          s.Name,
          s.IsStart,
          s.IsEnd,
          s.X,
          s.Y,
          s.Color,
          Roles = WorkflowMap.ParseList(s.RolesJson)   // ⭐ lấy roles
      })
      .ToListAsync(ct);

            var transitions = await _db.WorkflowTransitions.AsNoTracking()
                .Where(t => t.WorkflowId.HasValue
                         && wfIds.Contains(t.WorkflowId.Value)
                         && t.FromStatusId.HasValue
                         && t.ToStatusId.HasValue)
                .Select(t => new
                {
                    WorkflowId = t.WorkflowId!.Value,
                    From = t.FromStatusId!.Value,
                    To = t.ToStatusId!.Value,
                    t.Type,
                    t.Label
                })
                .ToListAsync(ct);

            var map = workflows.Select(w => new WorkflowPreviewVm(
                w.Id.ToString(),
                w.Name ?? "",
               statuses.Where(s => s.WorkflowId == w.Id)
    .OrderBy(s => s.Name)
    .Select(s => new StatusPreviewVm(
        s.Id.ToString(), s.Name ?? "", s.IsStart, s.IsEnd,
        s.X == 0 ? 200 : s.X,
        s.Y == 0 ? 320 : s.Y,
        s.Color,
        s.Roles
    )).ToList(),
                transitions.Where(t => t.WorkflowId == w.Id)
                           .Select(t => new TransitionPreviewVm(
                               t.From.ToString(),
                               t.To.ToString(),
                               WorkflowMap.NormalizeType(t.Type),
                               t.Label
                           )).ToList()
            )).ToList();

            return map;
        }

        public async Task<List<WorkflowPreviewVm>> GetPreviewsAdminAsync(Guid adminId, CancellationToken ct = default)
        {

            var adminUser = await _db.Users.SingleOrDefaultAsync(x => x.IsSystemAdmin && x.Id == adminId);

            if (adminUser == null)
                throw CustomExceptionFactory.CreateNotFoundError("Admin does not exist in this system");

            var workflows = await _db.Workflows.AsNoTracking()
                .OrderByDescending(w => w.IsDefault).ThenBy(w => w.Name)
                .Select(w => new { w.Id, w.Name })
                .ToListAsync(ct);

            var wfIds = workflows.Select(w => w.Id).ToList();

            var statuses = await _db.WorkflowStatuses.AsNoTracking()
      .Where(s => s.WorkflowId.HasValue && wfIds.Contains(s.WorkflowId.Value))
      .Select(s => new
      {
          s.WorkflowId,
          s.Id,
          s.Name,
          s.IsStart,
          s.IsEnd,
          s.X,
          s.Y,
          s.Color,
          Roles = WorkflowMap.ParseList(s.RolesJson)   // ⭐ lấy roles
      })
      .ToListAsync(ct);

            var transitions = await _db.WorkflowTransitions.AsNoTracking()
                .Where(t => t.WorkflowId.HasValue
                         && wfIds.Contains(t.WorkflowId.Value)
                         && t.FromStatusId.HasValue
                         && t.ToStatusId.HasValue)
                .Select(t => new
                {
                    WorkflowId = t.WorkflowId!.Value,
                    From = t.FromStatusId!.Value,
                    To = t.ToStatusId!.Value,
                    t.Type,
                    t.Label
                })
                .ToListAsync(ct);

            var map = workflows.Select(w => new WorkflowPreviewVm(
                w.Id.ToString(),
                w.Name ?? "",
               statuses.Where(s => s.WorkflowId == w.Id)
    .OrderBy(s => s.Name)
    .Select(s => new StatusPreviewVm(
        s.Id.ToString(), s.Name ?? "", s.IsStart, s.IsEnd,
        s.X == 0 ? 200 : s.X,
        s.Y == 0 ? 320 : s.Y,
        s.Color,
        s.Roles
    )).ToList(),
                transitions.Where(t => t.WorkflowId == w.Id)
                           .Select(t => new TransitionPreviewVm(
                               t.From.ToString(),
                               t.To.ToString(),
                               WorkflowMap.NormalizeType(t.Type),
                               t.Label
                           )).ToList()
            )).ToList();

            return map;
        }


        private static Guid? TryParseGuid(string? s)
            => string.IsNullOrWhiteSpace(s) ? (Guid?)null
             : Guid.TryParse(s, out var g) ? g : (Guid?)null;
    }

}
