using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fusion.Repository.Migrations
{
    /// <inheritdoc />
    public partial class AddMaintenanceFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "is_maintenance",
                table: "Projects",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "maintenance_for_project_id",
                table: "Projects",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_maintenance",
                table: "ProjectRequests",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "maintenance_for_project_id",
                table: "ProjectRequests",
                type: "uniqueidentifier",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "is_maintenance",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "maintenance_for_project_id",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "is_maintenance",
                table: "ProjectRequests");

            migrationBuilder.DropColumn(
                name: "maintenance_for_project_id",
                table: "ProjectRequests");
        }
    }
}
