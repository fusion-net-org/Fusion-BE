using Fusion.Repository.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fusion.Service.Services
{
    public interface IRoleAdminService
    {
        Task<int> CreateAsync(Guid companyId, CreateRoleDto dto, CancellationToken ct = default);
        Task<RoleDetailVm?> GetByIdAsync(Guid companyId, int roleId, CancellationToken ct = default);
        Task<List<RoleDetailVm>> GetAllAsync(Guid companyId, CancellationToken ct = default);
        Task UpdatePermissionsAsync(Guid companyId, int roleId, IEnumerable<int> functionIds, CancellationToken ct = default);
        Task<int> UpdateInfoAsync(Guid companyId, int roleId, UpdateRoleInfoDto dto, CancellationToken ct = default);
        Task DeleteAsync(Guid companyId, int roleId, CancellationToken ct = default);
    }

    public class RoleAdminService : IRoleAdminService
    {
        private readonly IRoleAdminRepository _repo;

        public RoleAdminService(IRoleAdminRepository repo) => _repo = repo;
        public Task<RoleDetailVm?> GetByIdAsync(Guid companyId, int roleId, CancellationToken ct = default)
        => _repo.GetByIdWithPermissionsAsync(companyId, roleId, ct);
        public Task UpdatePermissionsAsync(Guid companyId, int roleId, IEnumerable<int> functionIds, CancellationToken ct = default)
        => _repo.UpdatePermissionsAsync(companyId, roleId, functionIds, ct);
        public Task<List<RoleDetailVm>> GetAllAsync(Guid companyId, CancellationToken ct = default)
        => _repo.GetAllWithPermissionsAsync(companyId, ct);
        public Task<int> UpdateInfoAsync(Guid companyId, int roleId, UpdateRoleInfoDto dto, CancellationToken ct = default)
        => _repo.UpdateInfoAsync(companyId, roleId, dto, ct);
        public Task DeleteAsync(Guid companyId, int roleId, CancellationToken ct = default)
      => _repo.DeleteAsync(companyId, roleId, ct);
        public async Task<int> CreateAsync(Guid companyId, CreateRoleDto dto, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(dto.Name))
                throw new ArgumentException("Role name is required.");

            if (await _repo.ExistsNameAsync(companyId, dto.Name.Trim(), ct))
                throw new InvalidOperationException("Role name already exists in this company.");

            return await _repo.CreateAsync(companyId, dto, ct);
        }
    }

}
