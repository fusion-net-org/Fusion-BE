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
            services.AddScoped<IRoleAdminService, RoleAdminService>();
            services.AddScoped<IMemberRoleService, MemberRoleService>();
            services.AddScoped<ISprintService, SprintService>();
            //register service entities
            services.AddScoped<IAuthenService, AuthenService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<ICloudinaryService, CloudinaryService>();
            services.AddScoped<IJwtService, JwtService>();
            services.AddScoped<IWorkflowDesignerService, WorkflowDesignerService>();
            // register other services
            services.AddScoped<ICurrentService, CurrentService>();
            services.AddScoped<ICloudinaryService, CloudinaryService>();
            services.AddScoped<IMailService, MailService>();
            //partner
            services.AddScoped<ICompanyFriendshipService,CompanyFriendshipService>();
            //company
            services.AddScoped<ICompanyService, CompanyService>();
            services.AddScoped<ICompanyMemberService, CompanyMemberService>();
            services.AddScoped<IProjectBoardService, ProjectBoardService>();

            //task
            services.AddScoped<ITaskService, TaskService>();

			//ticket
			services.AddScoped<ITicketService, TicketService>();

            //comment
            services.AddScoped<ICommentService, CommentService>();

            //project request
            services.AddScoped<IProjectRequestService , ProjectRequestService>();

            //Subscription plan
            services.AddScoped<ISubscriptionPlanService, SubscriptionPlanService>();

            //transaction payment
            services.AddScoped<ITransactionPaymentService, TransactionPaymentService>();

            //refesh token
            services.AddScoped<IRefreshTokenService, RefreshTokenService>();

            //user subscrption
            //services.AddScoped<IUserSubscriptionService, UserSubscriptionService>();

            //notification
            services.AddScoped<INotificationService, NotificationService>();

            //firebase cloud message
            services.AddScoped<IFcmService, FcmService>();

            //user device
            services.AddScoped<IUserDeviceService, UserDeviceService>();

            // company activity log
            services.AddScoped<ICompanyActivityService, CompanyActivityLogService>();

            // project
            services.AddScoped<IProjectService, ProjectService>();

            // project member
            services.AddScoped<IProjectMemberService, ProjectMemberService>();

            // admin
            //services.AddScoped<IAdminService, AdminService>();

            //userlog 
            services.AddScoped<IUserLogService, UserLogService>();

            //contract
            services.AddScoped<IContractService, ContractService>();

            // PayOS
            services.AddSingleton<PayOS>(sp =>
            {
                var clientId = configuration["PayOS:ClientId"];
                var apiKey = configuration["PayOS:ApiKey"];
                var checksumKey = configuration["PayOS:ChecksumKey"];

                if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(apiKey) || string.IsNullOrEmpty(checksumKey))
                {
                    throw new InvalidOperationException("Missing PayOS configuration in appsettings.json");
                }

                return new PayOS(clientId, apiKey, checksumKey);
            });
            services.AddScoped<IPayOSService, PayOSService>();

            return services;
        }
     }
}
