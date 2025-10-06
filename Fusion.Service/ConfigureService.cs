using Fusion.Repository.IRepositories;
using Fusion.Repository.Repositories;
using Fusion.Service.Commons.Helpers;
using Fusion.Service.IServices;
using Fusion.Service.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Net.payOS;

namespace Fusion.Service
{
    public static class ConfigureService
    {
        public static IServiceCollection ConfigureServiceLayerService(this IServiceCollection services, IConfiguration configuration)
        {
            // register autoMapper
            services.AddAutoMapper(typeof(MappingProfile));

            //payOs
            services.AddScoped<PayOS>(sp =>
            {
                var config = configuration.GetSection("PayOS");
                var clientId = config["ClientId"];
                var apiKey = config["ApiKey"];
                var checksumKey = config["ChecksumKey"];

                return new PayOS(clientId, apiKey, checksumKey);
            });
            services.AddScoped<IPayOSService, PayOSService>();


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
			//ticket
			services.AddScoped<ITicketService, TicketService>();
            //Subscription package
            services.AddScoped<ISubscriptionPackageService, SubscriptionPackageService>();
            //transaction payment
            services.AddScoped<ITransactionPaymentService, TransactionPaymentService>();
            //refesh token
            services.AddScoped<IRefreshTokenService, RefreshTokenService>();
            //user subscrption
            services.AddScoped<IUserSubscriptionService, UserSubscriptionService>();

			return services;
        }
     }
}
