using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Fusion.Repository.Data;

namespace Fusion.Repository
{
    public static class ConfigureService
    {
        public static IServiceCollection ConfigureRepositoryLayerService(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDatabase(configuration);

            return services;
        }
        public static void AddDatabase(this IServiceCollection services, IConfiguration configuration)
        {
            //services.AddDbContext<ApplicationDbContext>(options =>
            //{
            //    options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"));
            //});

            services.AddDbContext<FusionDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection")
            )
        );
        }
    }
}
