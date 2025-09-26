using Fusion.Repository.Bases.Page;
using Fusion.Repository.Bases.Page.User;
using Fusion.Repository.Entities;
using Fusion.Service.Commons.BaseResponses;
using Fusion.Service.IServices;
using Fusion.Service.ViewModels.Users.Responses;
using Microsoft.AspNetCore.Mvc;

namespace Fusion.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseModel<User>))]
        public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
        {
            var user = await _userService.GetByIdAsync(id, cancellationToken);
            return Ok(ResponseModel<User>.Ok(
                data: user,
                message: "Get user successfully"));
        }

        [HttpGet("paged")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseModel<PagedResult<UserPageResponse>>))]
        public async Task<IActionResult> GetPaged([FromQuery] UserPagedRequest request, CancellationToken cancellationToken)
        {
            var result = await _userService.GetPagedUsersAsync(request, cancellationToken);
            return Ok(ResponseModel<PagedResult<UserPageResponse>>.Ok(
                data: result,
                message: "Get paged users successfully"));
        }
    }
}
