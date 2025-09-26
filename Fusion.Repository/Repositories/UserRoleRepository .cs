using Fusion.Repository.Data;
using Fusion.Repository.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Fusion.Repository.Repositories
{
    public interface IUserRoleRepository
    {
        Task<List<Role>> GetRolesAsync(Guid companyId, Guid userId, CancellationToken ct);
        Task AddAsync(Guid companyId, Guid userId, IEnumerable<int> roleIds, CancellationToken ct);
        Task ReplaceAsync(Guid companyId, Guid userId, IEnumerable<int> roleIds, CancellationToken ct);
        Task RemoveAsync(Guid companyId, Guid userId, int roleId, CancellationToken ct);
        Task<bool> IsMemberAsync(Guid companyId, Guid userId, CancellationToken ct);
    }

    public class UserRoleRepository : IUserRoleRepository
    {
        private readonly FusionDbContext _db;
        public UserRoleRepository(FusionDbContext db) => _db = db;

        public async Task<bool> IsMemberAsync(Guid companyId, Guid userId, CancellationToken ct) =>
            await _db.CompanyMembers.AnyAsync(m => m.CompanyId == companyId && m.UserId == userId /*&& m.IsActive*/, ct);

        // Lấy roles của user trong 1 company
        public async Task<List<Role>> GetRolesAsync(Guid companyId, Guid userId, CancellationToken ct) =>
            await (from ur in _db.UserRoles
                   join r in _db.Roles on ur.RoleId equals r.Id
                   where ur.UserId == userId && r.CompanyId == companyId
                   select r)
                  .AsNoTracking()
                  .ToListAsync(ct);

        // Thêm các role (cộng dồn) – chỉ nhận roleIds thuộc company
        public async Task AddAsync(Guid companyId, Guid userId, IEnumerable<int> roleIds, CancellationToken ct)
        {
            var roleIdsSet = roleIds?.ToHashSet() ?? new HashSet<int>();
            if (roleIdsSet.Count == 0) return;

            // role hợp lệ của company
            var validRoleIds = await _db.Roles
                .Where(r => r.CompanyId == companyId && roleIdsSet.Contains(r.Id))
                .Select(r => r.Id)
                .ToListAsync(ct);                    // List<int>

            if (validRoleIds.Count == 0) return;

            // role đã có của user trong company (é p về int, bỏ null)
            var exist = await (from ur in _db.UserRoles
                               join r in _db.Roles on ur.RoleId equals r.Id
                               where ur.UserId == userId && r.CompanyId == companyId && ur.RoleId != null
                               select ur.RoleId!.Value)
                              .ToListAsync(ct);       // List<int>

            // giờ 2 vế đều List<int> → Except OK
            var toAddIds = validRoleIds.Except(exist).ToList();
            if (toAddIds.Count == 0) return;

            var toAdd = toAddIds.Select(rid => new UserRole { UserId = userId, RoleId = rid });
            _db.UserRoles.AddRange(toAdd);
            await _db.SaveChangesAsync(ct);
        }

        // Thay thế toàn bộ role: chỉ giữ các roleIds (thuộc company), xóa phần còn lại
        public async Task ReplaceAsync(Guid companyId, Guid userId, IEnumerable<int> roleIds, CancellationToken ct)
        {
            var roleIdsSet = roleIds?.ToHashSet() ?? new HashSet<int>();

            // chỉ giữ role thuộc company + có trong input
            var keep = new HashSet<int>(await _db.Roles
                .Where(r => r.CompanyId == companyId && roleIdsSet.Contains(r.Id))
                .Select(r => r.Id)
                .ToListAsync(ct));                   // HashSet<int>

            // current của user trong company (chỉ lấy bản ghi có RoleId != null)
            var current = await (from ur in _db.UserRoles
                                 join r in _db.Roles on ur.RoleId equals r.Id
                                 where ur.UserId == userId && r.CompanyId == companyId && ur.RoleId != null
                                 select ur)
                                .ToListAsync(ct);

            // Xoá những role không còn trong keep
            var toRemove = current.Where(ur => !keep.Contains(ur.RoleId!.Value)).ToList();
            if (toRemove.Count > 0) _db.UserRoles.RemoveRange(toRemove);

            // Thêm những role mới cần có
            var currentIds = current.Select(x => x.RoleId!.Value).ToHashSet();
            var toAdd = keep.Where(id => !currentIds.Contains(id))
                            .Select(id => new UserRole { UserId = userId, RoleId = id });

            if (toAdd.Any()) _db.UserRoles.AddRange(toAdd);

            if (toRemove.Count > 0 || toAdd.Any())
                await _db.SaveChangesAsync(ct);
        }

        // Gỡ 1 role (chỉ nếu role thuộc company đang thao tác)
        public async Task RemoveAsync(Guid companyId, Guid userId, int roleId, CancellationToken ct)
        {
            var belongs = await _db.Roles.AnyAsync(r => r.Id == roleId && r.CompanyId == companyId, ct);
            if (!belongs) return;

            var ur = await _db.UserRoles.FirstOrDefaultAsync(x => x.UserId == userId && x.RoleId == roleId, ct);
            if (ur == null) return;

            _db.UserRoles.Remove(ur);
            await _db.SaveChangesAsync(ct);
        }
    }
}
