using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fusion.Repository.Migrations
{
    /// <inheritdoc />
    public partial class AddIsDeleteInTicket : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FunctionInPages",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    function_code = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    function_name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    sort_order = table.Column<int>(type: "int", nullable: true),
                    page_code = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FunctionInPages", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "(newid())"),
                    userName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    avatar = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    email = table.Column<string>(type: "nvarchar(320)", maxLength: 320, nullable: true),
                    phone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    address = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    gender = table.Column<string>(type: "varchar(10)", unicode: false, maxLength: 10, nullable: true),
                    password_hash = table.Column<byte[]>(type: "varbinary(512)", nullable: false),
                    password_salt = table.Column<byte[]>(type: "varbinary(128)", nullable: false),
                    google_sub = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    is_system_admin = table.Column<bool>(type: "bit", nullable: false),
                    status = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    create_at = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: false, defaultValueSql: "(sysutcdatetime())"),
                    update_at = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: false, defaultValueSql: "(sysutcdatetime())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "Companies",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "(newid())"),
                    name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    owner_user_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    tax_code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    email = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    detail = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    image_company = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    create_at = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: false, defaultValueSql: "(sysutcdatetime())"),
                    update_at = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: false, defaultValueSql: "(sysutcdatetime())"),
                    is_deleted = table.Column<bool>(type: "bit", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Companies", x => x.id);
                    table.ForeignKey(
                        name: "FK_Companies_OwnerUser",
                        column: x => x.owner_user_id,
                        principalTable: "Users",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "Notifications",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "(newid())"),
                    user_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    @event = table.Column<string>(name: "event", type: "nvarchar(50)", maxLength: 50, nullable: true),
                    title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    body = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    context = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    link_url = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    is_read = table.Column<bool>(type: "bit", nullable: false),
                    create_at = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: false, defaultValueSql: "(sysutcdatetime())"),
                    read_at = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notifications", x => x.id);
                    table.ForeignKey(
                        name: "FK_Notifications_User",
                        column: x => x.user_id,
                        principalTable: "Users",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "refresh_tokens",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "(newid())"),
                    user_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    token = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    expires_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysutcdatetime())"),
                    revoked_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    replaced_by_token = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_refresh_tokens", x => x.id);
                    table.ForeignKey(
                        name: "FK_RefreshTokens_User",
                        column: x => x.user_id,
                        principalTable: "Users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CompanyFriendships",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    company_a_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    company_b_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    requester_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    status = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: true),
                    responded_at = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: true),
                    last_action_by = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: false, defaultValueSql: "(sysutcdatetime())"),
                    updated_at = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: false, defaultValueSql: "(sysutcdatetime())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompanyFriendships", x => x.id);
                    table.ForeignKey(
                        name: "FK_CompanyFriendships_A",
                        column: x => x.company_a_id,
                        principalTable: "Companies",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_CompanyFriendships_B",
                        column: x => x.company_b_id,
                        principalTable: "Companies",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_CompanyFriendships_LastActor",
                        column: x => x.last_action_by,
                        principalTable: "Users",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "CompanyMembers",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    company_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    user_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    status = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    joined_at = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: false, defaultValueSql: "(sysutcdatetime())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompanyMembers", x => x.id);
                    table.ForeignKey(
                        name: "FK_CompanyMembers_Company",
                        column: x => x.company_id,
                        principalTable: "Companies",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_CompanyMembers_User",
                        column: x => x.user_id,
                        principalTable: "Users",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "ProjectRequests",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "(newid())"),
                    requester_company_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    executor_company_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    created_by = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    status = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: true),
                    start_date = table.Column<DateOnly>(type: "date", nullable: true),
                    end_date = table.Column<DateOnly>(type: "date", nullable: true),
                    create_at = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: false, defaultValueSql: "(sysutcdatetime())"),
                    update_at = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: false, defaultValueSql: "(sysutcdatetime())"),
                    converted_project_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectRequests", x => x.id);
                    table.ForeignKey(
                        name: "FK_PRQ_CreatedBy",
                        column: x => x.created_by,
                        principalTable: "Users",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_PRQ_Executor",
                        column: x => x.executor_company_id,
                        principalTable: "Companies",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_PRQ_Requester",
                        column: x => x.requester_company_id,
                        principalTable: "Companies",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    company_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    role_name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    description = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.id);
                    table.ForeignKey(
                        name: "FK_Roles_Company",
                        column: x => x.company_id,
                        principalTable: "Companies",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "Workflows",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "(newid())"),
                    company_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    is_default = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Workflows", x => x.id);
                    table.ForeignKey(
                        name: "FK_Workflows_Company",
                        column: x => x.company_id,
                        principalTable: "Companies",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "RolePermissions",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    function_id = table.Column<int>(type: "int", nullable: true),
                    company_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    role_id = table.Column<int>(type: "int", nullable: true),
                    is_access = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RolePermissions", x => x.id);
                    table.ForeignKey(
                        name: "FK_RolePermissions_Company",
                        column: x => x.company_id,
                        principalTable: "Companies",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_RolePermissions_Function",
                        column: x => x.function_id,
                        principalTable: "FunctionInPages",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_RolePermissions_Role",
                        column: x => x.role_id,
                        principalTable: "Roles",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "UserRoles",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "(newid())"),
                    user_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    role_id = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserRoles", x => x.id);
                    table.ForeignKey(
                        name: "FK_UserRoles_Role",
                        column: x => x.role_id,
                        principalTable: "Roles",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_UserRoles_User",
                        column: x => x.user_id,
                        principalTable: "Users",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "Projects",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "(newid())"),
                    company_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    isHired = table.Column<bool>(type: "bit", nullable: false),
                    company_hired_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    project_request_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    workflow_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    start_date = table.Column<DateOnly>(type: "date", nullable: true),
                    end_date = table.Column<DateOnly>(type: "date", nullable: true),
                    created_by = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    create_at = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: false, defaultValueSql: "(sysutcdatetime())"),
                    update_at = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: false, defaultValueSql: "(sysutcdatetime())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Projects", x => x.id);
                    table.ForeignKey(
                        name: "FK_Projects_Company",
                        column: x => x.company_id,
                        principalTable: "Companies",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_Projects_CreatedBy",
                        column: x => x.created_by,
                        principalTable: "Users",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_Projects_HiredCompany",
                        column: x => x.company_hired_id,
                        principalTable: "Companies",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_Projects_Request",
                        column: x => x.project_request_id,
                        principalTable: "ProjectRequests",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_Projects_Workflow",
                        column: x => x.workflow_id,
                        principalTable: "Workflows",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "WorkflowStatus",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "(newid())"),
                    workflow_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    position = table.Column<int>(type: "int", nullable: false),
                    is_start = table.Column<bool>(type: "bit", nullable: false),
                    is_end = table.Column<bool>(type: "bit", nullable: false),
                    guard_name_key = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkflowStatus", x => x.id);
                    table.ForeignKey(
                        name: "FK_WorkflowStatus_Workflow",
                        column: x => x.workflow_id,
                        principalTable: "Workflows",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "ProjectMembers",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    project_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    user_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    isPartner = table.Column<bool>(type: "bit", nullable: false),
                    isViewAll = table.Column<bool>(type: "bit", nullable: false),
                    joined_at = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: false, defaultValueSql: "(sysutcdatetime())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectMembers", x => x.id);
                    table.ForeignKey(
                        name: "FK_ProjectMembers_Project",
                        column: x => x.project_id,
                        principalTable: "Projects",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_ProjectMembers_User",
                        column: x => x.user_id,
                        principalTable: "Users",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "Sprints",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "(newid())"),
                    project_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    start_date = table.Column<DateOnly>(type: "date", nullable: true),
                    end_date = table.Column<DateOnly>(type: "date", nullable: true),
                    color = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sprints", x => x.id);
                    table.ForeignKey(
                        name: "FK_Sprints_Project",
                        column: x => x.project_id,
                        principalTable: "Projects",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "Tickets",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "(newid())"),
                    project_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    priority = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: true),
                    urgency = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: true),
                    is_highest_urgen = table.Column<bool>(type: "bit", nullable: false),
                    ticket_name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    status_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    submitted_by = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    is_billable = table.Column<bool>(type: "bit", nullable: false),
                    budget = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    is_deleted = table.Column<bool>(type: "bit", nullable: true),
                    resolved_at = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: true),
                    closed_at = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: false, defaultValueSql: "(sysutcdatetime())"),
                    updated_at = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: false, defaultValueSql: "(sysutcdatetime())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tickets", x => x.id);
                    table.ForeignKey(
                        name: "FK_Tickets_Project",
                        column: x => x.project_id,
                        principalTable: "Projects",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_Tickets_Status",
                        column: x => x.status_id,
                        principalTable: "WorkflowStatus",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_Tickets_Submitter",
                        column: x => x.submitted_by,
                        principalTable: "Users",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "WorkflowTransitions",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    workflow_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    from_status_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    to_status_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkflowTransitions", x => x.id);
                    table.ForeignKey(
                        name: "FK_WorkflowTransitions_From",
                        column: x => x.from_status_id,
                        principalTable: "WorkflowStatus",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_WorkflowTransitions_To",
                        column: x => x.to_status_id,
                        principalTable: "WorkflowStatus",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_WorkflowTransitions_Workflow",
                        column: x => x.workflow_id,
                        principalTable: "Workflows",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "ProjectTasks",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "(newid())"),
                    project_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    sprint_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    type = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: true),
                    img = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    priority = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: true),
                    is_backlog = table.Column<bool>(type: "bit", nullable: false),
                    point = table.Column<int>(type: "int", nullable: true),
                    due_date = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: true),
                    source = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    withdrawn_at = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: true),
                    created_by = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    create_at = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: false, defaultValueSql: "(sysutcdatetime())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectTasks", x => x.id);
                    table.ForeignKey(
                        name: "FK_ProjectTasks_CreatedBy",
                        column: x => x.created_by,
                        principalTable: "Users",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_ProjectTasks_Project",
                        column: x => x.project_id,
                        principalTable: "Projects",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_ProjectTasks_Sprint",
                        column: x => x.sprint_id,
                        principalTable: "Sprints",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "TicketComments",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ticket_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    author_user_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    visibility = table.Column<string>(type: "varchar(10)", unicode: false, maxLength: 10, nullable: true),
                    body = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    create_at = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: false, defaultValueSql: "(sysutcdatetime())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TicketComments", x => x.id);
                    table.ForeignKey(
                        name: "FK_TicketComments_Author",
                        column: x => x.author_user_id,
                        principalTable: "Users",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_TicketComments_Ticket",
                        column: x => x.ticket_id,
                        principalTable: "Tickets",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "Comments",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    task_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    author_user_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    body = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    create_at = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: false, defaultValueSql: "(sysutcdatetime())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Comments", x => x.id);
                    table.ForeignKey(
                        name: "FK_Comments_Author",
                        column: x => x.author_user_id,
                        principalTable: "Users",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_Comments_Task",
                        column: x => x.task_id,
                        principalTable: "ProjectTasks",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "TaskLogEvent",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    task_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    action = table.Column<string>(type: "varchar(30)", unicode: false, maxLength: 30, nullable: true),
                    actor_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    changed_cols = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    old_row = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    new_row = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    metadata = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "datetimeoffset(3)", precision: 3, nullable: false, defaultValueSql: "(sysutcdatetime())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaskLogEvent", x => x.id);
                    table.ForeignKey(
                        name: "FK_TaskLogEvent_Actor",
                        column: x => x.actor_id,
                        principalTable: "Users",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_TaskLogEvent_Task",
                        column: x => x.task_id,
                        principalTable: "ProjectTasks",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "TaskWorkflow",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "(newid())"),
                    task_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    workflow_status_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    assign_user_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaskWorkflow", x => x.id);
                    table.ForeignKey(
                        name: "FK_TaskWorkflow_AssignUser",
                        column: x => x.assign_user_id,
                        principalTable: "Users",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_TaskWorkflow_Status",
                        column: x => x.workflow_status_id,
                        principalTable: "WorkflowStatus",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_TaskWorkflow_Task",
                        column: x => x.task_id,
                        principalTable: "ProjectTasks",
                        principalColumn: "id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Comments_author_user_id",
                table: "Comments",
                column: "author_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_Comments_task_id",
                table: "Comments",
                column: "task_id");

            migrationBuilder.CreateIndex(
                name: "IX_Companies_owner_user_id",
                table: "Companies",
                column: "owner_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_CompanyFriendships_company_b_id",
                table: "CompanyFriendships",
                column: "company_b_id");

            migrationBuilder.CreateIndex(
                name: "IX_CompanyFriendships_last_action_by",
                table: "CompanyFriendships",
                column: "last_action_by");

            migrationBuilder.CreateIndex(
                name: "UX_Friendships_Pair_Active",
                table: "CompanyFriendships",
                columns: new[] { "company_a_id", "company_b_id" },
                unique: true,
                filter: "([status] IN ('pending', 'accepted'))");

            migrationBuilder.CreateIndex(
                name: "IX_CompanyMembers_user_id",
                table: "CompanyMembers",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "UX_CompanyMembers_Unique",
                table: "CompanyMembers",
                columns: new[] { "company_id", "user_id" },
                unique: true,
                filter: "([company_id] IS NOT NULL AND [user_id] IS NOT NULL)");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_User_Time",
                table: "Notifications",
                columns: new[] { "user_id", "create_at" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_User_Unread",
                table: "Notifications",
                column: "user_id",
                filter: "([is_read]=(0))");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectMembers_user_id",
                table: "ProjectMembers",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "UX_ProjectMembers_Unique",
                table: "ProjectMembers",
                columns: new[] { "project_id", "user_id" },
                unique: true,
                filter: "([project_id] IS NOT NULL AND [user_id] IS NOT NULL)");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectRequests_created_by",
                table: "ProjectRequests",
                column: "created_by");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectRequests_executor_company_id",
                table: "ProjectRequests",
                column: "executor_company_id");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectRequests_requester_company_id",
                table: "ProjectRequests",
                column: "requester_company_id");

            migrationBuilder.CreateIndex(
                name: "UQ__ProjectR__FDFB014B54F36625",
                table: "ProjectRequests",
                column: "converted_project_id",
                unique: true,
                filter: "[converted_project_id] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Projects_company_hired_id",
                table: "Projects",
                column: "company_hired_id");

            migrationBuilder.CreateIndex(
                name: "IX_Projects_company_id",
                table: "Projects",
                column: "company_id");

            migrationBuilder.CreateIndex(
                name: "IX_Projects_created_by",
                table: "Projects",
                column: "created_by");

            migrationBuilder.CreateIndex(
                name: "IX_Projects_workflow_id",
                table: "Projects",
                column: "workflow_id");

            migrationBuilder.CreateIndex(
                name: "UQ__Projects__B79D8DD66555382B",
                table: "Projects",
                column: "project_request_id",
                unique: true,
                filter: "[project_request_id] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectTasks_created_by",
                table: "ProjectTasks",
                column: "created_by");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectTasks_project_id",
                table: "ProjectTasks",
                column: "project_id");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectTasks_sprint_id",
                table: "ProjectTasks",
                column: "sprint_id");

            migrationBuilder.CreateIndex(
                name: "IX_refresh_tokens_user_id",
                table: "refresh_tokens",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_RolePermissions_function_id",
                table: "RolePermissions",
                column: "function_id");

            migrationBuilder.CreateIndex(
                name: "IX_RolePermissions_role_id",
                table: "RolePermissions",
                column: "role_id");

            migrationBuilder.CreateIndex(
                name: "UX_RolePermissions_Unique",
                table: "RolePermissions",
                columns: new[] { "company_id", "role_id", "function_id" },
                unique: true,
                filter: "([company_id] IS NOT NULL AND [role_id] IS NOT NULL AND [function_id] IS NOT NULL)");

            migrationBuilder.CreateIndex(
                name: "UX_Roles_Company_RoleName",
                table: "Roles",
                columns: new[] { "company_id", "role_name" },
                unique: true,
                filter: "([company_id] IS NOT NULL AND [role_name] IS NOT NULL)");

            migrationBuilder.CreateIndex(
                name: "IX_Sprints_project_id",
                table: "Sprints",
                column: "project_id");

            migrationBuilder.CreateIndex(
                name: "IX_TaskLogEvent_actor_id",
                table: "TaskLogEvent",
                column: "actor_id");

            migrationBuilder.CreateIndex(
                name: "IX_TaskLogEvent_task_id",
                table: "TaskLogEvent",
                column: "task_id");

            migrationBuilder.CreateIndex(
                name: "IX_TaskWorkflow_assign_user_id",
                table: "TaskWorkflow",
                column: "assign_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_TaskWorkflow_task_id",
                table: "TaskWorkflow",
                column: "task_id");

            migrationBuilder.CreateIndex(
                name: "IX_TaskWorkflow_workflow_status_id",
                table: "TaskWorkflow",
                column: "workflow_status_id");

            migrationBuilder.CreateIndex(
                name: "IX_TicketComments_author_user_id",
                table: "TicketComments",
                column: "author_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_TicketComments_ticket_id",
                table: "TicketComments",
                column: "ticket_id");

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_project_id",
                table: "Tickets",
                column: "project_id");

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_status_id",
                table: "Tickets",
                column: "status_id");

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_submitted_by",
                table: "Tickets",
                column: "submitted_by");

            migrationBuilder.CreateIndex(
                name: "IX_UserRoles_role_id",
                table: "UserRoles",
                column: "role_id");

            migrationBuilder.CreateIndex(
                name: "UX_UserRoles_Unique",
                table: "UserRoles",
                columns: new[] { "user_id", "role_id" },
                unique: true,
                filter: "([user_id] IS NOT NULL AND [role_id] IS NOT NULL)");

            migrationBuilder.CreateIndex(
                name: "UQ__Users__74328F6E0433F7F6",
                table: "Users",
                column: "google_sub",
                unique: true,
                filter: "[google_sub] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "UX_Users_Email_NotNull",
                table: "Users",
                column: "email",
                unique: true,
                filter: "([email] IS NOT NULL)");

            migrationBuilder.CreateIndex(
                name: "IX_Workflows_company_id",
                table: "Workflows",
                column: "company_id");

            migrationBuilder.CreateIndex(
                name: "UX_WorkflowStatus_Name",
                table: "WorkflowStatus",
                columns: new[] { "workflow_id", "name" },
                unique: true,
                filter: "([workflow_id] IS NOT NULL AND [name] IS NOT NULL)");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowTransitions_from_status_id",
                table: "WorkflowTransitions",
                column: "from_status_id");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowTransitions_to_status_id",
                table: "WorkflowTransitions",
                column: "to_status_id");

            migrationBuilder.CreateIndex(
                name: "UX_WorkflowTransitions_Unique",
                table: "WorkflowTransitions",
                columns: new[] { "workflow_id", "from_status_id", "to_status_id" },
                unique: true,
                filter: "([workflow_id] IS NOT NULL AND [from_status_id] IS NOT NULL AND [to_status_id] IS NOT NULL)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Comments");

            migrationBuilder.DropTable(
                name: "CompanyFriendships");

            migrationBuilder.DropTable(
                name: "CompanyMembers");

            migrationBuilder.DropTable(
                name: "Notifications");

            migrationBuilder.DropTable(
                name: "ProjectMembers");

            migrationBuilder.DropTable(
                name: "refresh_tokens");

            migrationBuilder.DropTable(
                name: "RolePermissions");

            migrationBuilder.DropTable(
                name: "TaskLogEvent");

            migrationBuilder.DropTable(
                name: "TaskWorkflow");

            migrationBuilder.DropTable(
                name: "TicketComments");

            migrationBuilder.DropTable(
                name: "UserRoles");

            migrationBuilder.DropTable(
                name: "WorkflowTransitions");

            migrationBuilder.DropTable(
                name: "FunctionInPages");

            migrationBuilder.DropTable(
                name: "ProjectTasks");

            migrationBuilder.DropTable(
                name: "Tickets");

            migrationBuilder.DropTable(
                name: "Roles");

            migrationBuilder.DropTable(
                name: "Sprints");

            migrationBuilder.DropTable(
                name: "WorkflowStatus");

            migrationBuilder.DropTable(
                name: "Projects");

            migrationBuilder.DropTable(
                name: "ProjectRequests");

            migrationBuilder.DropTable(
                name: "Workflows");

            migrationBuilder.DropTable(
                name: "Companies");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
