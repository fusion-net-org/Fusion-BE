using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fusion.Repository.Migrations
{
    /// <inheritdoc />
    public partial class Update_Table_SubscriptionPlan_V2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SubscriptionPlanFeature_Feature",
                table: "subscriptionplanfeatures");

            migrationBuilder.DropForeignKey(
                name: "FK_SubscriptionPlanFeature_Plan",
                table: "subscriptionplanfeatures");

            migrationBuilder.DropForeignKey(
                name: "FK_SubscriptionPlanPrice_Plan",
                table: "SubscriptionPlanPrices");

            migrationBuilder.DropForeignKey(
                name: "FK_SubscriptionPlans_SubscriptionPlanPrices_PricesId",
                table: "SubscriptionPlans");

            migrationBuilder.DropIndex(
                name: "IX_SubscriptionPlans_PricesId",
                table: "SubscriptionPlans");

            migrationBuilder.DropIndex(
                name: "IX_SubscriptionPlanPrices_plan_id",
                table: "SubscriptionPlanPrices");

            migrationBuilder.DropIndex(
                name: "UX_SubscriptionPlanFeature_Unique",
                table: "subscriptionplanfeatures");

            migrationBuilder.DropIndex(
                name: "UX_Features_Code_Unique",
                table: "FeaturesCatalogs");

            migrationBuilder.DropColumn(
                name: "PricesId",
                table: "SubscriptionPlans");

            migrationBuilder.AlterColumn<string>(
                name: "license_scope",
                table: "SubscriptionPlans",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<bool>(
                name: "is_full_package",
                table: "SubscriptionPlans",
                type: "bit",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<bool>(
                name: "is_active",
                table: "SubscriptionPlans",
                type: "bit",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<string>(
                name: "payment_mode",
                table: "SubscriptionPlanPrices",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "charge_unit",
                table: "SubscriptionPlanPrices",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "billing_period",
                table: "SubscriptionPlanPrices",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<bool>(
                name: "enabled",
                table: "subscriptionplanfeatures",
                type: "bit",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AddColumn<Guid>(
                name: "FeatureId1",
                table: "subscriptionplanfeatures",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "is_active",
                table: "FeaturesCatalogs",
                type: "bit",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.CreateIndex(
                name: "UX_SubscriptionPlanPrices_Plan",
                table: "SubscriptionPlanPrices",
                column: "plan_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_subscriptionplanfeatures_FeatureId1",
                table: "subscriptionplanfeatures",
                column: "FeatureId1");

            migrationBuilder.CreateIndex(
                name: "IX_subscriptionplanfeatures_plan_id",
                table: "subscriptionplanfeatures",
                column: "plan_id");

            migrationBuilder.CreateIndex(
                name: "UX_FeaturesCatalogs_Code",
                table: "FeaturesCatalogs",
                column: "code",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_SubscriptionPlanFeatures_Feature",
                table: "subscriptionplanfeatures",
                column: "feature_id",
                principalTable: "FeaturesCatalogs",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SubscriptionPlanFeatures_Plan",
                table: "subscriptionplanfeatures",
                column: "plan_id",
                principalTable: "SubscriptionPlans",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_subscriptionplanfeatures_FeaturesCatalogs_FeatureId1",
                table: "subscriptionplanfeatures",
                column: "FeatureId1",
                principalTable: "FeaturesCatalogs",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_SubscriptionPlanPrices_Plan",
                table: "SubscriptionPlanPrices",
                column: "plan_id",
                principalTable: "SubscriptionPlans",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SubscriptionPlanFeatures_Feature",
                table: "subscriptionplanfeatures");

            migrationBuilder.DropForeignKey(
                name: "FK_SubscriptionPlanFeatures_Plan",
                table: "subscriptionplanfeatures");

            migrationBuilder.DropForeignKey(
                name: "FK_subscriptionplanfeatures_FeaturesCatalogs_FeatureId1",
                table: "subscriptionplanfeatures");

            migrationBuilder.DropForeignKey(
                name: "FK_SubscriptionPlanPrices_Plan",
                table: "SubscriptionPlanPrices");

            migrationBuilder.DropIndex(
                name: "UX_SubscriptionPlanPrices_Plan",
                table: "SubscriptionPlanPrices");

            migrationBuilder.DropIndex(
                name: "IX_subscriptionplanfeatures_FeatureId1",
                table: "subscriptionplanfeatures");

            migrationBuilder.DropIndex(
                name: "IX_subscriptionplanfeatures_plan_id",
                table: "subscriptionplanfeatures");

            migrationBuilder.DropIndex(
                name: "UX_FeaturesCatalogs_Code",
                table: "FeaturesCatalogs");

            migrationBuilder.DropColumn(
                name: "FeatureId1",
                table: "subscriptionplanfeatures");

            migrationBuilder.AlterColumn<int>(
                name: "license_scope",
                table: "SubscriptionPlans",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<bool>(
                name: "is_full_package",
                table: "SubscriptionPlans",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<bool>(
                name: "is_active",
                table: "SubscriptionPlans",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: true);

            migrationBuilder.AddColumn<Guid>(
                name: "PricesId",
                table: "SubscriptionPlans",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "payment_mode",
                table: "SubscriptionPlanPrices",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20);

            migrationBuilder.AlterColumn<int>(
                name: "charge_unit",
                table: "SubscriptionPlanPrices",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20);

            migrationBuilder.AlterColumn<int>(
                name: "billing_period",
                table: "SubscriptionPlanPrices",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20);

            migrationBuilder.AlterColumn<bool>(
                name: "enabled",
                table: "subscriptionplanfeatures",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: true);

            migrationBuilder.AlterColumn<bool>(
                name: "is_active",
                table: "FeaturesCatalogs",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: true);

            migrationBuilder.CreateIndex(
                name: "IX_SubscriptionPlans_PricesId",
                table: "SubscriptionPlans",
                column: "PricesId");

            migrationBuilder.CreateIndex(
                name: "IX_SubscriptionPlanPrices_plan_id",
                table: "SubscriptionPlanPrices",
                column: "plan_id");

            migrationBuilder.CreateIndex(
                name: "UX_SubscriptionPlanFeature_Unique",
                table: "subscriptionplanfeatures",
                columns: new[] { "plan_id", "feature_id" },
                unique: true,
                filter: "([plan_id] IS NOT NULL AND [feature_id] IS NOT NULL)");

            migrationBuilder.CreateIndex(
                name: "UX_Features_Code_Unique",
                table: "FeaturesCatalogs",
                column: "code",
                unique: true,
                filter: "([code] IS NOT NULL)");

            migrationBuilder.AddForeignKey(
                name: "FK_SubscriptionPlanFeature_Feature",
                table: "subscriptionplanfeatures",
                column: "feature_id",
                principalTable: "FeaturesCatalogs",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SubscriptionPlanFeature_Plan",
                table: "subscriptionplanfeatures",
                column: "plan_id",
                principalTable: "SubscriptionPlans",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SubscriptionPlanPrice_Plan",
                table: "SubscriptionPlanPrices",
                column: "plan_id",
                principalTable: "SubscriptionPlans",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SubscriptionPlans_SubscriptionPlanPrices_PricesId",
                table: "SubscriptionPlans",
                column: "PricesId",
                principalTable: "SubscriptionPlanPrices",
                principalColumn: "id");
        }
    }
}
