

namespace Fusion.Service.ViewModels.Users.Responses;

public record UserPageResponse
{
    public string? UserName { get; init; }
    public string? Email { get; init; }
    public string? Address { get; init; }
    public string? Gender { get; init; }
    public string? GoogleSub { get; init; }
    public bool Status { get; init; }
    public DateTime CreateAt { get; init; }
    public DateTime UpdateAt { get; init; }
}

