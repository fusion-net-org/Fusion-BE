using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fusion.Repository.Bases.Page;
using Fusion.Repository.Bases.Page.Role;
using Fusion.Repository.Data;
using Fusion.Repository.Entities;

namespace Fusion.Repository.Repositories
{
    public interface IRoleRepository : IGenericRepository<Role>
    {
        Task<Role> CreateRoleAsync(Role role, CancellationToken ct);
        Task<Role?> GetRoleByIdAsync(int roleId, CancellationToken ct);
        Task<PagedResult<Role>> GetRolesPagedAsync(RolePagedRequest request, CancellationToken ct = default);
        Task<Role> UpdateRoleAsync(int roleId, Role updatedRole, CancellationToken ct);
        Task<bool> DeleteRoleAsync(int roleId, string reason,CancellationToken ct);
    }
}
