using Fusion.Repository.Bases.Page;
using Fusion.Repository.Bases.Page.UserLog;
using Fusion.Repository.Entities;
using Fusion.Service.Commons.BaseResponses;
using Fusion.Service.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Fusion.API.Controllers
{
    [Authorize(Roles = "Admin")]
    [Route("api/[controller]")]
    [ApiController]
    public class UserLogController : ControllerBase
    {
        private readonly IUserLogService _service;

        public UserLogController(IUserLogService service) => _service = service;

        //[HttpGet]
        //[ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseModel<PagedResult<UserLog>>))]
        //public async Task<IActionResult> GetAll([FromQuery] UserLogSearchRequest request, CancellationToken ct)
        //{
        //    var data = await _service.GetAllUserLogAsync(request, ct);
        //    return Ok(ResponseModel<PagedResult<UserLog>>.Ok(
        //        data: data,
        //        message: "Get user logs successfully"));
        //}
        [HttpGet("by-user/{actorUserId:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseModel<PagedResult<UserLog>>))]
        public async Task<IActionResult> GetByUser([FromRoute] Guid actorUserId, [FromQuery] UserLogSearchRequest request, CancellationToken ct)
        {
            var data = await _service.GetUserLogByIdAsync(actorUserId, request, ct);
            return Ok(ResponseModel<PagedResult<UserLog>>.Ok(
                data: data,
                message: "Get user logs by user successfully"));
        }

    }
}
