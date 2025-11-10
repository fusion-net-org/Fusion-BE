using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fusion.Repository.Migrations
{
    /// <inheritdoc />
    public partial class UpdateProjectHiredFieldUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Projects_HiredCompany",
                table: "Projects");

            migrationBuilder.DropIndex(
                name: "IX_Projects_CompanyHiredId",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "CompanyHiredId",
                table: "Projects");

            migrationBuilder.CreateIndex(
                name: "IX_Projects_company_request_id",
                table: "Projects",
                column: "company_request_id");

            migrationBuilder.AddForeignKey(
                name: "FK_Projects_HiredCompany",
                table: "Projects",
                column: "company_request_id",
                principalTable: "Companies",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Projects_HiredCompany",
                table: "Projects");

            migrationBuilder.DropIndex(
                name: "IX_Projects_company_request_id",
                table: "Projects");

            migrationBuilder.AddColumn<Guid>(
                name: "CompanyHiredId",
                table: "Projects",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Projects_CompanyHiredId",
                table: "Projects",
                column: "CompanyHiredId");

            migrationBuilder.AddForeignKey(
                name: "FK_Projects_HiredCompany",
                table: "Projects",
                column: "CompanyHiredId",
                principalTable: "Companies",
                principalColumn: "id");
        }
    }
}
