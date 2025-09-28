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

            //register service entities
            services.AddScoped<IAuthenService, AuthenService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<ICloudinaryService, CloudinaryService>();
            services.AddScoped<IJwtService, JwtService>();

            // register other services
            services.AddScoped<ICurrentService, CurrentService>();
            services.AddScoped<ICloudinaryService, CloudinaryService>();
            services.AddScoped<IMailService, MailService>();
            //partner
            services.AddScoped<ICompanyFriendshipService,CompanyFriendshipService>();
            //company
            services.AddScoped<ICompanyService, CompanyService>();
            services.AddScoped<ICompanyMemberService, CompanyMemberService>();
            return services;
        }
     }
}
