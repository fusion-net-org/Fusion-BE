using Fusion.Repository.Entities;
using Fusion.Service.ViewModels.RefreshToken.Responses;

namespace Fusion.Service.IServices;

public interface IRefreshTokenService
{
    Task<RefreshToken> ValidateRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default);
    Task<(string AccessToken, string RefreshToken)> RotateRefreshTokenAsync(string oldRefreshToken, CancellationToken cancellationToken = default);
    Task<bool> RevokeTokenAsync(string refreshToken, CancellationToken cancellationToken = default);
    Task<bool> CleanUpExpiredTokensAsync(CancellationToken cancellationToken = default);
}