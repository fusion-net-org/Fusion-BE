using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fusion.Repository.Migrations
{
    /// <inheritdoc />
    public partial class Update_Table_CompanySubscription_V3 : Migration
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
                    owner_user_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    status = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    SharedOn = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "(sysutcdatetime())"),
                    updated_at = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "(sysutcdatetime())"),
                    expired_at = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    seats_limit_snapshot = table.Column<int>(type: "int", nullable: true),
                    seats_limit_unit = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompanySubscriptions", x => x.id);
                    table.ForeignKey(
                        name: "FK_CompanySubscriptions_Companies_CompanyId",
                        column: x => x.company_id,
                        principalTable: "Companies",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CompanySubscriptions_UserSubscriptions_UserSubscriptionId",
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
                    feature_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    enabled = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompanySubscriptionEntitlements", x => x.id);
                    table.ForeignKey(
                        name: "FK_CompanySubscriptionEntitlements_CompanySubscriptions_CompanySubscriptionId",
                        column: x => x.company_subscription_id,
                        principalTable: "CompanySubscriptions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CompanySubscriptionEntitlements_Features_FeatureId",
                        column: x => x.feature_id,
                        principalTable: "FeaturesCatalogs",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CompanySubscriptionEntitlements_feature_id",
                table: "CompanySubscriptionEntitlements",
                column: "feature_id");

            migrationBuilder.CreateIndex(
                name: "UX_CompanySubscriptionEntitlements_Sub_Feature",
                table: "CompanySubscriptionEntitlements",
                columns: new[] { "company_subscription_id", "feature_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CompanySubscriptions_company_id",
                table: "CompanySubscriptions",
                column: "company_id");

            migrationBuilder.CreateIndex(
                name: "UX_CompanySubscriptions_UserSub_Company",
                table: "CompanySubscriptions",
                columns: new[] { "user_subscription_id", "company_id" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CompanySubscriptionEntitlements");

            migrationBuilder.DropTable(
                name: "CompanySubscriptions");
        }
    }
}
