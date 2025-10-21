using Fusion.Repository.Bases.Responses;
using Fusion.Repository.Data;
using Fusion.Repository.Entities;
using Fusion.Repository.Repositories;
using Fusion.Service.Commons.BaseResponses;
using Fusion.Service.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;

namespace Fusion.API.Controllers
{
    [ApiController]
    [Route("api/companies/{companyId:guid}/roles")]
    public class RolesAdminController : ControllerBase
    {
        private readonly IRoleAdminService _service;

        public RolesAdminController(IRoleAdminService service) => _service = service;

        // Quyền dành cho admin công ty
        [HttpPost]
        public async Task<IActionResult> Create(Guid companyId, [FromBody] CreateRoleDto dto, CancellationToken ct)
        {
            var id = await _service.CreateAsync(companyId, dto, ct);

            return CreatedAtAction(
                nameof(GetById),
                new { companyId, roleId = id },
                ResponseModel<object>.Ok(
                    data: new { id },
                    message: ResponseMessageHelper.FormatMessage(ResponseMessages.SAVE_SUCCESS, "Tạo role thành công")
                )
            );
        }

        [HttpGet("{roleId:int}")]
        public async Task<ActionResult<RoleDetailVm>> GetById(Guid companyId, int roleId, CancellationToken ct)
        {
            var role = await _service.GetByIdAsync(companyId, roleId, ct);
            return Ok(ResponseModel<RoleDetailVm>.Ok(
               data: role,
               message: "Get role successfully"));
        }

        [HttpGet]
        public async Task<IActionResult> GetAllAsync(Guid companyId, CancellationToken ct = default)
        {
            var role = await _service.GetAllAsync(companyId, ct);
            return Ok(ResponseModel<List<RoleDetailVm>>.Ok(
               data: role,
               message: "Get role successfully"));
        }
        [HttpPut("{roleId:int}/permissions")]
        [Consumes("application/json")]
        public async Task<IActionResult> PutPermissions(
         Guid companyId,
         int roleId,
         [FromBody] int[] functionIds,      // <— nhận mảng thô
         CancellationToken ct)
        {
            await _service.UpdatePermissionsAsync(companyId, roleId, functionIds ?? Array.Empty<int>(), ct);

            return Ok(ResponseModel<object>.Ok(
                data: new { roleId },
                message: ResponseMessageHelper.FormatMessage(
                    ResponseMessages.SAVE_SUCCESS,
                    "Cập nhật permissions thành công")
            ));
        }
        [HttpPost("{roleId:int}")]
        public async Task<IActionResult> UpdateInfo(
     Guid companyId,
     int roleId,
     [FromBody] UpdateRoleInfoDto dto,
     CancellationToken ct)
        {
            var id = await _service.UpdateInfoAsync(companyId, roleId, dto, ct);

            return Ok(ResponseModel<object>.Ok(
                data: new { id },
                message: ResponseMessageHelper.FormatMessage(ResponseMessages.SAVE_SUCCESS, "Cập nhật role thành công")
            ));
        }
        [HttpDelete("{roleId:int}")]
        public async Task<IActionResult> Delete(Guid companyId, int roleId, CancellationToken ct)
        {
            await _service.DeleteAsync(companyId, roleId, ct);

            return Ok(ResponseModel<object>.Ok(
                data: new { roleId },
                message: ResponseMessageHelper.FormatMessage(
                    ResponseMessages.DELETE_SUCCESS, "Xoá role thành công")
            ));
        }

    }

}
