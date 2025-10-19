using Fusion.Repository.Bases.Responses;
using Fusion.Service.Commons.BaseResponses;
using Fusion.Service.IServices;
using Fusion.Service.ViewModels.RefreshToken.Requests;
using Fusion.Service.ViewModels.RefreshToken.Responses;
using Microsoft.AspNetCore.Mvc;

namespace Fusion.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]

    public class RefreshTokenController : ControllerBase
    {
        private readonly IRefreshTokenService _refreshTokenService;

        public RefreshTokenController(IRefreshTokenService refreshTokenService)
        {
            _refreshTokenService = refreshTokenService;
        }

        /// <summary>
        /// Làm mới AccessToken và RefreshToken (refresh flow).
        /// </summary>
        [HttpPost("refresh")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseModel<TokenResponse>))]
        public async Task<IActionResult> RefreshTokenAsync([FromBody] RefreshTokenRequest request, CancellationToken cancellationToken)
        {
            var result = await _refreshTokenService.RotateRefreshTokenAsync(request.RefreshToken, cancellationToken);

            var response = new TokenResponse
            {
                AccessToken = result.AccessToken,
                RefreshToken = result.RefreshToken
            };

            return Ok(ResponseModel<TokenResponse>.Ok(
                data: response,
                message: ResponseMessageHelper.FormatMessage(ResponseMessages.REFRESH_TOKEN_SUCCESS)
            ));
        }

        /// <summary>
        /// Thu hồi (revoke) một refresh token.
        /// </summary>
        [HttpPost("revoke")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseModel<object>))]
        public async Task<IActionResult> RevokeTokenAsync([FromBody] RefreshTokenRequest request, CancellationToken cancellationToken)
        {
            await _refreshTokenService.RevokeTokenAsync(request.RefreshToken, cancellationToken);

            return Ok(ResponseModel<object>.Ok(
                data: null,
                message: ResponseMessageHelper.FormatMessage(ResponseMessages.REVOKE_TOKEN_SUCCESS)
            ));
        }

        /// <summary>
        /// Dọn dẹp các refresh token đã hết hạn (chỉ admin hoặc background job nên gọi).
        /// </summary>
        
        [HttpDelete("cleanup")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseModel<object>))]
        public async Task<IActionResult> CleanUpExpiredTokensAsync(CancellationToken cancellationToken)
        {
            await _refreshTokenService.CleanUpExpiredTokensAsync(cancellationToken);

            return Ok(ResponseModel<object>.Ok(
                data: null,
                message: ResponseMessageHelper.FormatMessage(ResponseMessages.CLEANUP_TOKEN_SUCCESS)
            ));
        }
    }
}