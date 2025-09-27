

namespace Fusion.Service.ViewModels.Users.Responses;

public record CompanyUserResponse
{
    public string UserName { get; init; }
    public string? Email { get; init; }
    public string? Phone { get; init; }
    public string? Address { get; init; }
    public string? Gender { get; init; }
}
