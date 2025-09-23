

namespace Fusion.Service.ViewModels.Users.Responses;

public record LoginResponse
{
    public string UserName { get; init; }
    public string AccessToken { get; init; }
    public string RefreshToken { get; init; }
}