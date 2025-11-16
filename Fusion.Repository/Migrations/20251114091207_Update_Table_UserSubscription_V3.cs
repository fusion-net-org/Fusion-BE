using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fusion.Repository.Migrations
{
    /// <inheritdoc />
    public partial class Update_Table_UserSubscription_V3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "installment_interval",
                table: "SubscriptionPlanPrices",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "UserSubscriptions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    user_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    plan_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    created_by_transaction_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    status = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    term_start = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    term_end = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    next_payment_due_at = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "(sysutcdatetime())"),
                    updated_at = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "(sysutcdatetime())"),
                    canceled_at = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    license_scope_snapshot = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    is_full_package_snapshot = table.Column<bool>(type: "bit", nullable: false),
                    company_share_limit_snapshot = table.Column<int>(type: "int", nullable: true),
                    seats_per_company_limit_snapshot = table.Column<int>(type: "int", nullable: true),
                    charge_unit_snapshot = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    billing_period_snapshot = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    period_count_snapshot = table.Column<int>(type: "int", nullable: false),
                    payment_mode_snapshot = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    installment_count_snapshot = table.Column<int>(type: "int", nullable: true),
                    installment_interval_snapshot = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    currency_snapshot = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    unit_price_snapshot = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserSubscriptions", x => x.id);
                    table.ForeignKey(
                        name: "FK_UserSubscriptions_SubscriptionPlans_PlanId",
                        column: x => x.plan_id,
                        principalTable: "SubscriptionPlans",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserSubscriptions_Users_UserId",
                        column: x => x.user_id,
                        principalTable: "Users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UserSubscriptionEntitlements",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "(newid())"),
                    user_subscription_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    feature_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    enabled = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserSubscriptionEntitlements", x => x.id);
                    table.ForeignKey(
                        name: "FK_UserSubscriptionEntitlements_FeaturesCatalogs_FeatureId",
                        column: x => x.feature_id,
                        principalTable: "FeaturesCatalogs",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserSubscriptionEntitlements_UserSubscriptions_UserSubscriptionId",
                        column: x => x.user_subscription_id,
                        principalTable: "UserSubscriptions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserSubscriptionEntitlements_feature_id",
                table: "UserSubscriptionEntitlements",
                column: "feature_id");

            migrationBuilder.CreateIndex(
                name: "UX_UserSubscriptionEntitlements_Sub_Feature",
                table: "UserSubscriptionEntitlements",
                columns: new[] { "user_subscription_id", "feature_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserSubscriptions_CreatedByTx",
                table: "UserSubscriptions",
                column: "created_by_transaction_id");

            migrationBuilder.CreateIndex(
                name: "IX_UserSubscriptions_plan_id",
                table: "UserSubscriptions",
                column: "plan_id");

            migrationBuilder.CreateIndex(
                name: "IX_UserSubscriptions_user_id",
                table: "UserSubscriptions",
                column: "user_id");

            migrationBuilder.AddForeignKey(
                name: "FK_TransactionPayments_UserSubscriptions_UserSubscriptionId",
                table: "TransactionPayments",
                column: "user_subscription_id",
                principalTable: "UserSubscriptions",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TransactionPayments_UserSubscriptions_UserSubscriptionId",
                table: "TransactionPayments");

            migrationBuilder.DropTable(
                name: "UserSubscriptionEntitlements");

            migrationBuilder.DropTable(
                name: "UserSubscriptions");

            migrationBuilder.AlterColumn<int>(
                name: "installment_interval",
                table: "SubscriptionPlanPrices",
                type: "int",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20,
                oldNullable: true);
        }
    }
}
