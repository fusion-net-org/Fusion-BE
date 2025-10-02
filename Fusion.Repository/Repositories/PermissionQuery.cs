using Fusion.Repository.Data;
using Fusion.Repository.Entities;       // DbSets/Entities
using Microsoft.EntityFrameworkCore;   // ToListAsync, Distinct
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

    public class PermissionQuery : IPermissionQuery
    {
        private readonly FusionDbContext _db;
        public PermissionQuery(FusionDbContext db) => _db = db;

        public async Task<HashSet<string>> GetEffectivePermissionsAsync(Guid companyId, Guid userId, CancellationToken ct)
        {
            // JOIN: UserRoles -> Roles -> RolePermissions -> FunctionInPages
            var codes = await (
                from ur in _db.UserRoles
                where ur.UserId == userId && ur.RoleId != null
                join r in _db.Roles on ur.RoleId!.Value equals r.Id
                where r.CompanyId == companyId
                join rp in _db.RolePermissions on r.Id equals rp.RoleId
                join f in _db.FunctionInPages on rp.FunctionId equals f.Id
                // ---- Quan trọng: dùng IsAccess thay vì IsAllowed; nếu nullable thì so sánh == true
                where (rp.IsAccess == true) 
                select f.FunctionCode
            )
            .Distinct()
            .ToListAsync(ct);

            return codes.ToHashSet(StringComparer.OrdinalIgnoreCase);
        }
    }
}
