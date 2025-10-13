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
        public async Task<IActionResult> GetAllAsync(Guid companyId, CancellationToken ct = default)
        {
            var role = await _service.GetAllAsync(companyId, ct);
            return Ok(ResponseModel<List<RoleDetailVm>>.Ok(
               data: role,
               message: "Get role successfully"));
        }
    }

}
