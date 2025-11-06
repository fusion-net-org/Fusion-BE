using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fusion.Repository.Migrations
{
    /// <inheritdoc />
    public partial class Fix_Table_SubscriptionPlan_l2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_subscriptionplanprices_plan_id",
                table: "subscriptionplanprices");

            migrationBuilder.AlterColumn<int>(
                name: "billing_period",
                table: "subscriptionplanprices",
                type: "int",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20);

            migrationBuilder.CreateIndex(
                name: "IX_subscriptionplanprices_plan_id",
                table: "subscriptionplanprices",
                column: "plan_id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_subscriptionplanprices_plan_id",
                table: "subscriptionplanprices");

            migrationBuilder.AlterColumn<string>(
                name: "billing_period",
                table: "subscriptionplanprices",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldMaxLength: 20);

            migrationBuilder.CreateIndex(
                name: "IX_subscriptionplanprices_plan_id",
                table: "subscriptionplanprices",
                column: "plan_id");
        }
    }
}
