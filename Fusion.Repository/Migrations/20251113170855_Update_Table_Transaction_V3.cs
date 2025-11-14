using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fusion.Repository.Migrations
{
    /// <inheritdoc />
    public partial class Update_Table_Transaction_V3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TransactionPayments",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    user_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    user_subscription_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    plan_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    order_code = table.Column<long>(type: "bigint", nullable: true),
                    payment_link_id = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    payment_method = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    provider = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: true),
                    charge_unit_snapshot = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    billing_period_snapshot = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    period_count_snapshot = table.Column<int>(type: "int", nullable: false),
                    seat_count_snapshot = table.Column<int>(type: "int", nullable: true),
                    payment_mode_snapshot = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    installment_index = table.Column<int>(type: "int", nullable: true),
                    installment_total = table.Column<int>(type: "int", nullable: true),
                    amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    currency = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: true),
                    description = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    reference = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    account_number = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    counterAccountBankId = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    counterAccountBankName = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    counterAccountName = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    counterAccountNumber = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    transaction_datetime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    due_at = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    paid_at = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    status = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    type = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "(sysutcdatetime())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TransactionPayments", x => x.id);
                    table.ForeignKey(
                        name: "FK_TransactionPayments_SubscriptionPlans_PlanId",
                        column: x => x.plan_id,
                        principalTable: "SubscriptionPlans",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TransactionPayments_Users_UserId",
                        column: x => x.user_id,
                        principalTable: "Users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TransactionPayments_due_at",
                table: "TransactionPayments",
                column: "due_at");

            migrationBuilder.CreateIndex(
                name: "IX_TransactionPayments_order_code",
                table: "TransactionPayments",
                column: "order_code",
                unique: true,
                filter: "[order_code] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_TransactionPayments_payment_link_id",
                table: "TransactionPayments",
                column: "payment_link_id");

            migrationBuilder.CreateIndex(
                name: "IX_TransactionPayments_plan_id",
                table: "TransactionPayments",
                column: "plan_id");

            migrationBuilder.CreateIndex(
                name: "IX_TransactionPayments_user_id_plan_id_created_at",
                table: "TransactionPayments",
                columns: new[] { "user_id", "plan_id", "created_at" });

            migrationBuilder.CreateIndex(
                name: "IX_TransactionPayments_user_subscription_id",
                table: "TransactionPayments",
                column: "user_subscription_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TransactionPayments");
        }
    }
}
