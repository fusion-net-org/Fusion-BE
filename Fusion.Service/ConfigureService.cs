using Fusion.Service.Commons.Helpers;
using Fusion.Service.IServices;
using Fusion.Service.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Fusion.Service
{
    public static class ConfigureService
    {
        public static IServiceCollection ConfigureServiceLayerService(this IServiceCollection services, IConfiguration configuration)
        {
            // register autoMapper
            services.AddAutoMapper(typeof(MappingProfile));
            services.AddScoped<IRoleAdminService, RoleAdminService>();
            services.AddScoped<IMemberRoleService, MemberRoleService>();

            //register service entities
            services.AddScoped<IAuthenService, AuthenService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IJwtService, JwtService>();
            services.AddScoped<IMailService, MailService>();
            //partner
            services.AddScoped<ICompanyFriendshipService,CompanyFriendshipService>();
            //company
            services.AddScoped<ICompanyService, CompanyService>();
            return services;
        }
     }
}
