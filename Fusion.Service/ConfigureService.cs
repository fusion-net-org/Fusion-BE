using Fusion.Repository.IRepositories;
using Fusion.Repository.Repositories;
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

            //task
            services.AddScoped<ITaskService, TaskService>();

			//ticket
			services.AddScoped<ITicketService, TicketService>();

            //comment
            services.AddScoped<ICommentService, CommentService>();

            //project request
            services.AddScoped<IProjectRequestService , ProjectRequestService>();

            //Subscription package
            services.AddScoped<ISubscriptionPackageService, SubscriptionPackageService>();

            //transaction payment
            services.AddScoped<ITransactionPaymentService, TransactionPaymentService>();

            //refesh token
            services.AddScoped<IRefreshTokenService, RefreshTokenService>();

            //user subscrption
            services.AddScoped<IUserSubscriptionService, UserSubscriptionService>();

            //notification
            services.AddScoped<INotificationService, NotificationService>();

            //firebase cloud message
            services.AddScoped<IFcmService, FcmService>();

            //user device
            services.AddScoped<IUserDeviceService, UserDeviceService>();

            return services;
        }
     }
}
