using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fusion.Repository.Migrations
{
    /// <inheritdoc />
    public partial class AddFieldscontract : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Contracts_ProjectRequests_project_request_id",
                table: "Contracts");

            migrationBuilder.DropIndex(
                name: "IX_Contracts_project_request_id",
                table: "Contracts");

            migrationBuilder.DropColumn(
                name: "project_request_id",
                table: "Contracts");

            migrationBuilder.AddColumn<Guid>(
                name: "contract_id",
                table: "ProjectRequests",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "reason",
                table: "Contracts",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "status",
                table: "Contracts",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectRequests_contract_id",
                table: "ProjectRequests",
                column: "contract_id",
                unique: true,
                filter: "[contract_id] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_ProjectRequests_Contracts_contract_id",
                table: "ProjectRequests",
                column: "contract_id",
                principalTable: "Contracts",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProjectRequests_Contracts_contract_id",
                table: "ProjectRequests");

            migrationBuilder.DropIndex(
                name: "IX_ProjectRequests_contract_id",
                table: "ProjectRequests");

            migrationBuilder.DropColumn(
                name: "contract_id",
                table: "ProjectRequests");

            migrationBuilder.DropColumn(
                name: "reason",
                table: "Contracts");

            migrationBuilder.DropColumn(
                name: "status",
                table: "Contracts");

            migrationBuilder.AddColumn<Guid>(
                name: "project_request_id",
                table: "Contracts",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_Contracts_project_request_id",
                table: "Contracts",
                column: "project_request_id",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Contracts_ProjectRequests_project_request_id",
                table: "Contracts",
                column: "project_request_id",
                principalTable: "ProjectRequests",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
