using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fusion.Repository.Migrations
{
    /// <inheritdoc />
    public partial class AddRequesterExecutorColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "executor_company_id",
                table: "Contracts",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "requester_company_id",
                table: "Contracts",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Contracts_executor_company_id",
                table: "Contracts",
                column: "executor_company_id");

            migrationBuilder.CreateIndex(
                name: "IX_Contracts_requester_company_id",
                table: "Contracts",
                column: "requester_company_id");

            migrationBuilder.AddForeignKey(
                name: "FK_Contracts_Companies_executor_company_id",
                table: "Contracts",
                column: "executor_company_id",
                principalTable: "Companies",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_Contracts_Companies_requester_company_id",
                table: "Contracts",
                column: "requester_company_id",
                principalTable: "Companies",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Contracts_Companies_executor_company_id",
                table: "Contracts");

            migrationBuilder.DropForeignKey(
                name: "FK_Contracts_Companies_requester_company_id",
                table: "Contracts");

            migrationBuilder.DropIndex(
                name: "IX_Contracts_executor_company_id",
                table: "Contracts");

            migrationBuilder.DropIndex(
                name: "IX_Contracts_requester_company_id",
                table: "Contracts");

            migrationBuilder.DropColumn(
                name: "executor_company_id",
                table: "Contracts");

            migrationBuilder.DropColumn(
                name: "requester_company_id",
                table: "Contracts");
        }
    }
}
