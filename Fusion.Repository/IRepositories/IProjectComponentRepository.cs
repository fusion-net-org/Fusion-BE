using Fusion.Repository.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fusion.Repository.IRepositories
{
    public interface IProjectComponentRepository
    {
        Task<List<ProjectComponent>> CreateManyAsync(
        List<ProjectComponent> components,
        CancellationToken cancellationToken = default);

        Task<ProjectComponent?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

        Task<List<ProjectComponent>> GetByProjectIdAsync(Guid projectId, CancellationToken cancellationToken = default);

        Task<List<ProjectComponent>> GetByProjectRequestIdAsync(Guid projectRequestId, CancellationToken cancellationToken = default);

        Task<ProjectComponent> UpdateAsync(ProjectComponent component, CancellationToken cancellationToken = default);

        Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    }
}
