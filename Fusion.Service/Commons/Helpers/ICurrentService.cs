

using Microsoft.AspNetCore.Http;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Fusion.Service.Commons.Helpers
{
    public interface ICurrentService
    {
        Guid GetUserId();
        string? GetUserEmail();
        bool IsAdmin();
        bool HasPermission(string code);
    }

    public class CurrentService : ICurrentService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        public CurrentService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }
        public Guid GetUserId()
        {
            var userIdClaim = _httpContextAccessor.HttpContext?.User?
                .FindFirstValue(ClaimTypes.NameIdentifier)
                ??_httpContextAccessor.HttpContext?.User ?
                .FindFirstValue(JwtRegisteredClaimNames.Sub);
            return userIdClaim != null ? Guid.Parse(userIdClaim) : Guid.Empty;
        }
        public string? GetUserEmail()
        {
            return _httpContextAccessor.HttpContext?.User?
               .FindFirstValue(ClaimTypes.Email)
               ?? _httpContextAccessor.HttpContext?.User?
               .FindFirstValue(JwtRegisteredClaimNames.Email);
        }
        public bool IsAdmin()
        {
            var roleClaim = _httpContextAccessor.HttpContext?.User?.FindFirst("Admin");
            return roleClaim != null && roleClaim.Value == "true";
        }
        public bool HasPermission(string code)
        {
            var u = _httpContextAccessor.HttpContext?.User;
            if (u == null) return false;

            return u.Claims.Any(c =>
                (c.Type == "permission" || c.Type == "permissions" || c.Type == "perm") &&
                string.Equals(c.Value, code, StringComparison.OrdinalIgnoreCase));
        }
    }
}
