using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fusion.Repository.Bases.Page;
using Fusion.Repository.Bases.Page.Role;
using Fusion.Service.ViewModels.Role.Request;
using Fusion.Service.ViewModels.Role.Responses;

namespace Fusion.Service.IServices
{
    public interface IRoleService
    {
        Task<RoleResponseVersion2> CreateRoleAsync(CreateRoleRequest request, CancellationToken ct);
        Task<RoleResponseVersion2?> GetRoleByIdAsync(int roleId, CancellationToken ct);
        Task<PagedResult<RoleResponseVersion2>> GetRolesPagedAsync(RolePagedRequest request, CancellationToken ct);
        Task<RoleResponseVersion2> UpdateRoleAsync(int roleId, UpdateRoleRequest request, CancellationToken ct);
        Task<bool> DeleteRoleAsync(int roleId, string reason, CancellationToken ct);
    }
}
