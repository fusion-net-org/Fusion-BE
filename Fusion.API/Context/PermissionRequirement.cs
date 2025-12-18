using Microsoft.AspNetCore.Authorization;

namespace Fusion.API.Context
{
    public sealed class PermissionRequirement : IAuthorizationRequirement
    {
        public string Code { get; }
        public PermissionRequirement(string code) => Code = code;
    }

    public sealed class PermissionHandler : AuthorizationHandler<PermissionRequirement>
    {
        private readonly IHttpContextAccessor _http;
        public PermissionHandler(IHttpContextAccessor http) => _http = http;

        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement req)
        {
            var cc = _http.HttpContext?.Items[nameof(CompanyContext)] as CompanyContext;
            if (cc == null) return Task.CompletedTask;

            if (cc.IsSystemAdmin) { context.Succeed(req); return Task.CompletedTask; }

            if (cc.CurrentCompanyId != null && cc.Permissions.Contains(req.Code))
                context.Succeed(req);

            return Task.CompletedTask;
        }
    }
}
