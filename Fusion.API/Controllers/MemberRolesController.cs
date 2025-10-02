using Fusion.API.Auth;
using Fusion.Service.BusinessModels;
using Fusion.Service.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Fusion.API.Controllers
{
    [ApiController]
    [Route("api/companies/{companyId:guid}/members/{userId:guid}/roles")]
    public class MemberRolesController : ControllerBase
    {
        private readonly IMemberRoleService _service;
        public MemberRolesController(IMemberRoleService service) => _service = service;

        // GET: lấy danh sách role của member trong 1 company
        [HttpGet]
        [HasPermission("Member.AssignRole")]
        public async Task<IActionResult> Get(Guid companyId, Guid userId, CancellationToken ct)
            => Ok(await _service.GetAsync(companyId, userId, ct));

        // POST: gán thêm các role (idempotent)
        [HttpPost]
        public async Task<IActionResult> Add(Guid companyId, Guid userId, [FromBody] RoleIdsRequest req, CancellationToken ct)
        {
            if (req?.RoleIds == null) return BadRequest("roleIds is required.");
            await _service.AddAsync(companyId, userId, new RoleIdsDto(req.RoleIds), ct);
            return NoContent();
        }

        // PUT: thay thế toàn bộ role (set cứng)
        [HttpPut]
        public async Task<IActionResult> Replace(Guid companyId, Guid userId, [FromBody] RoleIdsRequest req, CancellationToken ct)
        {
            if (req?.RoleIds == null) return BadRequest("roleIds is required.");
            await _service.ReplaceAsync(companyId, userId, new RoleIdsDto(req.RoleIds), ct);
            return NoContent(); // hoặc return Ok(await _service.GetAsync(companyId, userId, ct));
        }

        // DELETE: gỡ 1 role
        [HttpDelete("{roleId:int}")]
        public async Task<IActionResult> Remove(Guid companyId, Guid userId, int roleId, CancellationToken ct)
        {
            await _service.RemoveAsync(companyId, userId, roleId, ct);
            return NoContent();
        }
    }
}
