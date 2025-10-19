

namespace Fusion.Service.ViewModels.Users.Requests;

public class ResetPasswordConfirmRequest
{
    public string Token { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
}
