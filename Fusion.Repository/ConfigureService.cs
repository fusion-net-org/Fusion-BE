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
            services.AddScoped<IRoleAdminRepository, RoleAdminRepository>();
            services.AddScoped<IUserRoleRepository, UserRoleRepository>();

            //ticket
            services.AddScoped<ITicketRepository, TicketRepository>();

            //company
            services.AddScoped<ICompanyRepository,CompanyRepository>();
            services.AddScoped<ICompanyMemberRepository, CompanyMemberRepository>();
            services.AddScoped<IPermissionQuery, PermissionQuery>();

            //task
            services.AddScoped<ITaskRepository, TaskRepository>();

            //comment
            services.AddScoped<ICommentRepository, CommentRepository>();

            //project request
            services.AddScoped<IProjectRequestRepository, ProjectRequestRepository>();

            //subscriptionpackage
            services.AddScoped<ISubscriptionRepository, SubscriptionRepository>();

            // transaction payment
            services.AddScoped<ITransactionPaymentRepository, TransactionPaymentRepository>();

            //usersubscrption
            services.AddScoped<IUserSubscriptionRepository, UserSubscriptionRepository>();

            //refresh token
            services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();

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
