using Fusion.Repository.Bases.Page;
using Fusion.Repository.Bases.Page.User;
using Fusion.Repository.Entities;
using Fusion.Service.Commons.BaseResponses;
using Fusion.Service.IServices;
using Fusion.Service.ViewModels.Users.Requests;
using Fusion.Service.ViewModels.Users.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Fusion.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseModel<SelfUserResponse>))]
        public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
        {
            var user = await _userService.GetByIdAsync(id, cancellationToken);
            return Ok(ResponseModel<SelfUserResponse>.Ok(
                data: user,
                message: "Get user successfully"));
        }

        [HttpGet("paged-admin")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseModel<PagedResult<AdminUserResponse>>))]
        public async Task<IActionResult> GetUserAdminPaged([FromQuery] AdminUserPagedRequest request, CancellationToken cancellationToken)
        {
            var result = await _userService.GetPagedAdminUsersAsync(request, cancellationToken);
            return Ok(ResponseModel<PagedResult<AdminUserResponse>>.Ok(
                data: result,
                message: "Get paged users successfully"));
        }

        [HttpGet("self-user")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseModel<SelfUserResponse>))]
        public async Task<IActionResult> GetSelfUser(CancellationToken cancellationToken)
        {
            var result = await _userService.GetSelfUserAsync(cancellationToken);
            return Ok(ResponseModel<SelfUserResponse>.Ok(
                data: result,
                message: "Get self user successfully"));
        }

        [HttpPut("self-user")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseModel<SelfUserResponse>))]
        public async Task<IActionResult> UpdateSelfUser([FromForm]UpdateSelfUserRequest request, CancellationToken cancellationToken)
        {
            var result = await _userService.UpdateSelfUserAsync(request, cancellationToken);
            return Ok(ResponseModel<SelfUserResponse>.Ok(
                data: result,
                message: "Update self user successfully"));
        }

        //[HttpGet("paged-company")]
        //[ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseModel<PagedResult<CompanyUserResponse>>))]
        //public async Task<IActionResult> GetUserCompanyPaged([FromQuery] AdminUserPagedRequest request, CancellationToken cancellationToken)
        //{
        //    var result = await _userService.GetPagedAdminUsersAsync(request, cancellationToken);
        //    return Ok(ResponseModel<PagedResult<AdminUserResponse>>.Ok(
        //        data: result,
        //        message: "Get paged users successfully"));
        //}

        [HttpGet("get-all")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseModel<PagedResult<SelfUserResponse>>))]
        public async Task<IActionResult> GetAllUsersPaged([FromQuery] PagedRequest request, CancellationToken cancellationToken)
        {
            var result = await _userService.GetAllUsersAsync(request, cancellationToken);
            return Ok(ResponseModel<PagedResult<SelfUserResponse>>.Ok(
                data: result,
                message: "Get all users successfully"));
        }

        [HttpGet("owner-user/{companyId:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseModel<Guid>))]
        public async Task<IActionResult> GetOwnerUserIdByCompanyId(Guid companyId, CancellationToken cancellationToken)
        {
            var ownerId = await _userService.GetOwnerUserIdByCompanyIdAsync(companyId, cancellationToken);
            return Ok(ResponseModel<Guid>.Ok(
                data: ownerId.Value,
                message: "Get owner user id by company successfully"));
        }

    }
}
