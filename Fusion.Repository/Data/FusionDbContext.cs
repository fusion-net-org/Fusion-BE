
using Fusion.Repository.Entities;
using Fusion.Repository.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Fusion.Repository.Data;

public partial class FusionDbContext : DbContext
{
    public FusionDbContext(DbContextOptions<FusionDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Comment> Comments { get; set; }
    public virtual DbSet<Company> Companies { get; set; }
    public virtual DbSet<CompanyFriendship> CompanyFriendships { get; set; }
    public virtual DbSet<CompanyMember> CompanyMembers { get; set; }
    public virtual DbSet<FunctionInPage> FunctionInPages { get; set; }
    public virtual DbSet<Notification> Notifications { get; set; }
    public virtual DbSet<Project> Projects { get; set; }
    public virtual DbSet<ProjectMember> ProjectMembers { get; set; }
    public virtual DbSet<ProjectRequest> ProjectRequests { get; set; }
    public virtual DbSet<ProjectTask> ProjectTasks { get; set; }
    public virtual DbSet<Role> Roles { get; set; }
    public virtual DbSet<RolePermission> RolePermissions { get; set; }
    public virtual DbSet<Sprint> Sprints { get; set; }
    public virtual DbSet<TaskLogEvent> TaskLogEvents { get; set; }
    public virtual DbSet<TaskWorkflow> TaskWorkflows { get; set; }
    public virtual DbSet<RefreshToken> RefreshTokens { get; set; }
    public virtual DbSet<Ticket> Tickets { get; set; }
    public virtual DbSet<TicketComment> TicketComments { get; set; }
    public virtual DbSet<User> Users { get; set; }
    public virtual DbSet<UserRole> UserRoles { get; set; }
    public virtual DbSet<Workflow> Workflows { get; set; }
    public virtual DbSet<WorkflowStatus> WorkflowStatuses { get; set; }
    public virtual DbSet<WorkflowTransition> WorkflowTransitions { get; set; }
    public virtual DbSet<UserDevice> UserDevices { get; set; }
    public virtual DbSet<CompanyActivityLog> CompanyActivityLogs { get; set; }
    public virtual DbSet<UserLog> UserLogs { get; set; }
    public virtual DbSet<ProjectTaskAssignee> ProjectTaskAssignee { get; set; }
    public virtual DbSet<ProjectTaskDependency> ProjectTaskDependency { get; set; }
    public virtual DbSet<ProjectTaskChecklistItem> ProjectTaskChecklistItems { get; set; }
    public virtual DbSet<ProjectTaskAttachment> ProjectTaskAttachments { get; set; }

    public virtual DbSet<UserNotificationSetting> UserNotificationSettings { get; set; }
    // Subscription
    public virtual DbSet<SubscriptionPlan> SubscriptionPlans { get; set; }
    public virtual DbSet<SubscriptionPlanFeature> SubscriptionPlanFeatures { get; set; }
    public virtual DbSet<SubscriptionPlanPrice> SubscriptionPlanPrices { get; set; }
    public virtual DbSet<Contract> Contracts { get; set; }
    public virtual DbSet<ContractAppendix> ContractAppendices { get; set; }
    public virtual DbSet<Feature> Features { get; set; }

    public virtual DbSet<TransactionPayment> TransactionPayments { get; set; }
    public DbSet<UserSubscription> UserSubscriptions { get; set; } 
    public DbSet<UserSubscriptionEntitlement> UserSubscriptionEntitlements { get; set; }

    public DbSet<CompanySubscription> CompanySubscriptions { get; set; } = null!;
    public DbSet<CompanySubscriptionEntitlement> CompanySubscriptionEntitlements { get; set; } = null!;
    public virtual DbSet<CompanySubscriptionEntry> CompanySubscriptionEntries { get; set; } = null!;

    public virtual DbSet<SubscriptionPlanPriceDiscount> SubscriptionPlanPriceDiscounts { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var utcKindConverter = new ValueConverter<DateTime, DateTime>(
    v => v, 
    v => DateTime.SpecifyKind(v, DateTimeKind.Utc) 
);
        modelBuilder.Entity<Comment>(entity =>
        {
            entity.Property(e => e.CreateAt).HasDefaultValueSql("(sysutcdatetime())").HasConversion(utcKindConverter);

            entity.HasOne(d => d.AuthorUser).WithMany(p => p.Comments).HasConstraintName("FK_Comments_Author");

            entity.HasOne(d => d.Task).WithMany(p => p.Comments).HasConstraintName("FK_Comments_Task");
        });

        modelBuilder.Entity<Company>(entity =>
        {
            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.CreateAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.UpdateAt).HasDefaultValueSql("(sysutcdatetime())");

            entity.HasOne(d => d.OwnerUser).WithMany(p => p.Companies).HasConstraintName("FK_Companies_OwnerUser");
        });

        modelBuilder.Entity<CompanyFriendship>(entity =>
        {
            entity.HasIndex(e => new { e.CompanyAId, e.CompanyBId }, "UX_Friendships_Pair_Active")
                .IsUnique()
                .HasFilter("([status] IN ('pending', 'accepted'))");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("(sysutcdatetime())");

            entity.HasOne(d => d.CompanyA).WithMany(p => p.CompanyFriendshipCompanyAs).HasConstraintName("FK_CompanyFriendships_A");

            entity.HasOne(d => d.CompanyB).WithMany(p => p.CompanyFriendshipCompanyBs).HasConstraintName("FK_CompanyFriendships_B");

            entity.HasOne(d => d.LastActionByNavigation).WithMany(p => p.CompanyFriendships).HasConstraintName("FK_CompanyFriendships_LastActor");
        });

        modelBuilder.Entity<CompanyMember>(entity =>
        {
            entity.HasIndex(e => new { e.CompanyId, e.UserId }, "UX_CompanyMembers_Unique")
                .IsUnique()
                .HasFilter("([company_id] IS NOT NULL AND [user_id] IS NOT NULL)");

            entity.Property(e => e.JoinedAt).HasDefaultValueSql("(sysutcdatetime())").HasConversion(utcKindConverter);
            entity.Property(e => e.Status).HasDefaultValue(true);

            entity.HasOne(d => d.Company).WithMany(p => p.CompanyMembers).HasConstraintName("FK_CompanyMembers_Company");

            entity.HasOne(d => d.User).WithMany(p => p.CompanyMembers).HasConstraintName("FK_CompanyMembers_User");
        });

        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasIndex(e => e.UserId, "IX_Notifications_User_Unread").HasFilter("([is_read]=(0))");

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.CreateAt).HasDefaultValueSql("(sysutcdatetime())");

            entity.HasOne(d => d.User).WithMany(p => p.Notifications).HasConstraintName("FK_Notifications_User");
        });

        modelBuilder.Entity<Project>(entity =>
        {
            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.CreateAt).HasDefaultValueSql("(sysutcdatetime())").HasConversion(utcKindConverter);
            entity.Property(e => e.UpdateAt).HasDefaultValueSql("(sysutcdatetime())").HasConversion(utcKindConverter);

            entity.HasOne(d => d.CompanyRequest).WithMany(p => p.ProjectCompanyRequests).HasConstraintName("FK_Projects_HiredCompany");

            entity.HasOne(d => d.Company).WithMany(p => p.ProjectCompanies).HasConstraintName("FK_Projects_Company");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.Projects).HasConstraintName("FK_Projects_CreatedBy");

            entity.HasOne(d => d.ProjectRequest).WithOne(p => p.Project).HasConstraintName("FK_Projects_Request");

            entity.HasOne(d => d.Workflow).WithMany(p => p.Projects).HasConstraintName("FK_Projects_Workflow");
        });

        modelBuilder.Entity<ProjectMember>(entity =>
        {
            entity.HasIndex(e => new { e.ProjectId, e.UserId }, "UX_ProjectMembers_Unique")
                .IsUnique()
                .HasFilter("([project_id] IS NOT NULL AND [user_id] IS NOT NULL)");

            entity.Property(e => e.JoinedAt).HasDefaultValueSql("(sysutcdatetime())");

            entity.HasOne(d => d.Project).WithMany(p => p.ProjectMembers).HasConstraintName("FK_ProjectMembers_Project");

            entity.HasOne(d => d.User).WithMany(p => p.ProjectMembers).HasConstraintName("FK_ProjectMembers_User");
        });

        modelBuilder.Entity<ProjectRequest>(entity =>
        {
            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.CreateAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.UpdateAt).HasDefaultValueSql("(sysutcdatetime())");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.ProjectRequests).HasConstraintName("FK_PRQ_CreatedBy");

            entity.HasOne(d => d.ExecutorCompany).WithMany(p => p.ProjectRequestExecutorCompanies).HasConstraintName("FK_PRQ_Executor");

            entity.HasOne(d => d.RequesterCompany).WithMany(p => p.ProjectRequestRequesterCompanies).HasConstraintName("FK_PRQ_Requester");
        });

        modelBuilder.Entity<ProjectTask>(entity =>
        {
            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.CreateAt).HasDefaultValueSql("(sysutcdatetime())").HasConversion(utcKindConverter);
            entity.Property(e => e.UpdateAt).HasDefaultValueSql("(sysutcdatetime())").HasConversion(utcKindConverter);

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.ProjectTasks).HasConstraintName("FK_ProjectTasks_CreatedBy");

            entity.HasOne(d => d.Project).WithMany(p => p.ProjectTasks).HasConstraintName("FK_ProjectTasks_Project");

            entity.HasOne(d => d.Sprint).WithMany(p => p.ProjectTasks).HasConstraintName("FK_ProjectTasks_Sprint");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasIndex(e => new { e.CompanyId, e.RoleName }, "UX_Roles_Company_RoleName")
                .IsUnique()
                .HasFilter("([company_id] IS NOT NULL AND [role_name] IS NOT NULL AND [status] = 'Active')");
    
            entity.HasOne(d => d.Company).WithMany(p => p.Roles).HasConstraintName("FK_Roles_Company");
        });

        modelBuilder.Entity<RolePermission>(entity =>
        {
            entity.HasIndex(e => new { e.CompanyId, e.RoleId, e.FunctionId }, "UX_RolePermissions_Unique")
                .IsUnique()
                .HasFilter("([company_id] IS NOT NULL AND [role_id] IS NOT NULL AND [function_id] IS NOT NULL)");

            entity.Property(e => e.IsAccess).HasDefaultValue(true);

            entity.HasOne(d => d.Company).WithMany(p => p.RolePermissions).HasConstraintName("FK_RolePermissions_Company");

            entity.HasOne(d => d.Function).WithMany(p => p.RolePermissions).HasConstraintName("FK_RolePermissions_Function");

            entity.HasOne(d => d.Role).WithMany(p => p.RolePermissions).HasConstraintName("FK_RolePermissions_Role");
        });

        modelBuilder.Entity<Sprint>(entity =>
        {
            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");

            entity.HasOne(d => d.Project).WithMany(p => p.Sprints).HasConstraintName("FK_Sprints_Project");
        });

        modelBuilder.Entity<TaskLogEvent>(entity =>
        {
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");

            entity.HasOne(d => d.Actor).WithMany(p => p.TaskLogEvents).HasConstraintName("FK_TaskLogEvent_Actor");

            entity.HasOne(d => d.Task).WithMany(p => p.TaskLogEvents).HasConstraintName("FK_TaskLogEvent_Task");
        });

        modelBuilder.Entity<TaskWorkflow>(entity =>
        {
            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");

            entity.HasOne(d => d.AssignUser).WithMany(p => p.TaskWorkflows).HasConstraintName("FK_TaskWorkflow_AssignUser");

            entity.HasOne(d => d.Task).WithMany(p => p.TaskWorkflows).HasConstraintName("FK_TaskWorkflow_Task");

            entity.HasOne(d => d.WorkflowStatus).WithMany(p => p.TaskWorkflows).HasConstraintName("FK_TaskWorkflow_Status");
        });

        modelBuilder.Entity<Ticket>(entity =>
        {
            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("(sysutcdatetime())");

            entity.HasOne(d => d.Project).WithMany(p => p.Tickets).HasConstraintName("FK_Tickets_Project");

            entity.HasOne(d => d.WorkflowStatus).WithMany(p => p.Tickets).HasConstraintName("FK_Tickets_Status");

            entity.HasOne(d => d.SubmittedByNavigation).WithMany(p => p.Tickets).HasConstraintName("FK_Tickets_Submitter");
        });

        modelBuilder.Entity<TicketComment>(entity =>
        {
            entity.Property(e => e.CreateAt).HasDefaultValueSql("(sysutcdatetime())");

            entity.HasOne(d => d.AuthorUser).WithMany(p => p.TicketComments).HasConstraintName("FK_TicketComments_Author");

            entity.HasOne(d => d.Ticket).WithMany(p => p.TicketComments).HasConstraintName("FK_TicketComments_Ticket");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(e => e.Email, "UX_Users_Email_NotNull")
                .IsUnique()
                .HasFilter("([email] IS NOT NULL)");

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.CreateAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.Status).HasDefaultValue(true);
            entity.Property(e => e.UpdateAt).HasDefaultValueSql("(sysutcdatetime())");

            entity.Property(e => e.PasswordHash)
         .HasColumnType("varbinary(512)"); 
            entity.Property(e => e.PasswordSalt)
                  .HasColumnType("varbinary(128)"); 
        });

        modelBuilder.Entity<UserRole>(entity =>
        {
            entity.HasIndex(e => new { e.UserId, e.RoleId }, "UX_UserRoles_Unique")
                .IsUnique()
                .HasFilter("([user_id] IS NOT NULL AND [role_id] IS NOT NULL)");

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");

            entity.HasOne(d => d.Role).WithMany(p => p.UserRoles).HasConstraintName("FK_UserRoles_Role");

            entity.HasOne(d => d.User).WithMany(p => p.UserRoles).HasConstraintName("FK_UserRoles_User");
        });

        modelBuilder.Entity<Workflow>(entity =>
        {
            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");

            entity.HasOne(d => d.Company).WithMany(p => p.Workflows).HasConstraintName("FK_Workflows_Company");
        });

        modelBuilder.Entity<WorkflowStatus>(entity =>
        {
            entity.HasIndex(e => new { e.WorkflowId, e.Name }, "UX_WorkflowStatus_Name")
                .IsUnique()
                .HasFilter("([workflow_id] IS NOT NULL AND [name] IS NOT NULL)");

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");

            entity.HasOne(d => d.Workflow).WithMany(p => p.WorkflowStatuses).HasConstraintName("FK_WorkflowStatus_Workflow");
        });

        modelBuilder.Entity<WorkflowTransition>(entity =>
        {
            entity.HasIndex(e => new { e.WorkflowId, e.FromStatusId, e.ToStatusId }, "UX_WorkflowTransitions_Unique")
                .IsUnique()
                .HasFilter("([workflow_id] IS NOT NULL AND [from_status_id] IS NOT NULL AND [to_status_id] IS NOT NULL)");

            entity.HasOne(d => d.FromStatus).WithMany(p => p.WorkflowTransitionFromStatuses).HasConstraintName("FK_WorkflowTransitions_From");

            entity.HasOne(d => d.ToStatus).WithMany(p => p.WorkflowTransitionToStatuses).HasConstraintName("FK_WorkflowTransitions_To");

            entity.HasOne(d => d.Workflow).WithMany(p => p.WorkflowTransitions).HasConstraintName("FK_WorkflowTransitions_Workflow");
        });

        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");

            entity.HasOne(d => d.User)
                  .WithMany(p => p.RefreshTokens)
                  .HasForeignKey(d => d.UserId)
                  .HasConstraintName("FK_RefreshTokens_User");
        });

        modelBuilder.Entity<CompanyActivityLog>(entity =>
        {
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
        });
        modelBuilder.Entity<UserLog>(entity =>
        {
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
        });

        // ---- Feature ----
        modelBuilder.Entity<Feature>(entity =>
        {
            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.IsActive).HasDefaultValue(true);

            // Unique code constraint
            entity.HasIndex(e => e.Code)
                  .IsUnique()
                  .HasDatabaseName("UX_FeaturesCatalogs_Code");

            // Quan hệ: Feature có nhiều PlanFeatures
            entity.HasMany(e => e.PlanFeatures)
                  .WithOne(f => f.Feature) 
                  .HasForeignKey(f => f.FeatureId)
                  .HasConstraintName("FK_SubscriptionPlanFeatures_Feature");
        });

        // ---- SubscriptionPlan ----
        modelBuilder.Entity<SubscriptionPlan>(entity =>
        {
            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("(sysutcdatetime())");

            // Enum conversion
            entity.Property(e => e.LicenseScope)
                  .HasConversion(new EnumMemberValueConverter<LicenseScope>())
                  .HasMaxLength(50);

            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.IsFullPackage).HasDefaultValue(false);
            entity.Property(e => e.AutoGrantMonthly).HasDefaultValue(false);
            // Quan hệ: 1 plan -> n prices
            entity.HasOne(p => p.Price)
                  .WithOne(p => p.SubscriptionPlan)
                  .HasForeignKey<SubscriptionPlanPrice>(p => p.PlanId)
                  .OnDelete(DeleteBehavior.Cascade) 
                  .HasConstraintName("FK_SubscriptionPlanPrices_Plan");

            // Quan hệ: 1 plan -> n features
            entity.HasMany<SubscriptionPlanFeature>()
                  .WithOne(p => p.SubscriptionPlan)
                  .HasForeignKey(p => p.PlanId)
                  .HasConstraintName("FK_SubscriptionPlanFeatures_Plan");

            entity.HasMany(p => p.TransactionPayments)
                  .WithOne(tp => tp.SubscriptionPlan)
                  .HasForeignKey(tp => tp.PlanId)
                  .HasConstraintName("FK_TransactionPayments_SubscriptionPlans_PlanId")
                  .OnDelete(DeleteBehavior.Restrict);

        });

        // ---- SubscriptionPlanPrice ----
        modelBuilder.Entity<SubscriptionPlanPrice>(entity =>
        {
            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");

            entity.Property(e => e.BillingPeriod)
                  .HasConversion(new EnumMemberValueConverter<BillingPeriod>())
                  .HasMaxLength(20);

            entity.Property(e => e.ChargeUnit)
                  .HasConversion(new EnumMemberValueConverter<ChargeUnit>())
                  .HasMaxLength(20);

            entity.Property(e => e.PaymentMode)
                  .HasConversion(new EnumMemberValueConverter<PaymentMode>())
                  .HasMaxLength(20);

            entity.Property(e => e.InstallmentInterval)
                  .HasConversion(new EnumMemberValueConverter<BillingPeriod>())
                  .HasMaxLength(20);
            entity.Property(e => e.Price).HasColumnType("decimal(18,2)");

            entity.HasIndex(e => e.PlanId)
                  .IsUnique()
                  .HasDatabaseName("UX_SubscriptionPlanPrices_Plan");
        });
        // ---- SubscriptionPlanPriceDiscount ----
        modelBuilder.Entity<SubscriptionPlanPriceDiscount>(entity =>
        {
            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");

            entity.Property(e => e.CreatedAt)
                  .HasDefaultValueSql("(sysutcdatetime())");

            entity.Property(e => e.UpdatedAt)
                  .HasDefaultValueSql("(sysutcdatetime())");

            entity.Property(e => e.DiscountValue)
                  .HasColumnType("decimal(18,2)");

            // UNIQUE: 1 price, 1 kỳ chỉ có 1 cấu hình discount
            entity.HasIndex(e => new { e.PriceId, e.InstallmentIndex })
                  .IsUnique()
                  .HasDatabaseName("UX_SubscriptionPlanPriceDiscounts_Price_Installment");

            entity.HasOne(d => d.Price)
                  .WithMany(p => p.Discounts)
                  .HasForeignKey(d => d.PriceId)
                  .HasConstraintName("FK_SubscriptionPlanPriceDiscounts_Price")
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // ---- SubscriptionPlanFeature ----
        modelBuilder.Entity<SubscriptionPlanFeature>(entity =>
        {
            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.Enabled).HasDefaultValue(true);

            entity.HasOne(d => d.SubscriptionPlan)
                  .WithMany(p => p.Features)
                  .HasForeignKey(d => d.PlanId)
                  .HasConstraintName("FK_SubscriptionPlanFeatures_Plan");

            entity.HasOne(d => d.Feature)
                  .WithMany(p => p.PlanFeatures)
                  .HasForeignKey(d => d.FeatureId)
                  .HasConstraintName("FK_SubscriptionPlanFeatures_Feature");


        });

        /* ================== TRANSACTION PAYMENT ================== */
        modelBuilder.Entity<TransactionPayment>(entity =>
        {
            entity.Property(x => x.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(x => x.Amount).HasColumnType("decimal(18,2)");
            entity.Property(x => x.Currency).HasMaxLength(3);

            // Map enums -> string theo EnumMember
            entity.Property(x => x.Status).HasConversion(new EnumMemberValueConverter<PaymentStatus>())
                  .HasMaxLength(32);
            entity.Property(x => x.Type).HasConversion(new EnumMemberValueConverter<TransactionType>())
                  .HasMaxLength(32);

            entity.Property(x => x.ChargeUnitSnapshot)
                  .HasConversion(new EnumMemberValueConverter<ChargeUnit>())
                  .HasMaxLength(20);
            entity.Property(x => x.BillingPeriodSnapshot)
                  .HasConversion(new EnumMemberValueConverter<BillingPeriod>())
                  .HasMaxLength(20);
            entity.Property(x => x.PaymentModeSnapshot)
                  .HasConversion(new EnumMemberValueConverter<PaymentMode>())
                  .HasMaxLength(20);


            // FK -> User: RESTRICT (không xoá dây chuyền vì rất nhiều bảng trỏ về User)
            entity.HasOne(tp => tp.User)
                  .WithMany(u => u.TransactionPayments)
                  .HasForeignKey(tp => tp.UserId)
                  .HasConstraintName("FK_TransactionPayments_Users_UserId")
                  .OnDelete(DeleteBehavior.Restrict);

            // FK -> Plan: RESTRICT (giữ lịch sử)
            entity.HasOne(tp => tp.SubscriptionPlan)
                  .WithMany(p => p.TransactionPayments)
                  .HasForeignKey(tp => tp.PlanId)
                  .HasConstraintName("FK_TransactionPayments_SubscriptionPlans_PlanId")
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(tp => tp.UserSubscription)
                  .WithMany(us => us.TransactionPayments)
                  .HasForeignKey(tp => tp.UserSubscriptionId)
                  .HasConstraintName("FK_TransactionPayments_UserSubscriptions_UserSubscriptionId")
                  .OnDelete(DeleteBehavior.SetNull);

            // Index
            entity.HasIndex(x => x.UserSubscriptionId);
            entity.HasIndex(x => x.PaymentLinkId);
            entity.HasIndex(x => x.DueAt);
            entity.HasIndex(x => new { x.UserId, x.PlanId, x.CreatedAt });
            entity.HasIndex(x => x.OrderCode).IsUnique().HasFilter("[order_code] IS NOT NULL");
        });
        modelBuilder.Entity<ProjectTaskAssignee>(e =>
        {
            e.ToTable("ProjectTaskAssignees");
            e.HasKey(x => new { x.TaskId, x.UserId });               // khóa chính kép
            e.Property(x => x.AssignedAt).HasPrecision(3)
                .HasDefaultValueSql("GETUTCDATE()");
          
            e.HasOne(x => x.User).WithMany()
                .HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Restrict);
            e.HasIndex(x => x.UserId);
        });

        modelBuilder.Entity<ProjectTaskDependency>(e =>
        {
            e.ToTable("ProjectTaskDependencies");
            e.HasKey(x => new { x.TaskId, x.DependsOnTaskId });

            e.Property(x => x.CreatedAt).HasPrecision(3).HasDefaultValueSql("GETUTCDATE()");

            e.HasOne(x => x.Task)
             .WithMany(t => t.Dependencies)     
             .HasForeignKey(x => x.TaskId)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(x => x.DependsOnTask)
             .WithMany()                       
             .HasForeignKey(x => x.DependsOnTaskId)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasIndex(x => x.DependsOnTaskId);
        });

        // ================== USER SUBSCRIPTION ==================
        modelBuilder.Entity<UserSubscription>(entity =>
        {
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("(sysutcdatetime())");

            // Map enums -> string theo EnumMember
            entity.Property(e => e.Status)
                  .HasConversion(new EnumMemberValueConverter<SubscriptionStatus>())
                  .HasMaxLength(32);

            entity.Property(e => e.LicenseScopeSnapshot)
                  .HasConversion(new EnumMemberValueConverter<LicenseScope>())
                  .HasMaxLength(32);

            entity.Property(e => e.ChargeUnitSnapshot)
                  .HasConversion(new EnumMemberValueConverter<ChargeUnit>())
                  .HasMaxLength(20);

            entity.Property(e => e.BillingPeriodSnapshot)
                  .HasConversion(new EnumMemberValueConverter<BillingPeriod>())
                  .HasMaxLength(20);

            entity.Property(e => e.PaymentModeSnapshot)
                  .HasConversion(new EnumMemberValueConverter<PaymentMode>())
                  .HasMaxLength(20);

            entity.Property(e => e.InstallmentIntervalSnapshot)
                  .HasConversion(new EnumMemberValueConverter<BillingPeriod>())
                  .HasMaxLength(20);

            entity.Property(e => e.UnitPriceSnapshot).HasColumnType("decimal(18,2)");
            entity.Property(e => e.CurrencySnapshot).HasMaxLength(3);

            // Quan hệ: UserSubscription -> User (RESTRICT)
            entity.HasOne(us => us.User)
                  .WithMany(u => u.UserSubscriptions)
                  .HasForeignKey(us => us.UserId)
                  .HasConstraintName("FK_UserSubscriptions_Users_UserId")
                  .OnDelete(DeleteBehavior.Restrict);

            // Quan hệ: UserSubscription -> Plan (RESTRICT)
            entity.HasOne(us => us.Plan)
                  .WithMany(p => p.UserSubscriptions)
                  .HasForeignKey(us => us.PlanId)
                  .HasConstraintName("FK_UserSubscriptions_SubscriptionPlans_PlanId")
                  .OnDelete(DeleteBehavior.Restrict);

            // Audit field: CreatedByTransactionId — chỉ index, không FK để né multipath
            entity.HasIndex(e => e.CreatedByTransactionId)
                  .HasDatabaseName("IX_UserSubscriptions_CreatedByTx");
        });

        // ---- UserSubscriptionEntitlement ----
        modelBuilder.Entity<UserSubscriptionEntitlement>(entity =>
        {
            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.Enabled).HasDefaultValue(true);

            entity.HasIndex(e => new { e.UserSubscriptionId, e.FeatureId })
                  .IsUnique()
                  .HasDatabaseName("UX_UserSubscriptionEntitlements_Sub_Feature");

            entity.HasOne(d => d.UserSubscription)
                  .WithMany(p => p.Entitlements)
                  .HasForeignKey(d => d.UserSubscriptionId)
                  .HasConstraintName("FK_UserSubscriptionEntitlements_UserSubscriptions_UserSubscriptionId")
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(d => d.Feature)
                  .WithMany()
                  .HasForeignKey(d => d.FeatureId)
                  .HasConstraintName("FK_UserSubscriptionEntitlements_FeaturesCatalogs_FeatureId")
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // ================== COMPANY SUBSCRIPTION ==================
        modelBuilder.Entity<CompanySubscription>(entity =>
        {
            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");

            // Đồng bộ default datetime
            entity.Property(e => e.SharedOn).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("(sysutcdatetime())");

            // Nếu sửa lại ExpiredAt như đã bàn:
            // entity.Property(e => e.ExpiredAt).HasColumnType("datetimeoffset");

            // Map enum -> string
            entity.Property(e => e.Status)
                  .HasConversion(new EnumMemberValueConverter<SubscriptionStatus>())
                  .HasMaxLength(32);

            // UNIQUE: 1 UserSubscription chỉ share 1 lần cho 1 company
            entity.HasIndex(e => new { e.UserSubscriptionId, e.CompanyId })
                  .IsUnique()
                  .HasDatabaseName("UX_CompanySubscriptions_UserSub_Company");

            // FK -> Company (RESTRICT để không cascade delete)
            entity.HasOne(e => e.Company)
                  .WithMany(c => c.CompanySubscriptions)
                  .HasForeignKey(e => e.CompanyId)
                  .HasConstraintName("FK_CompanySubscriptions_Companies_CompanyId")
                  .OnDelete(DeleteBehavior.Restrict);

            // FK -> UserSubscription (RESTRICT để tránh multipath)
            entity.HasOne(e => e.UserSubscription)
                  .WithMany(us => us.CompanySubscriptions)
                  .HasForeignKey(e => e.UserSubscriptionId)
                  .HasConstraintName("FK_CompanySubscriptions_UserSubscriptions_UserSubscriptionId")
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // ================== COMPANY SUBSCRIPTION ENTITLEMENT ==================
        modelBuilder.Entity<CompanySubscriptionEntitlement>(entity =>
        {
            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.Enabled).HasDefaultValue(true);

            // UNIQUE: 1 CompanySubscription chỉ có 1 entitlement cho mỗi Feature
            entity.HasIndex(e => new { e.CompanySubscriptionId, e.FeatureId })
                  .IsUnique()
                  .HasDatabaseName("UX_CompanySubscriptionEntitlements_Sub_Feature");

            // FK -> CompanySubscription (RESTRICT để tránh cascade chain phức tạp)
            entity.HasOne(e => e.CompanySubscription)
                  .WithMany(cs => cs.Entitlements)
                  .HasForeignKey(e => e.CompanySubscriptionId)
                  .HasConstraintName("FK_CompanySubscriptionEntitlements_CompanySubscriptions_CompanySubscriptionId")
                  .OnDelete(DeleteBehavior.Restrict);

            // FK -> Feature (RESTRICT cho thống nhất)
            entity.HasOne(e => e.Feature)
                  .WithMany()
                  .HasForeignKey(e => e.FeatureId)
                  .HasConstraintName("FK_CompanySubscriptionEntitlements_Features_FeatureId")
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // ================== COMPANY SUBSCRIPTION ENTRY ==================
        modelBuilder.Entity<CompanySubscriptionEntry>(entity =>
        {
            entity.Property(e => e.UsedAt)
                  .HasDefaultValueSql("(sysutcdatetime())");

            // Mỗi member chỉ được ghi 1 lần cho 1 company subscription
            entity.HasIndex(e => new { e.CompanySubscriptionId, e.CompanyMemberId })
                  .IsUnique()
                  .HasDatabaseName("UX_CompanySubscriptionEntries_Sub_Member");

            entity.HasOne(e => e.CompanySubscription)
                  .WithMany(s => s.Entries)
                  .HasForeignKey(e => e.CompanySubscriptionId)
                  .HasConstraintName("FK_CompanySubscriptionEntries_CompanySubscriptions_SubId")
                  .OnDelete(DeleteBehavior.Restrict); // tránh multiple cascade paths

            entity.HasOne(e => e.CompanyMember)
                  .WithMany(m => m.CompanySubscriptionEntries)
                  .HasForeignKey(e => e.CompanyMemberId)
                  .HasConstraintName("FK_CompanySubscriptionEntries_CompanyMembers_MemberId")
                  .OnDelete(DeleteBehavior.Restrict); // tránh multiple cascade paths
        });
        modelBuilder.Entity<ProjectTaskChecklistItem>(e =>
        {
            e.ToTable("ProjectTaskChecklistItems");

            e.Property(x => x.Label)
                .HasMaxLength(255);

            e.Property(x => x.CreatedAt)
                .HasPrecision(3)
                .HasDefaultValueSql("SYSUTCDATETIME()");

            e.HasOne(x => x.Task)
                .WithMany(t => t.ChecklistItems)
                .HasForeignKey(x => x.TaskId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_ProjectTaskChecklistItems_Task");
        });


        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
