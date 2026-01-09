using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fusion.Repository.Migrations
{
    /// <inheritdoc />
    public partial class Cleanup_ProjectComponent_ShadowColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
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
    }
}
