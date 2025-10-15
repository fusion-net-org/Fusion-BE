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
    public record RoleDetailVm(int Id, string Name, string? Description);

    public interface IRoleAdminRepository
    {
        Task<RoleDetailVm?> GetByIdAsync(Guid companyId, int roleId, CancellationToken ct = default);
        Task<int> CreateAsync(Guid companyId, CreateRoleDto dto, CancellationToken ct = default);
        Task<bool> ExistsNameAsync(Guid companyId, string name, CancellationToken ct = default);
        Task<List<RoleDetailVm>> GetAllAsync(Guid companyId, CancellationToken ct = default);
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
            return await _db.Roles.AsNoTracking()
                .Where(r => r.CompanyId == companyId && r.Id == roleId)
                .Select(r => new RoleDetailVm(r.Id, r.RoleName, r.Description))
                .FirstOrDefaultAsync(ct);
        }
        public Task<List<RoleDetailVm>> GetAllAsync(Guid companyId, CancellationToken ct = default)
        {
            return _db.Roles.AsNoTracking()
                .Where(r => r.CompanyId == companyId)
                .OrderBy(r => r.RoleName)
                .Select(r => new RoleDetailVm(r.Id, r.RoleName, r.Description))
                .ToListAsync(ct);
        }
    }
}
