using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fusion.Repository.Migrations
{
    /// <inheritdoc />
    public partial class addfieldsdatasetprojectcomponents : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProjectComponents_Project",
                table: "ProjectComponents");

            migrationBuilder.DropForeignKey(
                name: "FK_ProjectComponents_ProjectRequest",
                table: "ProjectComponents");

            migrationBuilder.AlterColumn<DateTime>(
                name: "created_at",
                table: "ProjectComponents",
                type: "datetime2(3)",
                precision: 3,
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2(3)",
                oldPrecision: 3,
                oldDefaultValueSql: "(sysutcdatetime())");

            migrationBuilder.AlterColumn<Guid>(
                name: "id",
                table: "ProjectComponents",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldDefaultValueSql: "(newid())");

            migrationBuilder.AddForeignKey(
                name: "FK_ProjectComponents_ProjectRequests_project_request_id",
                table: "ProjectComponents",
                column: "project_request_id",
                principalTable: "ProjectRequests",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ProjectComponents_Projects_project_id",
                table: "ProjectComponents",
                column: "project_id",
                principalTable: "Projects",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProjectComponents_ProjectRequests_project_request_id",
                table: "ProjectComponents");

            migrationBuilder.DropForeignKey(
                name: "FK_ProjectComponents_Projects_project_id",
                table: "ProjectComponents");

            migrationBuilder.AlterColumn<DateTime>(
                name: "created_at",
                table: "ProjectComponents",
                type: "datetime2(3)",
                precision: 3,
                nullable: false,
                defaultValueSql: "(sysutcdatetime())",
                oldClrType: typeof(DateTime),
                oldType: "datetime2(3)",
                oldPrecision: 3);

            migrationBuilder.AlterColumn<Guid>(
                name: "id",
                table: "ProjectComponents",
                type: "uniqueidentifier",
                nullable: false,
                defaultValueSql: "(newid())",
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddForeignKey(
                name: "FK_ProjectComponents_Project",
                table: "ProjectComponents",
                column: "project_id",
                principalTable: "Projects",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_ProjectComponents_ProjectRequest",
                table: "ProjectComponents",
                column: "project_request_id",
                principalTable: "ProjectRequests",
                principalColumn: "id");
        }
    }
}
