using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fusion.Repository.Migrations
{
    /// <inheritdoc />
    public partial class Create_table_TransactionPayment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "transactionpayments",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "(newid())"),
                    user_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    plan_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    order_code = table.Column<long>(type: "bigint", nullable: true),
                    payment_link_id = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    description = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    account_number = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    reference = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    transaction_datetime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    currency = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: true),
                    counterAccountBankId = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    counterAccountBankName = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    counterAccountName = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    counterAccountNumber = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    payment_method = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_transactionpayments", x => x.id);
                    table.ForeignKey(
                        name: "FK_TransactionPayments_Plan",
                        column: x => x.plan_id,
                        principalTable: "subscriptionplans",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TransactionPayments_User",
                        column: x => x.user_id,
                        principalTable: "Users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_transactionpayments_order_code",
                table: "transactionpayments",
                column: "order_code");

            migrationBuilder.CreateIndex(
                name: "IX_transactionpayments_payment_link_id",
                table: "transactionpayments",
                column: "payment_link_id");

            migrationBuilder.CreateIndex(
                name: "IX_transactionpayments_plan_id",
                table: "transactionpayments",
                column: "plan_id");

            migrationBuilder.CreateIndex(
                name: "IX_transactionpayments_reference",
                table: "transactionpayments",
                column: "reference");

            migrationBuilder.CreateIndex(
                name: "IX_transactionpayments_user_id",
                table: "transactionpayments",
                column: "user_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "transactionpayments");
        }
    }
}
