
using Microsoft.AspNetCore.Http;

namespace Fusion.Service.ViewModels.Users.Requests;

public record UpdateSelfUserRequest
(
    IFormFile? Avatar,
    string? Phone,
    string? Address,
    string? Gender
);

