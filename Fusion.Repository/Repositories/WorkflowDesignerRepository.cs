using Fusion.Repository.Data;
using Fusion.Repository.Entities;
using Microsoft.EntityFrameworkCore;

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

    public interface IWorkflowDesignerRepository
    {
        Task<List<WorkflowListItemVm>> GetAllAsync(Guid companyId, CancellationToken ct = default);
        Task<Guid> CreateAsync(Guid companyId, string name, CancellationToken ct = default);
        Task DeleteAsync(Guid companyId, Guid workflowId, CancellationToken ct = default);

        Task<DesignerDto> GetDesignerAsync(Guid workflowId, CancellationToken ct = default);
        Task SaveDesignerAsync(Guid companyId, Guid workflowId, DesignerDto payload, CancellationToken ct = default);
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

            // Block delete if any Project is using this workflow
            var anyProjects = await _db.Projects.AsNoTracking()
                .AnyAsync(p => p.WorkflowId == workflowId, ct);
            if (anyProjects)
                throw new InvalidOperationException("Cannot delete workflow because it is referenced by Projects.");

            using var tx = await _db.Database.BeginTransactionAsync(ct);

            // Check if any status is referenced by Tickets or TaskWorkflows
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

                // Remove transitions, then statuses
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

            // Default layout from "Position" (DB has no x/y)
            var statuses = wf.WorkflowStatuses
                .OrderBy(s => s.Position)
                .Select((s, idx) => new StatusVm(
                    s.Id.ToString(),
                    s.Name ?? "",
                    s.IsStart,
                    s.IsEnd,
                    X: 200 + idx * 320,   // default X by order
                    Y: 320,               // default Y fixed
                    Roles: new List<string>(), // not stored in DB
                    Color: null                // not stored in DB
                ))
                .ToList();

            var transitions = wf.WorkflowTransitions
                .OrderBy(t => t.Id)
                .Select(t => new TransitionVm(
                    t.Id,
                    FromStatusId: t.FromStatusId!.Value.ToString(),
                    ToStatusId: t.ToStatusId!.Value.ToString(),
                    Type: "optional",      // not stored in DB
                    Label: null,
                    Rule: null,
                    RoleNames: new List<string>() // not stored in DB
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

            // Ensure status names are unique within the workflow (case-insensitive)
            var dupCheck = payload.Statuses
                .Where(s => !string.IsNullOrWhiteSpace(s.Name))
                .GroupBy(s => s.Name.Trim(), StringComparer.OrdinalIgnoreCase)
                .FirstOrDefault(g => g.Count() > 1);
            if (dupCheck != null)
                throw new InvalidOperationException($"Duplicate status name in payload: \"{dupCheck.Key}\"");

            using var tx = await _db.Database.BeginTransactionAsync(ct);

            // ===== Upsert Statuses (by position) =====
            var incoming = payload.Statuses ?? new();
            // Map string id -> Guid; generate a new Guid if parsing fails
            var normalized = incoming.Select((s, idx) =>
            {
                var ok = Guid.TryParse(s.Id, out var gid);
                return new
                {
                    Id = ok ? gid : Guid.NewGuid(),
                    Name = (s.Name ?? "").Trim(),
                    s.IsStart,
                    s.IsEnd,
                    Position = idx
                };
            }).ToList();

            // Extra name-uniqueness safety
            var names = normalized.Where(x => !string.IsNullOrWhiteSpace(x.Name))
                                  .Select(x => x.Name).ToList();
            if (names.Count != names.Distinct(StringComparer.OrdinalIgnoreCase).Count())
                throw new InvalidOperationException("Duplicate status names (case-insensitive).");

            var existById = wf.WorkflowStatuses.ToDictionary(x => x.Id, x => x);

            // Delete statuses not present in payload (but only if not referenced)
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

                // Remove related transitions first
                var transToRemove = await _db.WorkflowTransitions
                    .Where(tr => tr.WorkflowId == workflowId &&
                                 ((tr.FromStatusId != null && delIds.Contains(tr.FromStatusId.Value)) ||
                                  (tr.ToStatusId != null && delIds.Contains(tr.ToStatusId.Value))))
                    .ToListAsync(ct);
                if (transToRemove.Count > 0) _db.WorkflowTransitions.RemoveRange(transToRemove);

                _db.WorkflowStatuses.RemoveRange(toDelete);
            }

            // Upsert / update statuses
            foreach (var s in normalized)
            {
                if (!existById.TryGetValue(s.Id, out var entity))
                {
                    entity = new WorkflowStatus
                    {
                        Id = s.Id,
                        WorkflowId = wf.Id
                    };
                    _db.WorkflowStatuses.Add(entity);
                    wf.WorkflowStatuses.Add(entity);
                }

                entity.Name = s.Name;
                entity.IsStart = s.IsStart;
                entity.IsEnd = s.IsEnd;
                entity.Position = s.Position;
                // guard_name_key: leave untouched (not used by the payload)
            }
            await _db.SaveChangesAsync(ct);

            // ===== Upsert Transitions (by unique pair) =====
            var nowStatusIds = await _db.WorkflowStatuses.AsNoTracking()
                .Where(x => x.WorkflowId == workflowId)
                .Select(x => x.Id)
                .ToListAsync(ct);
            var statusSet = nowStatusIds.ToHashSet();

            var incomingPairs = (payload.Transitions ?? new())
                .Select(t => (
                    From: TryParseGuid(t.FromStatusId),
                    To: TryParseGuid(t.ToStatusId)
                ))
                .Where(p => p.From.HasValue && p.To.HasValue)
                .Select(p => (p.From!.Value, p.To!.Value))
                .Where(p => statusSet.Contains(p.Item1) && statusSet.Contains(p.Item2)) // only keep valid pairs
                .Distinct()
                .ToHashSet();

            // Remove transitions that are no longer present
            var delTrans = wf.WorkflowTransitions
                .Where(tr => !incomingPairs.Contains((tr.FromStatusId!.Value, tr.ToStatusId!.Value)))
                .ToList();
            if (delTrans.Count > 0) _db.WorkflowTransitions.RemoveRange(delTrans);

            // Add missing transitions
            foreach (var (fromId, toId) in incomingPairs)
            {
                var exists = wf.WorkflowTransitions.Any(tr =>
                    tr.FromStatusId == fromId && tr.ToStatusId == toId);
                if (!exists)
                {
                    _db.WorkflowTransitions.Add(new WorkflowTransition
                    {
                        WorkflowId = wf.Id,
                        FromStatusId = fromId,
                        ToStatusId = toId
                    });
                }
            }

            await _db.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);
        }

        // Helpers
        private static Guid? TryParseGuid(string? s)
        {
            if (string.IsNullOrWhiteSpace(s)) return null;
            return Guid.TryParse(s, out var g) ? g : null;
        }
    }
}
