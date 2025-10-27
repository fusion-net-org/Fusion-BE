
using Fusion.Service.ViewModels.Users.Requests;
using Fusion.Service.ViewModels.Users.Responses;

namespace Fusion.Service.IServices;

public interface IAuthenService
{
    Task<bool> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default);
    Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);
    Task<LoginResponse> GoogleLoginAsync(GoogleLoginRequest request, CancellationToken cancellationToken = default);
    Task<bool> RequestPasswordResetAsync(string email, string device, CancellationToken cancellationToken);
    Task<bool> ResetPasswordAsync(string token, string newPassword, CancellationToken cancellationToken);
}
