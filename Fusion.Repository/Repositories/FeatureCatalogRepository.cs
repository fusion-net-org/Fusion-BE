
using Fusion.Repository.Bases.Exceptions;
using Fusion.Repository.Bases.Page;
using Fusion.Repository.Bases.Page.FeatureCatalog;
using Fusion.Repository.Bases.Responses;
using Fusion.Repository.Data;
using Fusion.Repository.Entities;
using Fusion.Repository.IRepositories;
using Microsoft.EntityFrameworkCore;

namespace Fusion.Repository.Repositories;

public class FeatureCatalogRepository : GenericRepository<Feature>, IFeatureCatalogRepository
{
    private readonly FusionDbContext _context;
    public FeatureCatalogRepository(FusionDbContext context) : base(context)
    {
        _context = context;
    }

    public async Task<Feature> CreateAsync(Feature entity, CancellationToken ct = default)
    {
        entity.Code = NormalizeCode(entity.Code);
        if (string.IsNullOrWhiteSpace(entity.Code))
            throw CustomExceptionFactory.CreateNotFoundError(ResponseMessages.NOT_FOUND.FormatMessage("Feature code is required."));

        bool dup = await ExistsCodeAsync(entity.Code, null, ct);
        if (dup)
            throw CustomExceptionFactory.CreateBadRequestError(ResponseMessages.DUPLICATE.FormatMessage("Feature code already exists."));

        entity.CreatedAt = DateTime.UtcNow;
        entity.UpdatedAt = DateTime.UtcNow;

        _context.Features.Add(entity);
        await _context.SaveChangesAsync(ct);
        return entity;
    }
    public async Task<bool> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var db = await _context.Features
            .FirstOrDefaultAsync(x => x.Id == id, ct);

        if (db == null)
            return false;

        _context.Features.Remove(db);
        await _context.SaveChangesAsync(ct);
        return true;
    }
    public Task<Feature?> GetByCodeAsync(string code, CancellationToken ct = default)
    {
        var c = NormalizeCode(code);
        return _context.Features
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Code == c, ct);
    }
    public Task<Feature?> GetByIdAsync(Guid id, CancellationToken ct = default)
     => _context.Features
        .AsNoTracking()
        .FirstOrDefaultAsync(x => x.Id == id, ct);
    public async Task<Feature> UpdateAsync(Feature entity, CancellationToken ct = default)
    {
        var db = await _context.Features
            .FirstOrDefaultAsync(x => x.Id == entity.Id, ct)
                ?? throw CustomExceptionFactory.CreateNotFoundError(ResponseMessages.NOT_FOUND.FormatMessage("Feature not found."));

        var newCode = NormalizeCode(entity.Code);
        if (string.IsNullOrWhiteSpace(newCode))
            throw CustomExceptionFactory.CreateBadRequestError(ResponseMessages.INVALID_INPUT.FormatMessage("Feature code is required."));

        bool dup = await ExistsCodeAsync(newCode, db.Id, ct);
        if (dup)
            throw CustomExceptionFactory.CreateBadRequestError(ResponseMessages.DUPLICATE.FormatMessage("Feature code already exists."));

        db.Code = newCode;
        db.Name = entity.Name?.Trim() ?? string.Empty;
        db.Description = entity.Description;
        db.Category = entity.Category;
        db.IsActive = entity.IsActive;
        db.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(ct);
        return db;
    }
    private static string NormalizeCode(string code)
            => (code ?? string.Empty).Trim().ToLowerInvariant();
    public Task<bool> ExistsCodeAsync(string code, Guid? excludeId = null, CancellationToken ct = default)
    {
        var c = NormalizeCode(code);
        return _context.Features
            .AnyAsync(x => x.Code == c && (!excludeId.HasValue || x.Id != excludeId.Value), ct);
    }
    public async Task ToggleActiveAsync(Guid id, bool isActive, CancellationToken ct = default)
    {
        var db = await _context.Features
            .FirstOrDefaultAsync(x => x.Id == id, ct)
               ?? throw CustomExceptionFactory.CreateNotFoundError(ResponseMessages.NOT_FOUND.FormatMessage("Feature not found."));

        db.IsActive = isActive;
        db.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(ct);
    }

    public async Task<PagedResult<Feature>> GetAllAsync(FeatureCatalogPagedRequest request, CancellationToken ct = default)
    {
        var q = _context.Features
                        .AsNoTracking()
                        .AsQueryable();

        // --- Tìm kiếm tổng hợp ---
        if (!string.IsNullOrWhiteSpace(request.Keyword))
        {
            var kw = $"%{request.Keyword.Trim()}%";
            q = q.Where(f =>
                EF.Functions.Like(f.Code, kw) ||
                EF.Functions.Like(f.Name, kw) ||
                EF.Functions.Like(f.Category, kw) ||
                (f.Description != null && EF.Functions.Like(f.Description, kw)));
        }

        // --- Lọc trạng thái ---
        if (request.IsActive.HasValue)
        {
            q = q.Where(f => f.IsActive == request.IsActive.Value);
        }

        // --- Sắp xếp ---
        string sortColumn = nameof(Feature.CreatedAt);
        if (!string.IsNullOrWhiteSpace(request.SortColumn) &&
            FeatureCatalogPagedRequest.SortMap.TryGetValue(request.SortColumn, out var mapped))
        {
            sortColumn = mapped;
        }

        q = request.SortDescending
            ? q.OrderByDescending(e => EF.Property<object>(e, sortColumn))
            : q.OrderBy(e => EF.Property<object>(e, sortColumn));

        // --- Phân trang ---
        return await q.ToPagedResultAsync(request, ct);

    }

    public Task<List<Feature>> GetAllActiveAsync(CancellationToken ct = default)
    {

        return _context.Features
                       .AsNoTracking()
                       .Where(f => f.IsActive)
                       .OrderBy(f => f.Category)    
                       .ThenBy(f => f.Name)
                       .ThenBy(f => f.Code)
                       .ToListAsync(ct);
    }
}
