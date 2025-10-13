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
    }

    public class RoleAdminService : IRoleAdminService
    {
        private readonly IRoleAdminRepository _repo;

        public RoleAdminService(IRoleAdminRepository repo) => _repo = repo;
        public Task<RoleDetailVm?> GetByIdAsync(Guid companyId, int roleId, CancellationToken ct = default)
        => _repo.GetByIdAsync(companyId, roleId, ct);
        public Task<List<RoleDetailVm>> GetAllAsync(Guid companyId, CancellationToken ct = default)
        => _repo.GetAllAsync(companyId, ct);
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
