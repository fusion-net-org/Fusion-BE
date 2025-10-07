using Fusion.API.Context;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Fusion.API.Auth
{
    public class DynamicPermissionPolicyProvider : IAuthorizationPolicyProvider
    {
        const string POLICY_PREFIX = "perm:";

        private readonly DefaultAuthorizationPolicyProvider _fallbackPolicyProvider;

        public DynamicPermissionPolicyProvider(IOptions<AuthorizationOptions> options)
        {
            _fallbackPolicyProvider = new DefaultAuthorizationPolicyProvider(options);
        }

        public Task<AuthorizationPolicy?> GetDefaultPolicyAsync()
            => _fallbackPolicyProvider.GetDefaultPolicyAsync();

        public Task<AuthorizationPolicy?> GetFallbackPolicyAsync()
            => _fallbackPolicyProvider.GetFallbackPolicyAsync();

        public Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
        {
            if (policyName.StartsWith(POLICY_PREFIX))
            {
                var code = policyName.Substring(POLICY_PREFIX.Length);
                var policy = new AuthorizationPolicyBuilder()
                    .AddRequirements(new PermissionRequirement(code))
                    .Build();

                return Task.FromResult<AuthorizationPolicy?>(policy);
            }

            return _fallbackPolicyProvider.GetPolicyAsync(policyName);
        }
    }
}
