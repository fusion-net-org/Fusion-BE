

using Fusion.Repository.Bases.Page;
using Fusion.Repository.Bases.Page.FeatureCatalog;
using Fusion.Repository.Entities;

namespace Fusion.Repository.IRepositories;

public interface IFeatureCatalogRepository
{
    Task<Feature> CreateAsync(Feature entity, CancellationToken ct = default);
    Task<Feature?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Feature?> GetByCodeAsync(string code, CancellationToken ct = default);
    Task<Feature> UpdateAsync(Feature entity, CancellationToken ct = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken ct = default);
    Task<bool> ExistsCodeAsync(string code, Guid? excludeId = null, CancellationToken ct = default);
    Task ToggleActiveAsync(Guid id, bool isActive, CancellationToken ct = default);
    Task<PagedResult<Feature>> GetAllAsync(FeatureCatalogPagedRequest request, CancellationToken ct = default);
    Task<List<Feature>> GetAllActiveAsync(CancellationToken ct = default);

}
