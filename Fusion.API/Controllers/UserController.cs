using System.Security.Claims;
using Fusion.Repository.Bases.Page;
using Fusion.Repository.Bases.Page.User;
using Fusion.Repository.Bases.Page.UserLog;
using Fusion.Repository.Entities;
using Fusion.Repository.ViewModels.Users;
using Fusion.Service.Commons.BaseResponses;
using Fusion.Service.IServices;
using Fusion.Service.Services;
using Fusion.Service.ViewModels.Users.Requests;
using Fusion.Service.ViewModels.Users.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Fusion.Service.ViewModels.UserLog.Responses;

namespace Fusion.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IUserLogService _userLogService;

        public UserController(IUserService userService, IUserLogService userLogService)
        {
            _userService = userService;
            _userLogService = userLogService;
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


        [Authorize(Roles = "Admin")]
        [HttpPut("{id:guid}/self-user-admin")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseModel<SelfUserResponse>))]
        public async Task<IActionResult> UpdateSelfUserByAdmin(
            [FromRoute] Guid id,
            [FromForm] UpdateSelfUserRequest request,
            CancellationToken cancellationToken)
        {
            var result = await _userService.UpdateSelfUserByAdminAsync(id, request, cancellationToken);
            return Ok(ResponseModel<SelfUserResponse>.Ok(
                data: result,
                message: "Update self user successfully"));
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("{id:guid}/update-status-admin")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseModel<SelfUserResponse>))]
        public async Task<IActionResult> UpdateStatusByAdmin(
            [FromRoute] Guid id,
            [FromForm] bool status,
            CancellationToken cancellationToken)
        {
            var result = await _userService.UpdateStatus(id, status, cancellationToken);
            return Ok(ResponseModel<SelfUserResponse>.Ok(
                data: result,
                message: "Update self user successfully"));
        }
        [HttpPost("change-password")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseModel<bool>))]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest model, CancellationToken cancellationToken)
        {
            var result = await _userService.ChangePasswordAsync(model, cancellationToken);
            return Ok(ResponseModel<bool>.Ok(result, "Change password successfully"));
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
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseModel<SelfUserResponse>))]
        public async Task<IActionResult> GetOwnerUserByCompanyId(Guid companyId, CancellationToken cancellationToken)
        {
            var owner = await _userService.GetOwnerUserByCompanyIdAsync(companyId, cancellationToken);
            return Ok(ResponseModel<SelfUserResponse>.Ok(
                data: owner,
                message: "Get owner user by company successfully"));
        }

        [HttpGet("fullInfor/{id:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseModel<User>))]
        public async Task<IActionResult> GetFullInfoById(Guid id, CancellationToken cancellationToken)
        {
            var user = await _userService.GetFullInfoByIdAsync(id, cancellationToken);
            return Ok(ResponseModel<User>.Ok(
                data: user,
                message: "Get user successfully"));
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("count-status-user")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseModel<UserStatusResponse>))]
        public async Task<IActionResult> GetCountUserByStatus(
           CancellationToken cancellationToken)
        {
            var result = await _userService.GetCountUserByStatusAsync( cancellationToken);
            return Ok(ResponseModel<UserStatusResponse>.Ok(
                data: result,
                message: "Get user with status successfully"));
        }


        [Authorize(Roles = "Admin")]
        [HttpGet("overview/growth-and-status")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseModel<UserGrowthAndStatusOverviewResponse>))]
        public async Task<IActionResult> GetGrowthAndStatusOverview( [FromQuery] int months = 12,CancellationToken cancellationToken = default)
        {
            var result = await _userService.GetUserGrowthAndStatusOverviewAsync(months, cancellationToken);

            // Tuỳ theo kiểu ApiResponse bạn đang dùng
            return Ok(ResponseModel<UserGrowthAndStatusOverviewResponse>.Ok(
            data: result,
            message: "Get user growth and status successfully"));
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("overview/company-distribution")]
        [ProducesResponseType(StatusCodes.Status200OK,Type = typeof(ResponseModel<List<UserCompanyDistributionPoint>>))]
        public async Task<IActionResult> GetCompanyUserDistribution([FromQuery] int top = 10, CancellationToken cancellationToken = default)
        {
            var data = await _userService.GetTopCompaniesByUserCountAsync(top, cancellationToken);

            return Ok(ResponseModel<List<UserCompanyDistributionPoint>>.Ok(
                data: data,
                message: "Get user distribution by company successfully"));
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("overview/permission-levels")]
        [ProducesResponseType(StatusCodes.Status200OK,Type = typeof(ResponseModel<UserPermissionLevelOverviewResponse>))]
        public async Task<IActionResult> GetPermissionLevelOverview(CancellationToken cancellationToken = default)
        {
            var result = await _userService.GetUserPermissionLevelOverviewAsync(cancellationToken);

            return Ok(ResponseModel<UserPermissionLevelOverviewResponse>.Ok(
                data: result,
                message: "Get user permission level overview successfully"));
        }

        [HttpGet("roles/{companyId:guid}/{userId:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseModel<List<RoleDto>>))]
        public async Task<IActionResult> GetUserRolesByCompany(Guid companyId, Guid userId, CancellationToken cancellationToken)
        {
            if (userId == Guid.Empty)
            {
                return BadRequest(ResponseModel<string>.Error(StatusCodes.Status400BadRequest, "UserId is required!"));
            }

            var roles = await _userService.GetRolesByUserAndCompanyAsync(userId, companyId, cancellationToken);

            return Ok(ResponseModel<List<RoleDto>>.Ok(
                data: roles,
                message: "Get user roles by company successfully"));
        }



        [Authorize]
        [HttpGet("analytics")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseModel<AnalyticsUserResponse>))]
        public async Task<IActionResult> GetAnalyticsUser(CancellationToken token)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized(new { Message = "Invalid or missing token" });
            }

            var result = await _userService.GetAnalyticsUserAsync(userId, token);

            return Ok(ResponseModel<AnalyticsUserResponse>.Ok(
                data: result,
                message: "Get user analytics successfully"));
        }

        [Authorize]
        [HttpGet("log")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseModel<PagedResult<UserLogResponse>>))]
        public async Task<IActionResult> GetLogByUser(
            [FromQuery] UserLogSearchRequest request, CancellationToken ct)
        {

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized(new { Message = "Invalid or missing token" });
            }

            var data = await _userLogService.GetUserLogByUserIdAsync(userId, request, ct);
            return Ok(ResponseModel<PagedResult<UserLogResponse>>.Ok(
                data: data,
                message: "Get user logs by user successfully"));
        }
    }
}
