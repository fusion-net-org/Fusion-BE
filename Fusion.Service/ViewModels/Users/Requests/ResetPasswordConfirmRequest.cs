

namespace Fusion.Service.ViewModels.Users.Requests;

public class ResetPasswordConfirmRequest
{
    public string ResetToken { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
    public string ConfirmPassword { get; set; } = string.Empty;
}
