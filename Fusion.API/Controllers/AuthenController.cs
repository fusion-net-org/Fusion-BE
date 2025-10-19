
using Fusion.Repository.Bases.Responses;
using Fusion.Service.Commons.BaseResponses;
using Fusion.Service.IServices;
using Fusion.Service.ViewModels.Users.Requests;
using Fusion.Service.ViewModels.Users.Responses;
using Microsoft.AspNetCore.Mvc;

namespace Fusion.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenController : ControllerBase
    {
        private readonly IAuthenService _authenService;
        public AuthenController(IAuthenService authenService)
        {
            _authenService = authenService;
        }

        [HttpPost("register")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseModel<bool>))]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request, CancellationToken cancellationToken)
        {
            // FluentValidation automatically validates the request 
            var result = await _authenService.RegisterAsync(request, cancellationToken);
            return Ok(ResponseModel<bool>.Ok(
                     data: result,
                     message: ResponseMessageHelper.FormatMessage(ResponseMessages.CREATE_SUCCESS, "user")));
        }

        [HttpPost("login")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseModel<LoginResponse>))]
        public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
        {
            var result = await _authenService.LoginAsync(request, cancellationToken);
            return Ok(ResponseModel<LoginResponse>.Ok(
                      data: result,
                      message: "Login successfully"));
        }

        [HttpPost("login-google")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseModel<LoginResponse>))]
        public async Task<IActionResult> GoogleLoginAsync([FromBody] GoogleLoginRequest request, CancellationToken cancellationToken)
        {
            var result = await _authenService.GoogleLoginAsync(request, cancellationToken);

            return Ok(ResponseModel<LoginResponse>.Ok(
                data: result,
                message: "Login with Google successfully"
            ));
        }


        [HttpPost("request-password-reset")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseModel<bool>))]
        public async Task<IActionResult> RequestPasswordReset([FromBody] PasswordResetRequest model, CancellationToken cancellationToken)
        {
            var result = await _authenService.RequestPasswordResetAsync(model.Email, cancellationToken);
            return Ok(ResponseModel<bool>.Ok(result, "Password reset link sent successfully"));
        }

        [HttpPost("reset-password")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseModel<bool>))]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordConfirmRequest model, CancellationToken cancellationToken)
        {
            var result = await _authenService.ResetPasswordAsync(model.Token, model.NewPassword, cancellationToken);
            return Ok(ResponseModel<bool>.Ok(result, "Password reset successfully"));
        }

    }
}
