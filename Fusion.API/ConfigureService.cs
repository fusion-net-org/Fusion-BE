using System.Text.RegularExpressions;

namespace Fusion.API
{
    public static class ConfigureService
    {
        public static IServiceCollection ConfigureApiLayerServices(this IServiceCollection services, IConfiguration configuration)
        {
            return services;
        }
         public static void ConfigCors(this IServiceCollection services)
        {
        }
        public static void AddAuthenJwt(this IServiceCollection services, IConfiguration configuration)
        {

        }
        public class CamelCaseParameterTransformer : IOutboundParameterTransformer
        {
            public string? TransformOutbound(object? value)
            {
                if (value == null) return null;

                // Convert to camelCase
                string input = value.ToString()!;
                return Regex.Replace(input, "(?<!^)([A-Z])", "-$1").ToLowerInvariant();
            }
        }
    }
}
