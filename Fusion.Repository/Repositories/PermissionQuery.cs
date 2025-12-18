using Fusion.Repository.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Fusion.Repository.Repositories
{
    public interface IPermissionQuery
    {
        Task<HashSet<string>> GetEffectivePermissionsAsync(Guid companyId, Guid userId, CancellationToken ct);
    }

    public sealed class PermissionQuery : IPermissionQuery
    {
        private readonly FusionDbContext _db;
        public PermissionQuery(FusionDbContext db) => _db = db;

        public async Task<HashSet<string>> GetEffectivePermissionsAsync(Guid companyId, Guid userId, CancellationToken ct)
        {
            // (A) Owner => full quyền
            var isOwner = await _db.Companies.AsNoTracking()
                .AnyAsync(c => c.Id == companyId && c.IsDeleted != true && c.OwnerUserId == userId, ct);

            if (isOwner)
            {
                var all = await _db.FunctionInPages.AsNoTracking()
                    .Select(f => f.FunctionCode)
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .ToListAsync(ct);

                return all.ToHashSet(StringComparer.OrdinalIgnoreCase);
            }

            // (B) RBAC thường
            var codes = await (
                from ur in _db.UserRoles.AsNoTracking()
                where ur.UserId == userId && ur.RoleId != null

                join r in _db.Roles.AsNoTracking()
                    on ur.RoleId!.Value equals r.Id
                where r.CompanyId == companyId
                      && (r.Status == null || r.Status == "Active")

                join rp in _db.RolePermissions.AsNoTracking()
                    on r.Id equals rp.RoleId
                where rp.IsAccess == true
                      && (rp.CompanyId == companyId || rp.CompanyId == null)
                      && rp.FunctionId != null

                join f in _db.FunctionInPages.AsNoTracking()
                    on rp.FunctionId!.Value equals f.Id

                select f.FunctionCode
            )
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct()
            .ToListAsync(ct);

            return codes.ToHashSet(StringComparer.OrdinalIgnoreCase);
        }
    }
}
