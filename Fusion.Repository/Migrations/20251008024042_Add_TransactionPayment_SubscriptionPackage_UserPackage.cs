using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fusion.Repository.Migrations
{
    /// <inheritdoc />
    public partial class Add_TransactionPayment_SubscriptionPackage_UserPackage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SubscriptionPackages",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "(newid())"),
                    name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    price = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    quota_company = table.Column<int>(type: "int", nullable: false),
                    quota_project = table.Column<int>(type: "int", nullable: false),
                    description = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    create_at = table.Column<DateTime>(type: "datetime2", nullable: false),
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
                    user_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    package_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    transaction_code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    payment_method = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
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
                    user_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    package_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    purchase_date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    quota_company_added = table.Column<int>(type: "int", nullable: false),
                    quota_project_added = table.Column<int>(type: "int", nullable: false),
                    quota_company_remaining = table.Column<int>(type: "int", nullable: false),
                    quota_project_remaining = table.Column<int>(type: "int", nullable: false),
                    expiry_date = table.Column<DateTime>(type: "datetime2", nullable: true),
                    is_active = table.Column<bool>(type: "bit", nullable: false)
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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TransactionPayments");

            migrationBuilder.DropTable(
                name: "UserSubscriptions");

            migrationBuilder.DropTable(
                name: "SubscriptionPackages");
        }
    }
}
