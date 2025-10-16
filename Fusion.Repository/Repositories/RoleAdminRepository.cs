using Fusion.Repository.Data;
using Fusion.Repository.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fusion.Repository.Repositories
{
    public record CreateRoleDto(string Name, string? Description, IEnumerable<int>? FunctionIdsToGrant);
    public record RoleDetailVm(int Id, string Name, string? Description, List<RolePermissionVm> Permissions);
    public record RolePermissionVm(int FunctionId, string FunctionCode, string FunctionName, bool IsAccess);


    public interface IRoleAdminRepository
    {
        Task<RoleDetailVm?> GetByIdAsync(Guid companyId, int roleId, CancellationToken ct = default);
        Task<RoleDetailVm?> GetByIdWithPermissionsAsync(Guid companyId, int roleId, CancellationToken ct = default);
        Task<int> CreateAsync(Guid companyId, CreateRoleDto dto, CancellationToken ct = default);
        Task<bool> ExistsNameAsync(Guid companyId, string name, CancellationToken ct = default);
        Task<List<RoleDetailVm>> GetAllAsync(Guid companyId, CancellationToken ct = default);
        Task<List<RoleDetailVm>> GetAllWithPermissionsAsync(Guid companyId, CancellationToken ct = default);
    }

    public class RoleAdminRepository : IRoleAdminRepository
    {
        private readonly FusionDbContext _db;
        public RoleAdminRepository(FusionDbContext db) => _db = db;

        public Task<bool> ExistsNameAsync(Guid companyId, string name, CancellationToken ct = default)
            => _db.Roles.AsNoTracking().AnyAsync(r => r.CompanyId == companyId && r.RoleName == name, ct);

        public async Task<int> CreateAsync(Guid companyId, CreateRoleDto dto, CancellationToken ct = default)
        {
            var role = new Role
            {
                CompanyId = companyId,
                RoleName = dto.Name.Trim(),
                Description = dto.Description?.Trim(),
            };
            _db.Roles.Add(role);
            await _db.SaveChangesAsync(ct); // có Id

            if (dto.FunctionIdsToGrant != null)
            {
                var distinct = dto.FunctionIdsToGrant.Distinct().ToList();
                // chỉ nhận những function id còn active
                var validIds = await _db.FunctionInPages.AsNoTracking()
                    .Where(f => distinct.Contains(f.Id))
                    .Select(f => f.Id).ToListAsync(ct);

                foreach (var fid in validIds)
                    _db.RolePermissions.Add(new RolePermission { RoleId = role.Id, FunctionId = fid, IsAccess = true, CompanyId = companyId });

                await _db.SaveChangesAsync(ct);
            }

            return role.Id;
        }
        public async Task<RoleDetailVm?> GetByIdAsync(Guid companyId, int roleId, CancellationToken ct = default)
        {
            var role = await _db.Roles.AsNoTracking()
                .Where(r => r.CompanyId == companyId && r.Id == roleId)
                .Select(r => new { r.Id, r.RoleName, r.Description })
                .FirstOrDefaultAsync(ct);

            if (role == null) return null;

            return new RoleDetailVm(role.Id, role.RoleName, role.Description, new());
        }

        public async Task<RoleDetailVm?> GetByIdWithPermissionsAsync(
            Guid companyId, int roleId, CancellationToken ct = default)
        {
            var role = await _db.Roles.AsNoTracking()
                .Where(r => r.CompanyId == companyId && r.Id == roleId)
                .Select(r => new { r.Id, r.RoleName, r.Description })
                .FirstOrDefaultAsync(ct);

            if (role == null) return null;

            // 1) tất cả function
            var functions = await _db.FunctionInPages.AsNoTracking()
                .OrderBy(f => f.SortOrder).ThenBy(f => f.FunctionName)
                .Select(f => new { f.Id, f.FunctionCode, f.FunctionName })
                .ToListAsync(ct);

            // 2) function đã grant cho role
            var grantedIds = await _db.RolePermissions.AsNoTracking()
      .Where(rp => rp.RoleId == roleId
                   && rp.IsAccess
                   && (rp.CompanyId == companyId || rp.CompanyId == null)) 
      .Select(rp => rp.FunctionId)
      .ToListAsync(ct);
            var grantedSet = grantedIds.ToHashSet();

            // 3) ghép in-memory
            var perms = functions
    .Where(f => grantedSet.Contains(f.Id))                  
    .Select(f => new RolePermissionVm(
        f.Id, f.FunctionCode, f.FunctionName, true         
    ))
    .ToList();

            return new RoleDetailVm(role.Id, role.RoleName, role.Description, perms);
        }


        // lấy TẤT CẢ role kèm TOÀN BỘ function
        public async Task<List<RoleDetailVm>> GetAllWithPermissionsAsync(
            Guid companyId, CancellationToken ct = default)
        {
            var roles = await _db.Roles.AsNoTracking()
                .Where(r => r.CompanyId == companyId)
                .OrderBy(r => r.RoleName)
                .Select(r => new { r.Id, r.RoleName, r.Description })
                .ToListAsync(ct);
            if (roles.Count == 0) return new();

            // functions: 1 lần
            var functions = await _db.FunctionInPages.AsNoTracking()
                .OrderBy(f => f.SortOrder).ThenBy(f => f.FunctionName)
                .Select(f => new { f.Id, f.FunctionCode, f.FunctionName })
                .ToListAsync(ct);

            var roleIds = roles.Select(r => r.Id).ToList();

            // tất cả grant cho các role liên quan (chỉ lấy IsAccess = true)
            var rpRows = await _db.RolePermissions.AsNoTracking()
                .Where(rp => rp.RoleId.HasValue
             && roleIds.Contains(rp.RoleId.Value)
             && (rp.CompanyId == companyId || rp.CompanyId == null) 
             && rp.IsAccess)
                .Select(rp => new { RoleId = rp.RoleId!.Value, rp.FunctionId })
                .ToListAsync(ct);

            // map role -> set functionId
            var map = rpRows
                .GroupBy(x => x.RoleId)
                .ToDictionary(g => g.Key, g => g.Select(x => x.FunctionId).ToHashSet());

            // ghép in-memory
            var result = roles.Select(r =>
            {
                if (!map.TryGetValue(r.Id, out var set) || set.Count == 0)
                    return new RoleDetailVm(r.Id, r.RoleName, r.Description, new List<RolePermissionVm>());

                var perms = functions
                    .Where(f => set.Contains(f.Id)) 
                    .Select(f => new RolePermissionVm(
                        f.Id, f.FunctionCode, f.FunctionName, true)) 
                    .ToList();

                return new RoleDetailVm(r.Id, r.RoleName, r.Description, perms);
            }).ToList();

            return result;
        }





        public Task<List<RoleDetailVm>> GetAllAsync(Guid companyId, CancellationToken ct = default)
        {
            return _db.Roles.AsNoTracking()
                .Where(r => r.CompanyId == companyId)
                .OrderBy(r => r.RoleName)
                .Select(r => new RoleDetailVm(r.Id, r.RoleName, r.Description, new List<RolePermissionVm>()))
                .ToListAsync(ct);
        }
    }
}
