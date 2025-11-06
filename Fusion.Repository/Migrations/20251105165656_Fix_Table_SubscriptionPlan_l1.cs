using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fusion.Repository.Migrations
{
    /// <inheritdoc />
    public partial class Fix_Table_SubscriptionPlan_l1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "is_customizable",
                table: "subscriptionplans");

            migrationBuilder.DropColumn(
                name: "create_at",
                table: "subscriptionplanprices");

            migrationBuilder.DropColumn(
                name: "updated_at",
                table: "subscriptionplanprices");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "is_customizable",
                table: "subscriptionplans",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "create_at",
                table: "subscriptionplanprices",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "(sysutcdatetime())");

            migrationBuilder.AddColumn<DateTime>(
                name: "updated_at",
                table: "subscriptionplanprices",
                type: "datetime2",
                nullable: true);
        }
    }
}
