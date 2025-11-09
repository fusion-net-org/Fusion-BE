using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fusion.Repository.Migrations
{
    /// <inheritdoc />
    public partial class Create_Table_CompanySubscription : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CompanySubscriptions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "(newid())"),
                    company_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    user_subscription_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    name_subscription = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysutcdatetime())"),
                    expired_at = table.Column<DateTime>(type: "datetime2", nullable: false),
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

            migrationBuilder.CreateTable(
                name: "CompanySubscriptionRoles",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "(newid())"),
                    company_subscription_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    name_role = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompanySubscriptionRoles", x => x.id);
                    table.ForeignKey(
                        name: "FK_CompanySubscriptionRoles_Subscription",
                        column: x => x.company_subscription_id,
                        principalTable: "CompanySubscriptions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CompanySubscriptionEntitlements_company_subscription_id",
                table: "CompanySubscriptionEntitlements",
                column: "company_subscription_id");

            migrationBuilder.CreateIndex(
                name: "IX_CompanySubscriptionRoles_company_subscription_id",
                table: "CompanySubscriptionRoles",
                column: "company_subscription_id");

            migrationBuilder.CreateIndex(
                name: "IX_CompanySubscriptions_company_id",
                table: "CompanySubscriptions",
                column: "company_id");

            migrationBuilder.CreateIndex(
                name: "IX_CompanySubscriptions_user_subscription_id",
                table: "CompanySubscriptions",
                column: "user_subscription_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CompanySubscriptionEntitlements");

            migrationBuilder.DropTable(
                name: "CompanySubscriptionRoles");

            migrationBuilder.DropTable(
                name: "CompanySubscriptions");
        }
    }
}
