using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fusion.Repository.Migrations
{
    /// <inheritdoc />
    public partial class Update_Table_SubscriptionPlan_V1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SubscriptionPlanFeatures_Plan",
                table: "subscriptionplanfeatures");

            migrationBuilder.DropForeignKey(
                name: "FK_SubscriptionPlanPrices_Plan",
                table: "subscriptionplanprices");

            migrationBuilder.DropTable(
                name: "CompanySubscriptionEntitlements");

            migrationBuilder.DropTable(
                name: "UserSubscriptionEntitlements");

            migrationBuilder.DropTable(
                name: "CompanySubscriptions");

            migrationBuilder.DropTable(
                name: "UserSubscriptions");

            migrationBuilder.DropTable(
                name: "TransactionPayments");

            migrationBuilder.DropPrimaryKey(
                name: "PK_subscriptionplans",
                table: "subscriptionplans");

            migrationBuilder.DropPrimaryKey(
                name: "PK_subscriptionplanprices",
                table: "subscriptionplanprices");

            migrationBuilder.DropIndex(
                name: "IX_subscriptionplanprices_plan_id",
                table: "subscriptionplanprices");

            migrationBuilder.DropIndex(
                name: "IX_subscriptionplanfeatures_plan_id",
                table: "subscriptionplanfeatures");

            migrationBuilder.DropColumn(
                name: "code",
                table: "subscriptionplans");

            migrationBuilder.DropColumn(
                name: "refund_fee_percent",
                table: "subscriptionplanprices");

            migrationBuilder.DropColumn(
                name: "feature_key",
                table: "subscriptionplanfeatures");

            migrationBuilder.DropColumn(
                name: "limit_value",
                table: "subscriptionplanfeatures");

            migrationBuilder.RenameTable(
                name: "subscriptionplans",
                newName: "SubscriptionPlans");

            migrationBuilder.RenameTable(
                name: "subscriptionplanprices",
                newName: "SubscriptionPlanPrices");

            migrationBuilder.RenameColumn(
                name: "refund_window_days",
                table: "SubscriptionPlanPrices",
                newName: "payment_mode");

            migrationBuilder.AddColumn<Guid>(
                name: "PricesId",
                table: "SubscriptionPlans",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "company_share_limit",
                table: "SubscriptionPlans",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_full_package",
                table: "SubscriptionPlans",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "license_scope",
                table: "SubscriptionPlans",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "seats_per_company_limit",
                table: "SubscriptionPlans",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "charge_unit",
                table: "SubscriptionPlanPrices",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "installment_count",
                table: "SubscriptionPlanPrices",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "installment_interval",
                table: "SubscriptionPlanPrices",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "enabled",
                table: "subscriptionplanfeatures",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "feature_id",
                table: "subscriptionplanfeatures",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddPrimaryKey(
                name: "PK_SubscriptionPlans",
                table: "SubscriptionPlans",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SubscriptionPlanPrices",
                table: "SubscriptionPlanPrices",
                column: "id");

            migrationBuilder.CreateTable(
                name: "FeaturesCatalogs",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "(newid())"),
                    code = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                    category = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    is_active = table.Column<bool>(type: "bit", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysutcdatetime())"),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysutcdatetime())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FeaturesCatalogs", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SubscriptionPlans_PricesId",
                table: "SubscriptionPlans",
                column: "PricesId");

            migrationBuilder.CreateIndex(
                name: "IX_SubscriptionPlanPrices_plan_id",
                table: "SubscriptionPlanPrices",
                column: "plan_id");

            migrationBuilder.CreateIndex(
                name: "IX_subscriptionplanfeatures_feature_id",
                table: "subscriptionplanfeatures",
                column: "feature_id");

            migrationBuilder.CreateIndex(
                name: "UX_SubscriptionPlanFeature_Unique",
                table: "subscriptionplanfeatures",
                columns: new[] { "plan_id", "feature_id" },
                unique: true,
                filter: "([plan_id] IS NOT NULL AND [feature_id] IS NOT NULL)");

            migrationBuilder.CreateIndex(
                name: "UX_Features_Code_Unique",
                table: "FeaturesCatalogs",
                column: "code",
                unique: true,
                filter: "([code] IS NOT NULL)");

            migrationBuilder.AddForeignKey(
                name: "FK_SubscriptionPlanFeature_Feature",
                table: "subscriptionplanfeatures",
                column: "feature_id",
                principalTable: "FeaturesCatalogs",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SubscriptionPlanFeature_Plan",
                table: "subscriptionplanfeatures",
                column: "plan_id",
                principalTable: "SubscriptionPlans",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SubscriptionPlanPrice_Plan",
                table: "SubscriptionPlanPrices",
                column: "plan_id",
                principalTable: "SubscriptionPlans",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SubscriptionPlans_SubscriptionPlanPrices_PricesId",
                table: "SubscriptionPlans",
                column: "PricesId",
                principalTable: "SubscriptionPlanPrices",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SubscriptionPlanFeature_Feature",
                table: "subscriptionplanfeatures");

            migrationBuilder.DropForeignKey(
                name: "FK_SubscriptionPlanFeature_Plan",
                table: "subscriptionplanfeatures");

            migrationBuilder.DropForeignKey(
                name: "FK_SubscriptionPlanPrice_Plan",
                table: "SubscriptionPlanPrices");

            migrationBuilder.DropForeignKey(
                name: "FK_SubscriptionPlans_SubscriptionPlanPrices_PricesId",
                table: "SubscriptionPlans");

            migrationBuilder.DropTable(
                name: "FeaturesCatalogs");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SubscriptionPlans",
                table: "SubscriptionPlans");

            migrationBuilder.DropIndex(
                name: "IX_SubscriptionPlans_PricesId",
                table: "SubscriptionPlans");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SubscriptionPlanPrices",
                table: "SubscriptionPlanPrices");

            migrationBuilder.DropIndex(
                name: "IX_SubscriptionPlanPrices_plan_id",
                table: "SubscriptionPlanPrices");

            migrationBuilder.DropIndex(
                name: "IX_subscriptionplanfeatures_feature_id",
                table: "subscriptionplanfeatures");

            migrationBuilder.DropIndex(
                name: "UX_SubscriptionPlanFeature_Unique",
                table: "subscriptionplanfeatures");

            migrationBuilder.DropColumn(
                name: "PricesId",
                table: "SubscriptionPlans");

            migrationBuilder.DropColumn(
                name: "company_share_limit",
                table: "SubscriptionPlans");

            migrationBuilder.DropColumn(
                name: "is_full_package",
                table: "SubscriptionPlans");

            migrationBuilder.DropColumn(
                name: "license_scope",
                table: "SubscriptionPlans");

            migrationBuilder.DropColumn(
                name: "seats_per_company_limit",
                table: "SubscriptionPlans");

            migrationBuilder.DropColumn(
                name: "charge_unit",
                table: "SubscriptionPlanPrices");

            migrationBuilder.DropColumn(
                name: "installment_count",
                table: "SubscriptionPlanPrices");

            migrationBuilder.DropColumn(
                name: "installment_interval",
                table: "SubscriptionPlanPrices");

            migrationBuilder.DropColumn(
                name: "enabled",
                table: "subscriptionplanfeatures");

            migrationBuilder.DropColumn(
                name: "feature_id",
                table: "subscriptionplanfeatures");

            migrationBuilder.RenameTable(
                name: "SubscriptionPlans",
                newName: "subscriptionplans");

            migrationBuilder.RenameTable(
                name: "SubscriptionPlanPrices",
                newName: "subscriptionplanprices");

            migrationBuilder.RenameColumn(
                name: "payment_mode",
                table: "subscriptionplanprices",
                newName: "refund_window_days");

            migrationBuilder.AddColumn<string>(
                name: "code",
                table: "subscriptionplans",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "refund_fee_percent",
                table: "subscriptionplanprices",
                type: "decimal(5,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "feature_key",
                table: "subscriptionplanfeatures",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "limit_value",
                table: "subscriptionplanfeatures",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddPrimaryKey(
                name: "PK_subscriptionplans",
                table: "subscriptionplans",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_subscriptionplanprices",
                table: "subscriptionplanprices",
                column: "id");

            migrationBuilder.CreateTable(
                name: "TransactionPayments",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "(newid())"),
                    plan_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    user_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    account_number = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    counterAccountBankId = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    counterAccountBankName = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    counterAccountName = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    counterAccountNumber = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    currency = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: true, defaultValue: "VND"),
                    description = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    order_code = table.Column<long>(type: "bigint", nullable: true),
                    payment_link_id = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    payment_method = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    reference = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false, defaultValue: "Pending"),
                    transaction_datetime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TransactionPayments", x => x.id);
                    table.ForeignKey(
                        name: "FK_TransactionPayments_Plan",
                        column: x => x.plan_id,
                        principalTable: "subscriptionplans",
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
                    transaction_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    create_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysutcdatetime())"),
                    currency = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true, defaultValue: "VND"),
                    expired_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    name_plan = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    price = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    update_at = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "(sysutcdatetime())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserSubscriptions", x => x.id);
                    table.ForeignKey(
                        name: "FK_UserSubscriptions_TransactionPayment",
                        column: x => x.transaction_id,
                        principalTable: "TransactionPayments",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CompanySubscriptions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "(newid())"),
                    company_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    user_subscription_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysutcdatetime())"),
                    expired_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    name_subscription = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "(sysutcdatetime())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompanySubscriptions", x => x.id);
                    table.ForeignKey(
                        name: "FK_CompanySubscriptions_Company",
                        column: x => x.company_id,
                        principalTable: "Companies",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CompanySubscriptions_UserSubscription",
                        column: x => x.user_subscription_id,
                        principalTable: "UserSubscriptions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UserSubscriptionEntitlements",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "(newid())"),
                    user_subscription_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    feature_key = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    quantity = table.Column<int>(type: "int", nullable: false),
                    remaining = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserSubscriptionEntitlements", x => x.id);
                    table.ForeignKey(
                        name: "FK_UserSubscriptionEntitlements_UserSubscription",
                        column: x => x.user_subscription_id,
                        principalTable: "UserSubscriptions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CompanySubscriptionEntitlements",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "(newid())"),
                    company_subscription_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    feature_key = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    quantity = table.Column<int>(type: "int", nullable: false),
                    remaining = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompanySubscriptionEntitlements", x => x.id);
                    table.ForeignKey(
                        name: "FK_CompanySubscriptionEntitlements_Subscription",
                        column: x => x.company_subscription_id,
                        principalTable: "CompanySubscriptions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_subscriptionplanprices_plan_id",
                table: "subscriptionplanprices",
                column: "plan_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_subscriptionplanfeatures_plan_id",
                table: "subscriptionplanfeatures",
                column: "plan_id");

            migrationBuilder.CreateIndex(
                name: "IX_CompanySubscriptionEntitlements_company_subscription_id",
                table: "CompanySubscriptionEntitlements",
                column: "company_subscription_id");

            migrationBuilder.CreateIndex(
                name: "IX_CompanySubscriptions_company_id",
                table: "CompanySubscriptions",
                column: "company_id");

            migrationBuilder.CreateIndex(
                name: "IX_CompanySubscriptions_user_subscription_id",
                table: "CompanySubscriptions",
                column: "user_subscription_id");

            migrationBuilder.CreateIndex(
                name: "IX_TransactionPayments_plan_id",
                table: "TransactionPayments",
                column: "plan_id");

            migrationBuilder.CreateIndex(
                name: "IX_TransactionPayments_user_id",
                table: "TransactionPayments",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_UserSubscriptionEntitlements_user_subscription_id",
                table: "UserSubscriptionEntitlements",
                column: "user_subscription_id");

            migrationBuilder.CreateIndex(
                name: "IX_UserSubscriptions_transaction_id",
                table: "UserSubscriptions",
                column: "transaction_id",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_SubscriptionPlanFeatures_Plan",
                table: "subscriptionplanfeatures",
                column: "plan_id",
                principalTable: "subscriptionplans",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SubscriptionPlanPrices_Plan",
                table: "subscriptionplanprices",
                column: "plan_id",
                principalTable: "subscriptionplans",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
