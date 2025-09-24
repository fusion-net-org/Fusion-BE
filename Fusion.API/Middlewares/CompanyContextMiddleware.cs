using Fusion.API.Context;
using Fusion.Repository.Repositories;   // IPermissionQuery
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using System.Security.Claims;

namespace Fusion.API.Auth
{
    public sealed class CompanyContextMiddleware
    {
        private readonly RequestDelegate _next;
        public CompanyContextMiddleware(RequestDelegate next) => _next = next;

        public async Task Invoke(HttpContext ctx, IPermissionQuery permQuery, IMemoryCache cache)
        {
            if (ctx.User?.Identity?.IsAuthenticated != true)
            {
                await _next(ctx); return;
            }

            // userId từ "sub" hoặc NameIdentifier
            var sub = ctx.User.FindFirstValue("sub") ?? ctx.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(sub, out var userId)) { await _next(ctx); return; }

            // companyId: ưu tiên route → header X-Company-Id
            Guid? companyId = null;
            if (ctx.Request.RouteValues.TryGetValue("companyId", out var rv) &&
                Guid.TryParse(rv?.ToString(), out var routeCid))
                companyId = routeCid;
            else if (ctx.Request.Headers.TryGetValue("X-Company-Id", out var hv) &&
                     Guid.TryParse(hv.ToString(), out var headerCid))
                companyId = headerCid;

            var isSysAdmin = ctx.User.HasClaim("is_sys_admin", "1") || ctx.User.IsInRole("SystemAdmin");

            var cc = new CompanyContext { UserId = userId, CurrentCompanyId = companyId, IsSystemAdmin = isSysAdmin };

            if (companyId is Guid cid && !isSysAdmin)
            {
                var key = $"perm:{userId}:{cid}";
                if (!cache.TryGetValue(key, out HashSet<string>? codes))
                {
                    codes = await permQuery.GetEffectivePermissionsAsync(cid, userId, ctx.RequestAborted);
                    cache.Set(key, codes, TimeSpan.FromMinutes(10));
                }
                foreach (var code in codes!) cc.Permissions.Add(code);
            }

            ctx.Items[nameof(CompanyContext)] = cc;
            await _next(ctx);
        }
    }
}
