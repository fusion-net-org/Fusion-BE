using Fusion.Repository.Data;
using Fusion.Repository.IRepositories;
using Fusion.Repository.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Travelogue.Repository.Caching;

namespace Fusion.Repository
{
    public static class ConfigureService
    {
        public static IServiceCollection ConfigureRepositoryLayerService(this IServiceCollection services, IConfiguration configuration)
        {
            // register connection database
            services.AddDatabase(configuration);

            //cache
            services.AddScoped<ICacheService, CacheService>();

            // register repositories entites
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            //user
            services.AddScoped<IUserRepository, UserRepository>();

            //partner
            services.AddScoped<ICompanyFriendshipRepository, CompanyFriendshipRepository>();

            //ticket
            services.AddScoped<ITicketRepository, TicketRepository>();

            //company
            services.AddScoped<ICompanyRepository,CompanyRepository>();
            services.AddScoped<ICompanyMemberRepository, CompanyMemberRepository>();
            services.AddScoped<IPermissionQuery, PermissionQuery>();
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
