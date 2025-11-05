
using Fusion.Repository.Entities;
using Microsoft.EntityFrameworkCore;

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
    public virtual DbSet<SubscriptionPlan> SubscriptionPackages { get; set; }
    public virtual DbSet<UserSubscription> UserSubscriptions { get; set; }
    public virtual DbSet<TransactionPayment> TransactionPayments { get; set; }
    public virtual DbSet<UserDevice> UserDevices { get; set; }
    public virtual DbSet<CompanyActivityLog> CompanyActivityLogs { get; set; }
    public virtual DbSet<UserLog> UserLogs { get; set; }
    public virtual DbSet<CompanySubscriptionAssignment> CompanySubscriptionAssignments { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Comment>(entity =>
        {
            entity.Property(e => e.CreateAt).HasDefaultValueSql("(sysutcdatetime())");

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

            entity.Property(e => e.JoinedAt).HasDefaultValueSql("(sysutcdatetime())");
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
            entity.Property(e => e.CreateAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.UpdateAt).HasDefaultValueSql("(sysutcdatetime())");

            entity.HasOne(d => d.CompanyHired).WithMany(p => p.ProjectCompanyHireds).HasConstraintName("FK_Projects_HiredCompany");

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
            entity.Property(e => e.CreateAt).HasDefaultValueSql("(sysutcdatetime())");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.ProjectTasks).HasConstraintName("FK_ProjectTasks_CreatedBy");

            entity.HasOne(d => d.Project).WithMany(p => p.ProjectTasks).HasConstraintName("FK_ProjectTasks_Project");

            entity.HasOne(d => d.Sprint).WithMany(p => p.ProjectTasks).HasConstraintName("FK_ProjectTasks_Sprint");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasIndex(e => new { e.CompanyId, e.RoleName }, "UX_Roles_Company_RoleName")
                .IsUnique()
                .HasFilter("([company_id] IS NOT NULL AND [role_name] IS NOT NULL)");

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

            entity.HasOne(d => d.Status).WithMany(p => p.Tickets).HasConstraintName("FK_Tickets_Status");

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


        modelBuilder.Entity<SubscriptionPlan>(entity =>
        {
            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Price).IsRequired().HasColumnType("decimal(18,2)");
            entity.Property(e => e.Description).IsRequired().HasMaxLength(250);

            entity.HasMany(e => e.UserSubscriptions)
                  .WithOne(e => e.SubscriptionPackage)
                  .HasForeignKey(e => e.PackageId)
                  .HasConstraintName("FK_UserSubscriptions_SubscriptionPackage");

            entity.HasMany(e => e.TransactionPayments)
                  .WithOne(e => e.SubscriptionPackage)
                  .HasForeignKey(e => e.PackageId)
                  .HasConstraintName("FK_TransactionPayments_SubscriptionPackage");
        });


        modelBuilder.Entity<UserSubscription>(entity =>
        {
            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.PurchaseDate).IsRequired();
            entity.Property(e => e.QuotaCompanyAdded).IsRequired();
            entity.Property(e => e.QuotaProjectAdded).IsRequired();
            entity.Property(e => e.QuotaCompanyRemaining).IsRequired();
            entity.Property(e => e.QuotaProjectRemaining).IsRequired();

            entity.HasOne(e => e.User)
                  .WithMany(e => e.UserSubscriptions)
                  .HasForeignKey(e => e.UserId)
                  .HasConstraintName("FK_UserSubscriptions_User");

            entity.HasOne(e => e.SubscriptionPackage)
                  .WithMany(e => e.UserSubscriptions)
                  .HasForeignKey(e => e.PackageId)
                  .HasConstraintName("FK_UserSubscriptions_SubscriptionPackage");
        });


        modelBuilder.Entity<TransactionPayment>(entity =>
        {
            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.Amount).IsRequired().HasColumnType("decimal(18,2)");
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.PaymentMethod).HasMaxLength(50);
            entity.Property(e => e.Status).HasMaxLength(50);

            entity.HasOne(e => e.User)
                  .WithMany(e => e.TransactionPayments)
                  .HasForeignKey(e => e.UserId)
                  .HasConstraintName("FK_TransactionPayments_User");

            entity.HasOne(e => e.SubscriptionPackage)
                  .WithMany(e => e.TransactionPayments)
                  .HasForeignKey(e => e.PackageId)
                  .HasConstraintName("FK_TransactionPayments_SubscriptionPackage");
        });

        modelBuilder.Entity<CompanyActivityLog>(entity =>
        {
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
        });
        modelBuilder.Entity<UserLog>(entity =>
        {
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
        });

        modelBuilder.Entity<CompanySubscriptionAssignment>(entity =>
        {
            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");

            entity.Property(e => e.AssignedAt).HasDefaultValueSql("(sysutcdatetime())");

            entity.Property(e => e.IsEnabled).HasDefaultValue(true);

            entity.HasOne(e => e.Member)
                  .WithMany()
                  .HasForeignKey(e => e.CompanyMemberId)
                  .HasConstraintName("FK_CompanySubscriptionAssignments_CompanyMember");

            entity.HasOne(e => e.OwnerSubscription)
                  .WithMany()
                  .HasForeignKey(e => e.UserSubscriptionId)
                  .HasConstraintName("FK_CompanySubscriptionAssignments_UserSubscription");

            entity.HasIndex(e => new { e.CompanyMemberId, e.CodeTransaction }, "UX_CompanySubscriptionAssignments_Unique")
                  .IsUnique()
                  .HasFilter("([company_member_id] IS NOT NULL AND [code_transaction] IS NOT NULL)");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
