using Fusion.Repository.Bases.Page;
using Fusion.Repository.Entities;

namespace Fusion.Repository.IRepositories
{
    public interface ITaskRepository
    {
        Task<ProjectTask> AddAsync(ProjectTask entity, CancellationToken ct = default);
        Task<ProjectTask?> FindByIdAsync(Guid id, CancellationToken ct = default);
        Task<PagedResult<ProjectTask>> GetAllAsync(PagedRequest request, CancellationToken ct = default);
        Task<ProjectTask> UpdateAsync(ProjectTask entity, CancellationToken ct = default);
        Task<bool> SoftDeleteAsync(Guid id, CancellationToken ct = default);
    }
}
