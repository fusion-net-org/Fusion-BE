
using Fusion.Repository.Bases.Responses;
using Fusion.Service.Commons.BaseResponses;
using Fusion.Service.IServices;
using Fusion.Service.ViewModels.Users.Requests;
using Fusion.Service.ViewModels.Users.Responses;
using Microsoft.AspNetCore.Authentication;
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
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            // FluentValidation automatically validates the request
            var result = await _authenService.RegisterAsync(request);
            return Ok(ResponseModel<bool>.OkResponseModel(
                     data: result,
                     message: ResponseMessageHelper.FormatMessage(ResponseMessages.CREATE_SUCCESS, "user")));
        }

        [HttpPost("login")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseModel<LoginResponse>))]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var result = await _authenService.LoginAsync(request);
            return Ok(ResponseModel<LoginResponse>.OkResponseModel(
                      data: result,
                      message: "Login successfully" ));
        }

        [HttpPost("google-login")]
        public IActionResult GoogleLogin(string returnUrl = "/")
        {
            var properties = new AuthenticationProperties { RedirectUri = Url.Action("GoogleResponse", new { returnUrl }) };
            return Challenge(properties, "Google");
        }
    }
}
