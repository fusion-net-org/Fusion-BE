using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fusion.Repository
{
    public static class ConfigureService
    {
        public static IServiceCollection ConfigureRepositoryLayerService(this IServiceCollection services, IConfiguration configuration1configuration)
        {
            return services;
        }

    }
}
