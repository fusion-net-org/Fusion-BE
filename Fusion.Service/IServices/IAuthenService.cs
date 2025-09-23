
using Fusion.Service.Commons.BaseResponses;
using Fusion.Service.ViewModels.Users.Requests;
using Fusion.Service.ViewModels.Users.Responses;

namespace Fusion.Service.IServices;

public interface IAuthenService
{
    Task<ResponseModel<string>> RegisterAsync(RegisterRequest request);
    Task<ResponseModel<LoginResponse>> LoginAsync(LoginRequest request);
}
