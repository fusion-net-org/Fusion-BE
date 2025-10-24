using Fusion.Repository.Repositories;

namespace Fusion.Service.Services
{
    public interface IWorkflowDesignerService
    {
        Task<List<WorkflowListItemVm>> GetAllAsync(Guid companyId, CancellationToken ct = default);
        Task<Guid> CreateAsync(Guid companyId, string name, CancellationToken ct = default);
        Task DeleteAsync(Guid companyId, Guid workflowId, CancellationToken ct = default);

        Task<DesignerDto> GetDesignerAsync(Guid workflowId, CancellationToken ct = default);
        Task SaveDesignerAsync(Guid companyId, Guid workflowId, DesignerDto payload, CancellationToken ct = default);
    }

    public class WorkflowDesignerService : IWorkflowDesignerService
    {
        private readonly IWorkflowDesignerRepository _repo;
        public WorkflowDesignerService(IWorkflowDesignerRepository repo) => _repo = repo;

        public Task<List<WorkflowListItemVm>> GetAllAsync(Guid companyId, CancellationToken ct = default)
            => _repo.GetAllAsync(companyId, ct);

        public async Task<Guid> CreateAsync(Guid companyId, string name, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Workflow name is required.");
            return await _repo.CreateAsync(companyId, name.Trim(), ct);
        }

        public Task DeleteAsync(Guid companyId, Guid workflowId, CancellationToken ct = default)
            => _repo.DeleteAsync(companyId, workflowId, ct);

        public Task<DesignerDto> GetDesignerAsync(Guid workflowId, CancellationToken ct = default)
            => _repo.GetDesignerAsync(workflowId, ct);

        public async Task SaveDesignerAsync(Guid companyId, Guid workflowId, DesignerDto payload, CancellationToken ct = default)
        {
            // Chuẩn hoá sơ bộ type
            foreach (var t in payload.Transitions)
            {
                var typ = (t.Type ?? "optional").ToLowerInvariant();
                if (typ is not ("success" or "failure" or "optional"))
                    throw new InvalidOperationException($"Loại transition không hợp lệ: {t.Type}");
            }
            await _repo.SaveDesignerAsync(companyId, workflowId, payload, ct);
        }
    }
}
