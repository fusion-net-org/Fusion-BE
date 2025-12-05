using System.Security.Claims;
using Fusion.Repository.Bases.Page;
using Fusion.Repository.Bases.Page.Role;
using Fusion.Service.Commons.BaseResponses;
using Fusion.Service.IServices;
using Fusion.Service.ViewModels.Role.Request;
using Fusion.Service.ViewModels.Role.Responses;
using Microsoft.AspNetCore.Mvc;

[Route("api/roles")]
[ApiController]
public class RoleController : ControllerBase
{
    private readonly IRoleService _roleService;

    public RoleController(IRoleService roleService)
    {
        _roleService = roleService;
    }

    [HttpGet("paged")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseModel<PagedResult<RoleResponseVersion2>>))]
    public async Task<IActionResult> GetRolesPaged([FromQuery] RolePagedRequest request, CancellationToken cancellationToken)
    {
        var result = await _roleService.GetRolesPagedAsync(request, cancellationToken);
        return Ok(ResponseModel<PagedResult<RoleResponseVersion2>>.Ok(
            data: result,
            message: "Get paged roles successfully"));
    }

    [HttpGet("{roleId:int}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseModel<RoleResponseVersion2>))]
    public async Task<IActionResult> GetRoleById(int roleId, CancellationToken cancellationToken)
    {
        var result = await _roleService.GetRoleByIdAsync(roleId, cancellationToken);
        if (result == null)
            return NotFound(ResponseModel<RoleResponseVersion2>.Error(
                statusCode: StatusCodes.Status404NotFound,
                message: "Role not found"));

        return Ok(ResponseModel<RoleResponseVersion2>.Ok(
            data: result,
            message: "Get role by id successfully"));
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(ResponseModel<RoleResponseVersion2>))]
    public async Task<IActionResult> CreateRole(CreateRoleRequest request, CancellationToken cancellationToken)
    {
        var result = await _roleService.CreateRoleAsync(request, cancellationToken);
        return Ok(ResponseModel<RoleResponseVersion2>.Ok(
            data: result,
            message: "Create role successfully"));
    }

    [HttpPut("{roleId:int}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseModel<RoleResponseVersion2>))]
    public async Task<IActionResult> UpdateRole(int roleId, UpdateRoleRequest request, CancellationToken cancellationToken)
    {
        var result = await _roleService.UpdateRoleAsync(roleId, request, cancellationToken);
        return Ok(ResponseModel<RoleResponseVersion2>.Ok(
            data: result,
            message: "Update role successfully"));
    }

    [HttpDelete("{roleId:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> DeleteRole(int roleId, [FromBody] DeleteRoleRequest request, CancellationToken cancellationToken)
    {
        var result = await _roleService.DeleteRoleAsync(roleId, request.Reason, cancellationToken);
        if (!result)
            return NotFound(ResponseModel<bool>.Error(
                statusCode: StatusCodes.Status404NotFound,
                message: "Role not found"));

        return Ok(ResponseModel<bool>.Ok(
            data: true,
            message: "Delete role successfully"));
    }
}
