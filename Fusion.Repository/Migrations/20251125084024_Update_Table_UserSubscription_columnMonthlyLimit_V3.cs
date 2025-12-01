using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fusion.Repository.Migrations
{
    /// <inheritdoc />
    public partial class Update_Table_UserSubscription_columnMonthlyLimit_V3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "monthly_limit",
                table: "FeaturesCatalogs");

            migrationBuilder.AddColumn<int>(
                name: "limit_unit",
                table: "UserSubscriptionEntitlements",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "monthly_limit",
                table: "UserSubscriptionEntitlements",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "monthly_limit",
                table: "subscriptionplanfeatures",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "limit_unit",
                table: "CompanySubscriptionEntitlements",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "monthly_limit",
                table: "CompanySubscriptionEntitlements",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "limit_unit",
                table: "UserSubscriptionEntitlements");

            migrationBuilder.DropColumn(
                name: "monthly_limit",
                table: "UserSubscriptionEntitlements");

            migrationBuilder.DropColumn(
                name: "monthly_limit",
                table: "subscriptionplanfeatures");

            migrationBuilder.DropColumn(
                name: "limit_unit",
                table: "CompanySubscriptionEntitlements");

            migrationBuilder.DropColumn(
                name: "monthly_limit",
                table: "CompanySubscriptionEntitlements");

            migrationBuilder.AddColumn<int>(
                name: "monthly_limit",
                table: "FeaturesCatalogs",
                type: "int",
                nullable: true);
        }
    }
}
