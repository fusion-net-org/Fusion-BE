using Fusion.Repository.Data;
using Fusion.Repository.Entities;
using Microsoft.EntityFrameworkCore;

public interface IRbacBootstrapper
{
    Task EnsureOwnerRoleAsync(Guid companyId, Guid ownerUserId, CancellationToken ct);
    Task TouchCompanyPermVersionAsync(Guid companyId, CancellationToken ct); // optional
}

public sealed class RbacBootstrapper : IRbacBootstrapper
{
    private readonly FusionDbContext _db;
    public const string OwnerRoleName = "Owner";

    public RbacBootstrapper(FusionDbContext db) => _db = db;

    public async Task EnsureOwnerRoleAsync(Guid companyId, Guid ownerUserId, CancellationToken ct)
    {
        // 1) Ensure Owner role exists
        var ownerRole = await _db.Roles.FirstOrDefaultAsync(
            r => r.CompanyId == companyId && r.RoleName == OwnerRoleName, ct);

        if (ownerRole == null)
        {
            ownerRole = new Role
            {
                CompanyId = companyId,
                RoleName = OwnerRoleName,
                Description = "Owner role - full permissions",
                Status = "Active"
            };
            _db.Roles.Add(ownerRole);
            await _db.SaveChangesAsync(ct);
        }

        // 2) Ensure owner user has UserRole
        var hasLink = await _db.UserRoles.AsNoTracking()
            .AnyAsync(ur => ur.UserId == ownerUserId && ur.RoleId == ownerRole.Id, ct);

        if (!hasLink)
        {
            _db.UserRoles.Add(new UserRole { UserId = ownerUserId, RoleId = ownerRole.Id });
            await _db.SaveChangesAsync(ct);
        }

        // 3) Ensure Owner role has ALL FunctionInPages (sync add-missing)
        var allFunctionIds = await _db.FunctionInPages.AsNoTracking()
            .Select(f => f.Id)
            .ToListAsync(ct);

        var existing = await _db.RolePermissions
            .Where(rp => rp.RoleId == ownerRole.Id && (rp.CompanyId == companyId || rp.CompanyId == null))
            .Select(rp => rp.FunctionId)
            .ToListAsync(ct);

        var existingSet = existing.Where(x => x.HasValue).Select(x => x!.Value).ToHashSet();

        var toAdd = allFunctionIds.Where(fid => !existingSet.Contains(fid)).ToList();
        foreach (var fid in toAdd)
        {
            _db.RolePermissions.Add(new RolePermission
            {
                CompanyId = companyId,
                RoleId = ownerRole.Id,
                FunctionId = fid,
                IsAccess = true
            });
        }

        if (toAdd.Count > 0)
        {
            await _db.SaveChangesAsync(ct);
            await TouchCompanyPermVersionAsync(companyId, ct); // optional nhưng rất nên có
        }
    }

    //  OPTIONAL nhưng nên có để invalidate cache an toàn khi revoke quyền
    public async Task TouchCompanyPermVersionAsync(Guid companyId, CancellationToken ct)
    {
        var company = await _db.Companies.FirstOrDefaultAsync(c => c.Id == companyId, ct);
        if (company == null) return;

        // giả sử company có UpdateAt
        company.UpdateAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
    }
}
