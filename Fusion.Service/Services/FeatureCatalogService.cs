

using AutoMapper;
using Fusion.Repository.Bases.Exceptions;
using Fusion.Repository.Bases.Page;
using Fusion.Repository.Bases.Page.FeatureCatalog;
using Fusion.Repository.Bases.Responses;
using Fusion.Repository.Entities;
using Fusion.Repository.IRepositories;
using Fusion.Service.IServices;
using Fusion.Service.ViewModels.FeatureCatalog.Requests;
using Fusion.Service.ViewModels.FeatureCatalog.Responses;

namespace Fusion.Service.Services;

public class FeatureCatalogService : IFeatureCatalogService
{
    private readonly IFeatureCatalogRepository _repo;
    private readonly IMapper _mapper;

    public FeatureCatalogService(IFeatureCatalogRepository repo, IMapper mapper)
    {
        _repo = repo;
        _mapper = mapper;
    }

    public async Task<FeatureResponse> CreateAsync(FeatureCreateRequest req, CancellationToken ct = default)
    {
        var entity = _mapper.Map<Feature>(req);
        var created = await _repo.CreateAsync(entity, ct);
        return _mapper.Map<FeatureResponse>(created);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        return await _repo.DeleteAsync(id, ct);
    }

    public async Task<List<FeatureActiveResponse>> GetAllActiveAsync(CancellationToken ct = default)
    {
        var items = await _repo.GetAllActiveAsync(ct);
        return _mapper.Map<List<FeatureActiveResponse>>(items);
    }

    public async Task<PagedResult<FeatureResponse>> GetAllAsync(FeatureCatalogPagedRequest request, CancellationToken ct = default)
    {
        var entities = await _repo.GetAllAsync(request, ct);

        return new PagedResult<FeatureResponse>
        {
            Items = _mapper.Map<List<FeatureResponse>>(entities.Items),
            TotalCount = entities.TotalCount,
            PageNumber = entities.PageNumber,
            PageSize = entities.PageSize
        };
    }

    public async Task<FeatureResponse> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var f = await _repo.GetByIdAsync(id, ct)
               ?? throw CustomExceptionFactory.CreateNotFoundError(ResponseMessages.NOT_FOUND.FormatMessage("Feature not found."));
        return _mapper.Map<FeatureResponse>(f);
    }

    public async Task ToggleActiveAsync(Guid id, bool isActive, CancellationToken ct = default)
    {
        await _repo.ToggleActiveAsync(id, isActive, ct);
    }

    public async Task<FeatureResponse> UpdateAsync(FeatureUpdateRequest req, CancellationToken ct = default)
    {
        var entity = _mapper.Map<Feature>(req);
        var updated = await _repo.UpdateAsync(entity, ct);
        return _mapper.Map<FeatureResponse>(updated);
    }
}
