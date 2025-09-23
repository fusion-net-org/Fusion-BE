

namespace Fusion.Service.ViewModels.Users.Requests;

public record RegisterRequest
(
    string FirstName,
    string LastName,
    string Email,
    string Password,
    string ConfirmPassword
);
