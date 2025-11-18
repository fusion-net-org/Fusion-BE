using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fusion.Repository.Migrations
{
    /// <inheritdoc />
    public partial class Update_Table_SubscriptionPlan_V3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_subscriptionplanfeatures_FeaturesCatalogs_FeatureId1",
                table: "subscriptionplanfeatures");

            migrationBuilder.DropIndex(
                name: "IX_subscriptionplanfeatures_FeatureId1",
                table: "subscriptionplanfeatures");

            migrationBuilder.DropColumn(
                name: "FeatureId1",
                table: "subscriptionplanfeatures");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "FeatureId1",
                table: "subscriptionplanfeatures",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_subscriptionplanfeatures_FeatureId1",
                table: "subscriptionplanfeatures",
                column: "FeatureId1");

            migrationBuilder.AddForeignKey(
                name: "FK_subscriptionplanfeatures_FeaturesCatalogs_FeatureId1",
                table: "subscriptionplanfeatures",
                column: "FeatureId1",
                principalTable: "FeaturesCatalogs",
                principalColumn: "id");
        }
    }
}
