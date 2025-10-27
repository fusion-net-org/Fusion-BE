using Fusion.API.Context;
using Fusion.Service.Commons.BaseResponses;
using Fusion.Service.IServices;
using Fusion.Service.ViewModels.Companies.Responses;
using Fusion.Service.ViewModels.UserDevices.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Fusion.API.Controllers
{
    [Route("api/user-device")]
    [ApiController]
    public class UserDevicesController : ControllerBase
    {
        private readonly IUserDeviceService _deviceService;

        public UserDevicesController(IUserDeviceService deviceService)
        {
            _deviceService = deviceService;
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseModel<string>))]
        public async Task<IActionResult> Register([FromBody] RegisterUserDeviceRequest req, CancellationToken ct)
        {

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized(ResponseModel<string>.Error(
                    StatusCodes.Status401Unauthorized,
                    "Don't find token!"));
            }

            await _deviceService.RegisterAsync(userId, req.DeviceToken, req.Platform, req.DeviceName, ct);
            return Ok(ResponseModel<string>.Ok(
                data: null,
                message: "Device registered successfully"));
        }
    }
}
