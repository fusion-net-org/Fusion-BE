

using Fusion.Repository.Bases.Page;
using Fusion.Repository.Bases.Page.FeatureCatalog;
using Fusion.Repository.Entities;
using Fusion.Service.ViewModels.FeatureCatalog.Requests;
using Fusion.Service.ViewModels.FeatureCatalog.Responses;

namespace Fusion.Service.IServices;

public interface IFeatureCatalogService
{
    Task<FeatureResponse> CreateAsync(FeatureCreateRequest req, CancellationToken ct = default);
    Task<FeatureResponse> UpdateAsync(FeatureUpdateRequest req, CancellationToken ct = default);
    Task<FeatureResponse> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task ToggleActiveAsync(Guid id, bool isActive, CancellationToken ct = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken ct = default);
    Task<PagedResult<FeatureResponse>> GetAllAsync(FeatureCatalogPagedRequest request, CancellationToken ct = default);
    Task<List<FeatureActiveResponse>> GetAllActiveAsync(CancellationToken ct = default);
}
