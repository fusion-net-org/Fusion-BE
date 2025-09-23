

using Fusion.Repository.Entities;

namespace Fusion.Service.IServices;

public interface IJwtService
{
    Task<(string AccessToken, string RefreshToken)> GenerateTokensAsync(User user);
}
