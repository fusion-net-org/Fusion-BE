using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fusion.Repository.Migrations
{
    /// <inheritdoc />
    public partial class AddUpdateByfieldsintoprojectrequest : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "updated_by",
                table: "ProjectRequests",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProjectRequests_updated_by",
                table: "ProjectRequests",
                column: "updated_by");

            migrationBuilder.AddForeignKey(
                name: "FK_ProjectRequests_Users_updated_by",
                table: "ProjectRequests",
                column: "updated_by",
                principalTable: "Users",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProjectRequests_Users_updated_by",
                table: "ProjectRequests");

            migrationBuilder.DropIndex(
                name: "IX_ProjectRequests_updated_by",
                table: "ProjectRequests");

            migrationBuilder.DropColumn(
                name: "updated_by",
                table: "ProjectRequests");
        }
    }
}
