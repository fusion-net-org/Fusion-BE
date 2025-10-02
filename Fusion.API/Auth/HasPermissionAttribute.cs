using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Fusion.API.Auth
{
    public class HasPermissionAttribute : AuthorizeAttribute
    {
        private const string POLICY_PREFIX = "perm:";

        public HasPermissionAttribute(string code)
        {
            Policy = $"{POLICY_PREFIX}{code}";
        }
    }
}
