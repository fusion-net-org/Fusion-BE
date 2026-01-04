using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fusion.Repository.Migrations
{
    /// <inheritdoc />
    public partial class adddatasetcomponents : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
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

            migrationBuilder.AddColumn<Guid>(
                name: "ProjectId1",
                table: "ProjectComponents",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ProjectRequestId1",
                table: "ProjectComponents",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProjectComponents_ProjectId1",
                table: "ProjectComponents",
                column: "ProjectId1");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectComponents_ProjectRequestId1",
                table: "ProjectComponents",
                column: "ProjectRequestId1");

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

            migrationBuilder.AddForeignKey(
                name: "FK_ProjectComponents_ProjectRequests_ProjectRequestId1",
                table: "ProjectComponents",
                column: "ProjectRequestId1",
                principalTable: "ProjectRequests",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_ProjectComponents_Projects_ProjectId1",
                table: "ProjectComponents",
                column: "ProjectId1",
                principalTable: "Projects",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProjectComponents_Project",
                table: "ProjectComponents");

            migrationBuilder.DropForeignKey(
                name: "FK_ProjectComponents_ProjectRequest",
                table: "ProjectComponents");

            migrationBuilder.DropForeignKey(
                name: "FK_ProjectComponents_ProjectRequests_ProjectRequestId1",
                table: "ProjectComponents");

            migrationBuilder.DropForeignKey(
                name: "FK_ProjectComponents_Projects_ProjectId1",
                table: "ProjectComponents");

            migrationBuilder.DropIndex(
                name: "IX_ProjectComponents_ProjectId1",
                table: "ProjectComponents");

            migrationBuilder.DropIndex(
                name: "IX_ProjectComponents_ProjectRequestId1",
                table: "ProjectComponents");

            migrationBuilder.DropColumn(
                name: "ProjectId1",
                table: "ProjectComponents");

            migrationBuilder.DropColumn(
                name: "ProjectRequestId1",
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
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_ProjectComponents_Projects_project_id",
                table: "ProjectComponents",
                column: "project_id",
                principalTable: "Projects",
                principalColumn: "id");
        }
    }
}
