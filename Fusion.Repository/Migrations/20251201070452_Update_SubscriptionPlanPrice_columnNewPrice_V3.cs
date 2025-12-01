using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fusion.Repository.Migrations
{
    /// <inheritdoc />
    public partial class Update_SubscriptionPlanPrice_columnNewPrice_V3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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

            migrationBuilder.AddColumn<bool>(
                name: "auto_grant_monthly",
                table: "SubscriptionPlans",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "new_price",
                table: "SubscriptionPlanPrices",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

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
                name: "auto_grant_monthly",
                table: "SubscriptionPlans");

            migrationBuilder.DropColumn(
                name: "new_price",
                table: "SubscriptionPlanPrices");

            migrationBuilder.DropColumn(
                name: "monthly_limit",
                table: "subscriptionplanfeatures");

            migrationBuilder.DropColumn(
                name: "limit_unit",
                table: "CompanySubscriptionEntitlements");

            migrationBuilder.DropColumn(
                name: "monthly_limit",
                table: "CompanySubscriptionEntitlements");
        }
    }
}
