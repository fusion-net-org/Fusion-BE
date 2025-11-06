using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fusion.Repository.Migrations
{
    /// <inheritdoc />
    public partial class Create_Table_SubscriptionPlan : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CompanySubscriptionAssignments");

            migrationBuilder.DropTable(
                name: "TransactionPayments");

            migrationBuilder.DropTable(
                name: "UserSubscriptions");

            migrationBuilder.DropTable(
                name: "SubscriptionPackages");

            migrationBuilder.CreateTable(
                name: "subscriptionplans",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "(newid())"),
                    code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    is_active = table.Column<bool>(type: "bit", nullable: false),
                    is_customizable = table.Column<bool>(type: "bit", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysutcdatetime())"),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysutcdatetime())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_subscriptionplans", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "subscriptionplanfeatures",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "(newid())"),
                    plan_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    feature_key = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    limit_value = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_subscriptionplanfeatures", x => x.id);
                    table.ForeignKey(
                        name: "FK_SubscriptionPlanFeatures_Plan",
                        column: x => x.plan_id,
                        principalTable: "subscriptionplans",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "subscriptionplanprices",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "(newid())"),
                    plan_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    billing_period = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    period_count = table.Column<int>(type: "int", nullable: false),
                    price = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    currency = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    refund_window_days = table.Column<int>(type: "int", nullable: false),
                    refund_fee_percent = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    create_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysutcdatetime())"),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_subscriptionplanprices", x => x.id);
                    table.ForeignKey(
                        name: "FK_SubscriptionPlanPrices_Plan",
                        column: x => x.plan_id,
                        principalTable: "subscriptionplans",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_subscriptionplanfeatures_plan_id",
                table: "subscriptionplanfeatures",
                column: "plan_id");

            migrationBuilder.CreateIndex(
                name: "IX_subscriptionplanprices_plan_id",
                table: "subscriptionplanprices",
                column: "plan_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "subscriptionplanfeatures");

            migrationBuilder.DropTable(
                name: "subscriptionplanprices");

            migrationBuilder.DropTable(
                name: "subscriptionplans");

            migrationBuilder.CreateTable(
                name: "SubscriptionPackages",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "(newid())"),
                    create_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    description = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    price = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    quota_company = table.Column<int>(type: "int", nullable: false),
                    quota_project = table.Column<int>(type: "int", nullable: false),
                    update_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubscriptionPackages", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "TransactionPayments",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "(newid())"),
                    package_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    user_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    payment_method = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    transaction_code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TransactionPayments", x => x.id);
                    table.ForeignKey(
                        name: "FK_TransactionPayments_SubscriptionPackage",
                        column: x => x.package_id,
                        principalTable: "SubscriptionPackages",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TransactionPayments_User",
                        column: x => x.user_id,
                        principalTable: "Users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserSubscriptions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "(newid())"),
                    package_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    user_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    expiry_date = table.Column<DateTime>(type: "datetime2", nullable: true),
                    is_active = table.Column<bool>(type: "bit", nullable: false),
                    purchase_date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    quota_company_added = table.Column<int>(type: "int", nullable: false),
                    quota_company_remaining = table.Column<int>(type: "int", nullable: false),
                    quota_project_added = table.Column<int>(type: "int", nullable: false),
                    quota_project_remaining = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserSubscriptions", x => x.id);
                    table.ForeignKey(
                        name: "FK_UserSubscriptions_SubscriptionPackage",
                        column: x => x.package_id,
                        principalTable: "SubscriptionPackages",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserSubscriptions_User",
                        column: x => x.user_id,
                        principalTable: "Users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CompanySubscriptionAssignments",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "(newid())"),
                    company_member_id = table.Column<long>(type: "bigint", nullable: false),
                    user_subscription_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    assigned_at = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: false, defaultValueSql: "(sysutcdatetime())"),
                    code_transaction = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CompanyMemberId1 = table.Column<long>(type: "bigint", nullable: true),
                    is_enabled = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    revoked_at = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompanySubscriptionAssignments", x => x.id);
                    table.ForeignKey(
                        name: "FK_CompanySubscriptionAssignments_CompanyMember",
                        column: x => x.company_member_id,
                        principalTable: "CompanyMembers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CompanySubscriptionAssignments_CompanyMembers_CompanyMemberId1",
                        column: x => x.CompanyMemberId1,
                        principalTable: "CompanyMembers",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_CompanySubscriptionAssignments_UserSubscription",
                        column: x => x.user_subscription_id,
                        principalTable: "UserSubscriptions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CompanySubscriptionAssignments_CompanyMemberId1",
                table: "CompanySubscriptionAssignments",
                column: "CompanyMemberId1");

            migrationBuilder.CreateIndex(
                name: "IX_CompanySubscriptionAssignments_user_subscription_id",
                table: "CompanySubscriptionAssignments",
                column: "user_subscription_id");

            migrationBuilder.CreateIndex(
                name: "UX_CompanySubscriptionAssignments_Unique",
                table: "CompanySubscriptionAssignments",
                columns: new[] { "company_member_id", "code_transaction" },
                unique: true,
                filter: "([company_member_id] IS NOT NULL AND [code_transaction] IS NOT NULL)");

            migrationBuilder.CreateIndex(
                name: "IX_TransactionPayments_package_id",
                table: "TransactionPayments",
                column: "package_id");

            migrationBuilder.CreateIndex(
                name: "IX_TransactionPayments_user_id",
                table: "TransactionPayments",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_UserSubscriptions_package_id",
                table: "UserSubscriptions",
                column: "package_id");

            migrationBuilder.CreateIndex(
                name: "IX_UserSubscriptions_user_id",
                table: "UserSubscriptions",
                column: "user_id");
        }
    }
}
