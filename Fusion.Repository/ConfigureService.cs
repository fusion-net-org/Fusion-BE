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

            services.AddScoped<IWorkflowDesignerRepository, WorkflowDesignerRepository>();
            //user
            services.AddScoped<IUserRepository, UserRepository>();

            //partner
            services.AddScoped<ICompanyFriendshipRepository, CompanyFriendshipRepository>();
            services.AddScoped<IRoleAdminRepository, RoleAdminRepository>();
            services.AddScoped<IUserRoleRepository, UserRoleRepository>();
            services.AddScoped<ISprintRepository, SprintRepository>();
            //ticket
            services.AddScoped<ITicketRepository, TicketRepository>();

            //company
            services.AddScoped<ICompanyRepository,CompanyRepository>();
            services.AddScoped<ICompanyMemberRepository, CompanyMemberRepository>();
            services.AddScoped<IPermissionQuery, PermissionQuery>();
            services.AddScoped<IProjectBoardRepository, ProjectBoardRepository>();
            //task
            services.AddScoped<ITaskRepository, TaskRepository>();
            //checklist
            services.AddScoped<ITaskChecklistRepository, TaskChecklistRepository>();
            services.AddScoped<ITaskWorkflowRepository, TaskWorkflowRepository>();

            //comment
            services.AddScoped<ICommentRepository, CommentRepository>();

            //project request
            services.AddScoped<IProjectRequestRepository, ProjectRequestRepository>();

            //project member
            services.AddScoped<IProjectMemberRepository, ProjectMemberRepository>();

            //feature catalog
            services.AddScoped<IFeatureCatalogRepository, FeatureCatalogRepository>();

            //subscriptionplan
            services.AddScoped<ISubscriptionPlanRepository, SubscriptionPlanRepository>();

            // transaction payment
            services.AddScoped<ITransactionPaymentRepository, TransactionPaymentRepository>();

            //usersubscrption
            services.AddScoped<IUserSubscriptionRepository, UserSubscriptionRepository>();


            //refresh token
            services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();

            //notification
            services.AddScoped<INotificationRepository, NotificationRepository>();

            //user device
            services.AddScoped<IUserDeviceRepository, UserDeviceRepository>();

            //activity log 
            services.AddScoped<ICompanyActivityLogRepository, CompanyActivityLogRepository>();

            //project
            services.AddScoped<IProjectRepository, ProjectRepository>();

            //user log
            services.AddScoped<IUserLogRepository, UserLogRepository>();

            //User setting
            services.AddScoped<IUserNotificationSettingRepository , UserNotificationSettingRepository>();

            //Contract
            services.AddScoped<IContractRepository, ContractRepository>();
            services.AddScoped<IContractAppendixRepository, ContractAppendixRepository>();

            //workflow status
            services.AddScoped<IWorkflowStatusRepository, WorkflowStatusRepository>();

            // company subscription
            services.AddScoped<ICompanySubscriptionRepository, CompanySubscriptionRepository>();

            //TicketCOmment
            services.AddScoped<ITicketCommentRepository, TicketCommentRepository>();
            // company entry
            services.AddScoped<ICompanySubscriptionEntryRepository, CompanySubscriptionEntryRepository>();
            services.AddScoped<ITaskLogEventRepository, TaskLogEventRepository>();

            //roles
            services.AddScoped<IRoleRepository, RoleRepository>();
            services.AddScoped<IRbacBootstrapper, RbacBootstrapper>();

            //friend 
            services.AddScoped<IUserFriendshipRepository, UserFriendshipRepository>();

            //Chat
            services.AddScoped<IChatConversationRepository, ChatConversationRepository>();
            services.AddScoped<IChatMessageRepository, ChatMessageRepository>();
            services.AddScoped<IChatConversationMemberRepository, ChatConversationMemberRepository>();

            //components
            services.AddScoped<IProjectComponentRepository, ProjectComponentRepository>();

            return services;

        }
        public static void AddDatabase(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<FusionDbContext>(options =>
                    options.UseSqlServer(
                           configuration.GetConnectionString("DefaultConnection"),
                           sqlOptions => sqlOptions.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery)
                    )
         );
        }
    }
}
