using AutoMapper;
using Fusion.Repository.Bases.Page;
using Fusion.Repository.Bases.Page.Role;
using Fusion.Repository.Entities;
using Fusion.Repository.Repositories;
using Fusion.Service.IServices;
using Fusion.Service.ViewModels.Role.Request;
using Fusion.Service.ViewModels.Role.Responses;

public class RoleService : IRoleService
{
    private readonly IRoleRepository _roleRepository;
    private readonly IMapper _mapper;

    public RoleService(IRoleRepository roleRepository, IMapper mapper)
    {
        _roleRepository = roleRepository;
        _mapper = mapper;
    }

    public async Task<RoleResponseVersion2> CreateRoleAsync(CreateRoleRequest request, CancellationToken ct)
    {
        var role = _mapper.Map<Role>(request);

        var created = await _roleRepository.CreateRoleAsync(role, ct);

        return _mapper.Map<RoleResponseVersion2>(created);
    }

    public async Task<RoleResponseVersion2?> GetRoleByIdAsync(int roleId, CancellationToken ct)
    {
        var role = await _roleRepository.GetRoleByIdAsync(roleId, ct);
        return role == null ? null : _mapper.Map<RoleResponseVersion2>(role);
    }


    public async Task<PagedResult<RoleResponseVersion2>> GetRolesPagedAsync(RolePagedRequest request, CancellationToken ct)
    {
        var pagedRoles = await _roleRepository.GetRolesPagedAsync(request, ct);

        var mapped = _mapper.Map<List<RoleResponseVersion2>>(pagedRoles.Items);

        return new PagedResult<RoleResponseVersion2>
        {
            Items = mapped,
            PageNumber = pagedRoles.PageNumber,
            PageSize = pagedRoles.PageSize,
            TotalCount = pagedRoles.TotalCount
        };
    }


    public async Task<RoleResponseVersion2> UpdateRoleAsync(int roleId, UpdateRoleRequest request, CancellationToken ct)
    {
        var updatedRole = _mapper.Map<Role>(request);
        var updated = await _roleRepository.UpdateRoleAsync(roleId, updatedRole, ct);
        return _mapper.Map<RoleResponseVersion2>(updated);
    }

    public async Task<bool> DeleteRoleAsync(int roleId, string reason, CancellationToken ct)
    {
        return await _roleRepository.DeleteRoleAsync(roleId,reason, ct);
    }


}
