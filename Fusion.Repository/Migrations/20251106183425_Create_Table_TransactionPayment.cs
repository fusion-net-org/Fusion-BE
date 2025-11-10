using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fusion.Repository.Migrations
{
    /// <inheritdoc />
    public partial class Create_Table_TransactionPayment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TransactionPayments",
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
                    currency = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: true, defaultValue: "VND"),
                    counterAccountBankId = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    counterAccountBankName = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    counterAccountName = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    counterAccountNumber = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    payment_method = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false, defaultValue: "Pending")
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

            migrationBuilder.CreateIndex(
                name: "IX_TransactionPayments_plan_id",
                table: "TransactionPayments",
                column: "plan_id");

            migrationBuilder.CreateIndex(
                name: "IX_TransactionPayments_user_id",
                table: "TransactionPayments",
                column: "user_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TransactionPayments");
        }
    }
}
